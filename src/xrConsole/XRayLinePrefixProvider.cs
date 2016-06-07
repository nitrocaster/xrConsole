namespace xr
{
    public sealed class XRayLinePrefixProvider : ILinePrefixProvider
    {
        public string CommandInput { get { return "@ "; } }
        public string DisabledCommand { get { return "! "; } }
        public string UnknownCommand { get { return "! "; } }
        public string CommandListing { get { return "- "; } }
        public string CommandStatus { get { return "- "; } }
        public string InvalidSyntax { get { return "~ "; } }
    }
}
