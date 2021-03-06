﻿using System;

namespace xr.ConsoleCommands
{
    public class BooleanVar : ConsoleCommand
    {
        protected Accessor<bool> Target;

        public BooleanVar(Console console, string name, Accessor<bool> target, string info = "")
            : base(console, name, info)
        {
            Target = target;
        }

        public override string Args
        {
            get { return "'on/off' or '1/0'"; }
        }

        public override string Status
        {
            get { return ArgConverter.ToString(Target.Get()); }
        }

        public override void Execute(string args)
        {
            bool val;
            var arg = args.GetFirstArg() ?? args;
            if (!ArgConverter.ParseBool(arg, out val))
            {
                InvalidSyntax();
                return;
            }
            Target.Set(val);
        }

        public override ConsoleCommandFlags Flags
        {
            get { return ConsoleCommandFlags.Enabled | ConsoleCommandFlags.Variable; }
        }
    }

    public class IntegerVar : ConsoleCommand
    {
        protected int Min;
        protected int Max;
        protected Accessor<int> Target;

        public IntegerVar(Console console, string name, Accessor<int> target, int min, int max, string info = "")
            : base(console, name, info)
        {
            Min = min;
            Max = max;
            Target = target;
        }

        public override string Args
        {
            get
            {
                return String.Format("integer value in range [{0},{1}]",
                    ArgConverter.ToString(Min), ArgConverter.ToString(Max));
            }
        }

        public override string Status
        {
            get { return ArgConverter.ToString(Target.Get()); }
        }

        public override void Execute(string args)
        {
            int val;
            var arg = args.GetFirstArg() ?? args;
            if (!ArgConverter.ParseInt32(arg, out val))
            {
                InvalidSyntax();
                return;
            }
            if (val > Max || val < Min)
            {
                InvalidSyntax();
                return;
            }
            Target.Set(val);
        }

        public override ConsoleCommandFlags Flags
        {
            get { return ConsoleCommandFlags.Enabled | ConsoleCommandFlags.Variable; }
        }
    }

    public class FloatVar : ConsoleCommand
    {
        protected float Min;
        protected float Max;
        protected Accessor<float> Target;

        public FloatVar(Console console, string name, Accessor<float> target, float min, float max, string info = "")
            : base(console, name, info)
        {
            Min = min;
            Max = max;
            Target = target;
        }

        public override string Args
        {
            get
            {
                return String.Format("float value in range [{0},{1}]",
                    ArgConverter.ToString(Min), ArgConverter.ToString(Max));
            }
        }

        public override string Status
        {
            get { return ArgConverter.ToString(Target.Get()); }
        }

        public override void Execute(string args)
        {
            float val;
            var arg = args.GetFirstArg() ?? args;
            if (!ArgConverter.ParseSingle(arg, out val))
            {
                InvalidSyntax();
                return;
            }
            if (val > Max || val < Min)
            {
                InvalidSyntax();
                return;
            }
            Target.Set(val);
        }

        public override ConsoleCommandFlags Flags
        {
            get { return ConsoleCommandFlags.Enabled | ConsoleCommandFlags.Variable; }
        }
    }

    public class StringVar : ConsoleCommand
    {
        protected int MaxLength;
        protected Accessor<string> Target;

        public StringVar(Console console, string name, Accessor<string> target, int maxLength, string info = "")
            : base(console, name, info)
        {
            MaxLength = maxLength;
            Target = target;
        }

        public override string Args
        {
            get { return String.Format("string up to {0} characters", ArgConverter.ToString(MaxLength)); }
        }

        public override string Status
        {
            get { return Target.Get(); }
        }

        public override void Execute(string args)
        {
            if (args.Length > MaxLength)
            {
                args = args.Substring(0, MaxLength);
            }
            Target.Set(args);
        }

        public override ConsoleCommandFlags Flags
        {
            get { return ConsoleCommandFlags.Enabled | ConsoleCommandFlags.Variable; }
        }
    }

    public class Func : ConsoleCommand
    {
        protected Action Handler;

        public Func(Console console, string name, Action handler, string info = "")
            : base(console, name, info)
        {
            Handler = handler;
        }

