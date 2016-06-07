namespace xr
{
    public interface ILineColorProvider
    {
        uint GetLineColor(string line, out int hiddenCharCount);
    }
}
