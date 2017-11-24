using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtension
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, int> config,
            CultureInfo culture)
        {
            return config.PrintingConfig.SetCulture(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, long> config, CultureInfo culture)
        {
            return config.PrintingConfig.SetCulture(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, double> config, CultureInfo culture)
        {
            return config.PrintingConfig.SetCulture(culture);
        }

        public static PrintingConfig<TOwner> CuttingBy<TOwner>(
            this PropertyPrintingConfig<TOwner, string> config, int count)
        {
            if (count < 0)
                throw new ArgumentException("Index should be a positive number");

            var x = Expression.Parameter(typeof(string));
            var methodInfo = typeof(string).GetMethod("Substring", new[] { typeof(int) });
            var trimmer = Expression.Lambda<Func<string, string>>(
                         Expression.Call(x, methodInfo,
                         Expression.Constant(count)), x);
            return config.PrintingConfig.SetSerializer(trimmer);
        }
    }
}
