using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtension
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, int> config,
            CultureInfo culture)
        {
            return config.PrintingConfig.SetCulture<int>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, long> config, CultureInfo culture)
        {
            return config.PrintingConfig.SetCulture<long>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, double> config, CultureInfo culture)
        {
            return config.PrintingConfig.SetCulture<double>(culture);
        }

        public static PrintingConfig<TOwner> CutTo<TOwner>(
            this PropertyPrintingConfig<TOwner, string> config, int count)
        {
            return config.PrintingConfig.SetStringTrimmer(count);
        }
    }
}
