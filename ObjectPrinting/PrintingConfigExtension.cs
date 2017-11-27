using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtension
    {
        private static PrintingConfig<TOwner> SetCulture<TOwner, TProp>(PrintingConfig<TOwner> config, CultureInfo culture)
        {
            var type = typeof(PrintingConfig<TOwner>);
            var method = type.GetMethod("SetCulture", BindingFlags.Instance | BindingFlags.NonPublic)?
                             .MakeGenericMethod(typeof(TProp));
            return (PrintingConfig<TOwner>)method?
                    .Invoke(config, new object[] { culture });
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, int> config,
            CultureInfo culture)
        {
            return SetCulture<TOwner, int>(config.PrintingConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, long> config, CultureInfo culture)
        {
            return SetCulture<TOwner, long>(config.PrintingConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, double> config, CultureInfo culture)
        {
            return SetCulture<TOwner, double>(config.PrintingConfig, culture);
        }

        public static PrintingConfig<TOwner> CutTo<TOwner>(
            this PropertyPrintingConfig<TOwner, string> config, int count)
        {
            var type = typeof(PrintingConfig<TOwner>);
            var method = type.GetMethod("SetStringTrimmer", BindingFlags.Instance | BindingFlags.NonPublic);

            return (PrintingConfig<TOwner>)method?
                    .Invoke(config.PrintingConfig, new object[] { count });
        }
    }
}
