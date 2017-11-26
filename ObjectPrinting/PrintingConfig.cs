using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private IEnumerable<Type> ExcludedTypes { get; }
        private IEnumerable<string> ExcludedProperties { get; }
        private IDictionary<Type, CultureInfo> CulturesForNumbersTypes { get; }
        private IDictionary<Type, Delegate> TypesPrinters { get; }
        private IDictionary<string, Delegate> PropetiesPrinters { get; }
        private IDictionary<string, Func<string, string>> StringPropertiesTrimmer { get; }
        private string selectedProperty;

        private PrintingConfig(IEnumerable<Type> ExcludedTypes, IEnumerable<string> ExcludedProperties,
             IDictionary<Type, Delegate> TypesPrinters, IDictionary<string, Delegate> PropetiesPrinters,
             IDictionary<Type, CultureInfo> culturesForNumbersTypes, IDictionary<string, Func<string, string>> stringPropertiesTrimmer)
        {
            this.ExcludedTypes = ExcludedTypes;
            this.ExcludedProperties = ExcludedProperties;
            this.TypesPrinters = TypesPrinters;
            this.PropetiesPrinters = PropetiesPrinters;
            CulturesForNumbersTypes = culturesForNumbersTypes;
            StringPropertiesTrimmer = stringPropertiesTrimmer;
            selectedProperty = string.Empty;
        }

        public PrintingConfig() : this(new HashSet<Type>(), new HashSet<string>(),
            new Dictionary<Type, Delegate>(), new Dictionary<string, Delegate>(),
            new Dictionary<Type, CultureInfo>(), new Dictionary<string, Func<string, string>>())
        {
        }

        public PropertyPrintingConfig<TOwner, TProp> ForProperty<TProp>(Expression<Func<TOwner, TProp>> serializer)
        {
            var propertyInfo = ((MemberExpression)serializer.Body).Member as PropertyInfo;
            selectedProperty = propertyInfo?.Name;
            return new PropertyPrintingConfig<TOwner, TProp>(this);
        }

        public PropertyPrintingConfig<TOwner, TProp> ForType<TProp>()
        {
            return new PropertyPrintingConfig<TOwner, TProp>(this);
        }

        private PrintingConfig<TOwner> SetSerializerFor<TProp>(Expression<Func<TProp, string>> serializer)
        {
            var func = serializer.Compile();
            var typesPrinters = new Dictionary<Type, Delegate>(TypesPrinters);
            var propertyPrinters = new Dictionary<string, Delegate>(PropetiesPrinters);
            if (string.IsNullOrEmpty(selectedProperty))
                typesPrinters[typeof(TProp)] = func;
            else
                propertyPrinters[selectedProperty] = func;
            return new PrintingConfig<TOwner>(ExcludedTypes, ExcludedProperties,
                typesPrinters, propertyPrinters, CulturesForNumbersTypes, StringPropertiesTrimmer);
        }

        private PrintingConfig<TOwner> SetCulture<TProp>(CultureInfo culture)
        {
            var culturesForNumbersTypes = new Dictionary<Type, CultureInfo>(CulturesForNumbersTypes)
            {
                [typeof(TProp)] = culture
            };
            return new PrintingConfig<TOwner>(ExcludedTypes, ExcludedProperties, TypesPrinters,
                PropetiesPrinters, culturesForNumbersTypes, StringPropertiesTrimmer);
        }

        private PrintingConfig<TOwner> SetStringTrimmer(int count)
        {
            if (count < 0)
                throw new ArgumentException("Index should be a positive number");

            var x = Expression.Parameter(typeof(string));
            var methodInfo = typeof(string).GetMethod("Substring", new[] { typeof(int) });
            var stringPropertiesTrimmer = new Dictionary<string, Func<string, string>>(StringPropertiesTrimmer);
            var trimmer = Expression.Lambda<Func<string, string>>(
                          Expression.Call(x, methodInfo,
                          Expression.Constant(count)), x)
                          .Compile();
            if (string.IsNullOrEmpty(selectedProperty))
            {
                var properties = typeof(TOwner).GetProperties().Select(p => p.Name);
                foreach (var propName in properties)
                    stringPropertiesTrimmer[propName] = trimmer;
            }
            else
                stringPropertiesTrimmer[selectedProperty] = trimmer;
            selectedProperty = string.Empty;
            return new PrintingConfig<TOwner>(ExcludedTypes, ExcludedProperties, TypesPrinters, PropetiesPrinters,
                CulturesForNumbersTypes, stringPropertiesTrimmer);
        }

        public PrintingConfig<TOwner> Exclude<TProp>()
        {
            return new PrintingConfig<TOwner>(ExcludedTypes.Concat(new[] { typeof(TProp) }), ExcludedProperties,
                TypesPrinters, PropetiesPrinters, CulturesForNumbersTypes, StringPropertiesTrimmer);
        }

        public PrintingConfig<TOwner> Exclude<TProp>(Expression<Func<TOwner, TProp>> serializer)
        {
            var propertyInfo = ((MemberExpression)serializer.Body).Member as PropertyInfo;
            return new PrintingConfig<TOwner>(ExcludedTypes, ExcludedProperties.Concat(new[] { propertyInfo?.Name }),
                TypesPrinters, PropetiesPrinters, CulturesForNumbersTypes, StringPropertiesTrimmer);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            var simpleObjectPrint = TrySerializeSimpleObject(obj);
            if (simpleObjectPrint != null)
                return simpleObjectPrint;

            var identation = new string('\t', nestingLevel + 1);
            var type = obj.GetType();

            var sb = new StringBuilder(type.Name + Environment.NewLine);

            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyToPrint = SerializeProperty(obj, propertyInfo, nestingLevel);
                if (propertyToPrint == null) continue;
                sb.Append(identation + propertyInfo.Name + " = " + propertyToPrint);
            }
            return sb.ToString();
        }

        private string TrySerializeSimpleObject(object obj)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

            if (obj is int || obj is double || obj is float)
            {
                var culture = CulturesForNumbersTypes.ContainsKey(type)
                    ? CulturesForNumbersTypes[type]
                    : CultureInfo.InvariantCulture;
                return obj.ToString().ToLower(culture) + Environment.NewLine;
            }

            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;
            return null;
        }

        private string SerializeProperty(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            var propertyType = propertyInfo.PropertyType;
            var propertyValue = propertyInfo.GetValue(obj);
            var propertyName = propertyInfo.Name;

            if (ExcludedTypes.Contains(propertyType) || ExcludedProperties.Contains(propertyName))
                return null;

            if (propertyType == typeof(string))
                propertyValue = Serialize(propertyValue, StringPropertiesTrimmer[propertyName]);

            if (PropetiesPrinters.ContainsKey(propertyName))
                return Serialize(propertyValue, PropetiesPrinters[propertyName]) + Environment.NewLine;

            if (TypesPrinters.ContainsKey(propertyType))
                return Serialize(propertyValue, TypesPrinters[propertyType]) + Environment.NewLine;

            return PrintToString(propertyValue, nestingLevel + 1);
        }

        private static string Serialize<TProp>(TProp value, Delegate serializer)
        {
            return serializer.DynamicInvoke(value).ToString();
        }
    }
}