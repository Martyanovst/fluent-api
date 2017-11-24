using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public static class ObjectExtension
    {
        public static string PrintToString<TOwner>(this TOwner obj)
        {
            return ObjectPrinter.For<TOwner>().PrintToString(obj);
        }

        public static string PrintToString<TOwner>(this TOwner obj, Expression<Func<PrintingConfig<TOwner>,
                                                   PrintingConfig<TOwner>>> serializer)
        {
            var printer = serializer.Compile();
            return printer(ObjectPrinter.For<TOwner>()).PrintToString(obj);
        }
    }
}
