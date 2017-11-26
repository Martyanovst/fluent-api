using System;
using System.Linq.Expressions;
using System.Reflection;

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
            var type = typeof(PrintingConfig<TOwner>);
            var method = type.GetMethod("SetSerializerFor", BindingFlags.Instance | BindingFlags.NonPublic)?
                             .MakeGenericMethod(typeof(TProp));
            return (PrintingConfig<TOwner>)method?.Invoke(PrintingConfig, new object[] { serializer });
        }

        public PrintingConfig<TOwner> PrintingConfig { get; }
    }
}
