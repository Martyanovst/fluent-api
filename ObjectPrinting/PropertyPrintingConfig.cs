using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProp> : PrintingConfig<TOwner>, IPropertyConfig<TOwner, TProp>
    {
        public PropertyPrintingConfig(PrintingConfig<TOwner> config)
        {
            PrintingConfig = config;
        }

        public PrintingConfig<TOwner> Using(Expression<Func<TProp, string>> serializer)
        {
            return PrintingConfig.SetSerializer(serializer);
        }

        public PrintingConfig<TOwner> PrintingConfig { get; }
    }
}