        public override void Execute(string args)
        {
            Handler();
        }

        public override ConsoleCommandFlags Flags
        {
            get { return ConsoleCommandFlags.Enabled | ConsoleCommandFlags.Function; }
        }
    }

    public class BooleanFunc : ConsoleCommand
    {
        protected Action<bool> Handler;

        public BooleanFunc(Console console, string name, Action<bool> handler, string info = "")
            : base(console, name, info)
        {
            Handler = handler;
        }

        public override string Args
        {
            get { return "'on/off' or '1/0'"; }
        }

        public override void Execute(string args)
        {
            bool val;
            var arg = args.GetFirstArg() ?? args;
            if (!ArgConverter.ParseBool(arg, out val))
            {
                InvalidSyntax();
                return;
            }
            Handler(val);
        }

        public override ConsoleCommandFlags Flags
        {
            get
            {
                return ConsoleCommandFlags.Enabled | ConsoleCommandFlags.Function |
                    ConsoleCommandFlags.ArgsRequired;
            }
        }
    }

    public class IntegerFunc : ConsoleCommand
    {
        protected int Min;
        protected int Max;
        protected Action<int> Handler;

        public IntegerFunc(Console console, string name, Action<int> handler, int min, int max, string info = "")
            : base(console, name, info)
        {
            Handler = handler;
            Min = min;
            Max = max;
        }

        public override string Args
        {
            get
            {
                return String.Format("integer value in range [{0},{1}]",
                    ArgConverter.ToString(Min), ArgConverter.ToString(Max));
            }
        }

        public override void Execute(string args)
        {
            int val;
            var arg = args.GetFirstArg() ?? args;
            if (!ArgConverter.ParseInt32(arg, out val))
            {
                InvalidSyntax();
                return;
            }
            if (val > Max || val < Min)
            {
                InvalidSyntax();
                return;
            }
            Handler(val);
        }

        public override ConsoleCommandFlags Flags
        {
            get
            {
                return ConsoleCommandFlags.Enabled | ConsoleCommandFlags.Function |
                    ConsoleCommandFlags.ArgsRequired;
            }
        }
    }

    public class FloatFunc : ConsoleCommand
    {
        protected float Min;
        protected float Max;
        protected Action<float> Handler;

        public FloatFunc(Console console, string name, Action<float> handler, float min, float max, string info = "")
            : base(console, name, info)
        {
            Handler = handler;
            Min = min;
            Max = max;
        }

        public override string Args
        {
            get
            {
                return String.Format("float value in range [{0},{1}]",
                    ArgConverter.ToString(Min), ArgConverter.ToString(Max));
            }
        }

        public override void Execute(string args)
        {
            float val;
            var arg = args.GetFirstArg() ?? args;
            if (!ArgConverter.ParseSingle(arg, out val))
            {
                InvalidSyntax();
                return;
            }
            if (val > Max || val < Min)
            {
                InvalidSyntax();
                return;
            }
            Handler(val);
        }

        public override ConsoleCommandFlags Flags
        {
            get
            {
                return ConsoleCommandFlags.Enabled | ConsoleCommandFlags.Function |
                    ConsoleCommandFlags.ArgsRequired;
            }
        }
    }

    public class StringFunc : ConsoleCommand
    {
        protected int MaxLength;
        protected Action<string> Handler;

        public StringFunc(Console console, string name, Action<string> handler, int maxLength, string info = "")
            : base(console, name, info)
        {
            Handler = handler;
            MaxLength = maxLength;
        }

        public override string Args
        {
            get { return String.Format("string up to {0} characters", ArgConverter.ToString(MaxLength)); }
        }

        public override void Execute(string args)
        {
            if (MaxLength > 0)
            {
                if (args.Length > MaxLength)
                {
                    args = args.Substring(0, MaxLength);
                }
            }
            Handler(args);
        }

        public override ConsoleCommandFlags Flags
        {
            get
            {
                return ConsoleCommandFlags.Enabled | ConsoleCommandFlags.Function |
                    ConsoleCommandFlags.ArgsRequired;
            }
        }
    }
}
