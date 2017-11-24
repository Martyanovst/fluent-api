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
        private CultureInfo Culture { get; }
        private IDictionary<Type, Delegate> TypesPrinters { get; }
        private IDictionary<string, Delegate> PropetiesPrinters { get; }
        private string selectedProperty;

        private PrintingConfig(IEnumerable<Type> ExcludedTypes, IEnumerable<string> ExcludedProperties,
             IDictionary<Type, Delegate> TypesPrinters, IDictionary<string, Delegate> PropetiesPrinters,
             CultureInfo culture)
        {
            this.ExcludedTypes = ExcludedTypes;
            this.ExcludedProperties = ExcludedProperties;
            this.TypesPrinters = TypesPrinters;
            this.PropetiesPrinters = PropetiesPrinters;
            Culture = culture;
            selectedProperty = string.Empty;
        }

        public PrintingConfig() : this(new HashSet<Type>(), new HashSet<string>(),
            new Dictionary<Type, Delegate>(), new Dictionary<string, Delegate>(),
            CultureInfo.InvariantCulture)
        {
        }

        public PropertyPrintingConfig<TOwner, TProp> Select<TProp>(Expression<Func<TOwner, TProp>> serializer)
        {
            var propertyInfo = ((MemberExpression)serializer.Body).Member as PropertyInfo;
            selectedProperty = propertyInfo?.Name;
            return new PropertyPrintingConfig<TOwner, TProp>(this);
        }

        public PropertyPrintingConfig<TOwner, TProp> For<TProp>()
        {
            return new PropertyPrintingConfig<TOwner, TProp>(this);
        }

        public PrintingConfig<TOwner> SetSerializer<TProp>(Expression<Func<TProp, string>> serializer)
        {
            var func = serializer.Compile();
            var typesPrinters = new Dictionary<Type, Delegate>(TypesPrinters);
            var propertyPrinters = new Dictionary<string, Delegate>(PropetiesPrinters);
            if (string.IsNullOrEmpty(selectedProperty))
                typesPrinters[typeof(TProp)] = func;
            else
                propertyPrinters[selectedProperty] = func;
            return new PrintingConfig<TOwner>(ExcludedTypes, ExcludedProperties,
                typesPrinters, propertyPrinters, Culture);
        }

        public PrintingConfig<TOwner> SetCulture(CultureInfo culture)
        {
            return new PrintingConfig<TOwner>(ExcludedTypes, ExcludedProperties, TypesPrinters,
                PropetiesPrinters, culture);
        }

        public PrintingConfig<TOwner> Exclude<TProp>()
        {
            return new PrintingConfig<TOwner>(ExcludedTypes.Concat(new[] { typeof(TProp) }), ExcludedProperties,
                TypesPrinters, PropetiesPrinters, Culture);
        }

        public PrintingConfig<TOwner> Exclude<TProp>(Expression<Func<TOwner, TProp>> serializer)
        {
            var propertyInfo = ((MemberExpression)serializer.Body).Member as PropertyInfo;
            return new PrintingConfig<TOwner>(ExcludedTypes, ExcludedProperties.Concat(new[] { propertyInfo?.Name }),
                TypesPrinters, PropetiesPrinters, Culture);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (obj is int || obj is double || obj is float)
                return obj.ToString().ToLower(Culture) + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                var propertyValue = propertyInfo.GetValue(obj);
                var propertyName = propertyInfo.Name;

                if (ExcludedTypes.Contains(propertyType) || ExcludedProperties.Contains(propertyName))
                    continue;

                string propertyToPrint;
                if (PropetiesPrinters.ContainsKey(propertyName))
                    propertyToPrint = Serialize(propertyValue, PropetiesPrinters[propertyName]) + Environment.NewLine;

                else if (TypesPrinters.ContainsKey(propertyType))
                    propertyToPrint = Serialize(propertyValue, TypesPrinters[propertyType]) + Environment.NewLine;

                else
                    propertyToPrint = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);

                sb.Append(identation + propertyInfo.Name + " = " + propertyToPrint);
            }
            return sb.ToString();
        }

        private static string Serialize<TProp>(TProp value, Delegate serializer)
        {
            return serializer.DynamicInvoke(value).ToString();
        }
    }
}