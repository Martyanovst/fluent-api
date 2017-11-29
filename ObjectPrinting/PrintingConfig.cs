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

        public PrintingConfig() : this(new HashSet<Type>(), new HashSet<string>(),
           new Dictionary<Type, Delegate>(), new Dictionary<string, Delegate>(),
           new Dictionary<Type, CultureInfo>(), new Dictionary<string, Func<string, string>>())
        {
        }

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
            var typeOfObject = typeof(TOwner);
            return TypesPrinters.ContainsKey(typeOfObject)
                   ? Serialize(obj, TypesPrinters[typeOfObject])
                   : PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            var simpleObjectPrint = TrySerializeSimpleObject(obj);
            if (simpleObjectPrint != null)
                return simpleObjectPrint;

            var identation = new string('\t', nestingLevel + 1);
            var type = obj.GetType();

            var stringBuilder = new StringBuilder(type.Name + Environment.NewLine);

            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyToPrint = SerializeProperty(obj, propertyInfo, nestingLevel);
                if (propertyToPrint == null) continue;
                stringBuilder.Append(identation + propertyInfo.Name + " = " + propertyToPrint);
            }
            return stringBuilder.ToString();
        }

        private string TrySerializeSimpleObject(object obj)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan),typeof(Guid)
            };

            if (obj is int || obj is double || obj is long)
            {
                var culture = CulturesForNumbersTypes.ContainsKey(type)
                              ? CulturesForNumbersTypes[type]
                              : CultureInfo.InvariantCulture;
                return obj.ToString().ToString(culture.NumberFormat) + Environment.NewLine;
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
                propertyValue = StringPropertiesTrimmer.ContainsKey(propertyName)
                              ? Serialize(propertyValue, StringPropertiesTrimmer[propertyName])
                              : propertyValue;

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

        internal PrintingConfig<TOwner> SetSerializerFor<TProp>(Expression<Func<TProp, string>> serializer)
        {
            var printingFunction = serializer.Compile();
            var typesPrinters = new Dictionary<Type, Delegate>(TypesPrinters);
            var propertyPrinters = new Dictionary<string, Delegate>(PropetiesPrinters);

            if (string.IsNullOrEmpty(selectedProperty))
                typesPrinters[typeof(TProp)] = printingFunction;
            else
                propertyPrinters[selectedProperty] = printingFunction;

            return new PrintingConfig<TOwner>(ExcludedTypes, ExcludedProperties,
                typesPrinters, propertyPrinters, CulturesForNumbersTypes, StringPropertiesTrimmer);
        }

        internal PrintingConfig<TOwner> SetCulture<TProp>(CultureInfo culture)
        {
            var culturesForNumbersTypes = new Dictionary<Type, CultureInfo>(CulturesForNumbersTypes)
            {
                [typeof(TProp)] = culture
            };
            return new PrintingConfig<TOwner>(ExcludedTypes, ExcludedProperties, TypesPrinters,
                   PropetiesPrinters, culturesForNumbersTypes, StringPropertiesTrimmer);
        }

        internal PrintingConfig<TOwner> SetStringTrimmer(int count)
        {
            if (count < 0)
                throw new ArgumentException("Count should be a positive number");

            Func<string, string> trimmer = x => x.Substring(count);

            var stringPropertiesTrimmer = string.IsNullOrEmpty(selectedProperty)
                                        ? typeof(TOwner).GetProperties()
                                        .Where(x => x.PropertyType == typeof(string))
                                        .Select(p => p.Name)
                                        .ToDictionary(name => name, value => trimmer)
                                        : new Dictionary<string, Func<string, string>>(StringPropertiesTrimmer)
                                        {
                                            [selectedProperty] = trimmer
                                        };

            selectedProperty = string.Empty;

            return new PrintingConfig<TOwner>(ExcludedTypes, ExcludedProperties, TypesPrinters, PropetiesPrinters,
                                              CulturesForNumbersTypes, stringPropertiesTrimmer);
        }
    }
}