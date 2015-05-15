namespace xr
{
    public interface ILinePrefixProvider
    {
        string CommandInput { get; }
        string UnknownCommand { get; }
        string DisabledCommand { get; }
        string CommandListing { get; }
        string CommandStatus { get; }
        string InvalidSyntax { get; }
    }
}
