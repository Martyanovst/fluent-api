using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProp> : IPropertyConfig<TOwner, TProp>
    {
        public PropertyPrintingConfig(PrintingConfig<TOwner> config)
        {
            PrintingConfig = config;
        }

        public PrintingConfig<TOwner> Using(Expression<Func<TProp, string>> serializer)
        {
            return PrintingConfig.SetSerializerFor(serializer);
        }

        public PrintingConfig<TOwner> PrintingConfig { get; }
    }
}
