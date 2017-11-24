namespace ObjectPrinting
{
    public interface IPropertyConfig<TOwner, TProp>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
    }
}
