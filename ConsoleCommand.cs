using System;

namespace xr
{
    using CFlags = ConsoleCommandFlags;

    [Flags]
    public enum ConsoleCommandFlags : uint
    {
        None = 0,
        Enabled = 1,
        ArgsRequired = 2,
        ArgsOptional = 4,
        Function = 8,
        Variable = 16
    }

    public abstract class ConsoleCommand
    {
        protected ConsoleCommand(Console console, string name, string info = "")
        {
            Console = console;
            Name = name;
            Args = "no arguments";
            Info = info;
        }

        /// <summary>
        /// Returns Console instance assotiated with this ConsoleCommand.
        /// </summary>
        protected Console Console { get; private set; }

        /// <summary>
        /// Returns command flags.
        /// </summary>
        public virtual CFlags Flags
        {
            get { return CFlags.Enabled; }
        }

        /// <summary>
        /// Returns command name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Returns command arguments info.
        /// </summary>
        public virtual string Args { get; private set; }

        /// <summary>
        /// Returns command summary.
        /// </summary>
        public string Info { get; private set; }

        /// <summary>
        /// Returns command status.
        /// Valid for variables only.
        /// </summary>
        public virtual string Status
        {
            get { return null; }
        }

        /// <summary>
        /// Executes command.
        /// </summary>
        /// <param name="args"></param>
        public abstract void Execute(string args);

        protected void InvalidSyntax()
        {
            Console.Msg("~ Invalid syntax in call to '{0}'", Name);
            Console.Msg("~ Valid arguments: " + Args);
        }
    }
}
