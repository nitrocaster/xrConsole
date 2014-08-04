using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Text;
using XrConsoleProject.ConsoleCommands;

namespace XrConsoleProject
{
    public sealed class XrConsole : XrConsoleBase
    {
        private const int CommandQueueSize = 16;
        private const int ListenerExitTimeout = 5000;
        private readonly object syncExec = new object();
        private Queue<string> commandQueue;
        private TrieNode<char> commandTrie;
        private SortedList<string, ConsoleCommand> commands;
        private Thread listener;
        private volatile bool listenInput = true;
        private bool tabKeyState = false;

        public XrConsole()
            : this(null)
        {
        }

        public XrConsole(ILogger logger)
            : base(logger)
        {
            commandQueue = new Queue<string>(CommandQueueSize);
            commandTrie = new TrieNode<char>();
            commands = new SortedList<string, ConsoleCommand>();
            AddCommand(new StringFunc("help", Console_Help, 255, "Print help"));
            listener = Utils.CreateThread(ConsoleListenerProc, "ConsoleListener");
            Initialized = true;
        }
        
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!IsDisposed)
                {
                    if (disposing)
                    {
                        listenInput = false;
                        if (Thread.CurrentThread != listener)
                        {
                            listener.Join(ListenerExitTimeout);
                            if (listener.IsAlive)
                            {
                                listener.Abort();
                            }
                        }
                        int commandsCount = commands.Count;
                        for (int i = 0; i < commandsCount; i++)
                        {
                            RemoveCommand(commands.Keys[0]);
                        }
                    }
                    DisposeHelper.OnDispose(disposing, "XrConsole");
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        
        public bool Initialized
        {
            get;
            private set;
        }

        private void ConsoleListenerProc()
        {
            while (listenInput)
            {
                lock (syncExec)
                {
                    Monitor.Wait(syncExec);
                }
                Execute(commandQueue.Dequeue());
            }
        }

        protected override void OnCommand(string command)
        {
            commandQueue.Enqueue(command);
            lock (syncExec)
            {
                Monitor.Pulse(syncExec);
            }
        }

        public void Execute(string args)
        {
            ExecuteCommand(args, true);
        }

        public void ExecuteCommand(string args, bool log)
        {
            args = args.Trim();
            if (log)
            {
                LogCommand(args);
            }
            string cmdName = args.GetFirstArg();
            string cmdArgs;
            if (cmdName == null)
            {
                cmdName = args;
                cmdArgs = String.Empty;
            }
            else
            {
                cmdArgs = args.Substring(cmdName.Length).Trim();
            }
            var command = GetCommandByName(cmdName);
            if (command == null)
            {
                Program.Msg("! Unknown command: " + cmdName);
                return;
            }
            if (!command.Flags.HasFlag(ConsoleCommandFlags.Enabled))
            {
                Program.Msg("! Command disabled.");
                return;
            }
            if (command.Flags.HasFlag(ConsoleCommandFlags.Variable))
            {
                if (cmdArgs.Length == 0)
                {
                    Program.Msg("- {0} {1}", command.Name, command.Status);
                    return;
                }
            }
            command.Execute(cmdArgs);
        }

        private void Console_Help(string args)
        {
            if (args.Length > 0)
            {
                var cmd = GetCommandByName(args);
                if (cmd == null)
                {
                    Program.Msg("! Unknown command: " + args);
                    return;
                }
                PrintCommandInfo(cmd);
                return;
            }
            Program.Msg("- --- Command listing start ---");
            foreach (var cmd in commands.Values)
            {
                PrintCommandInfo(cmd);
            }
            Program.Msg("- --- Command listing end ---");
        }

        private static void PrintCommandInfo(ConsoleCommand cmd)
        {
            Program.Msg("{0}  {1}<{2}> {3}",
                cmd.Name,
                (cmd.Status != null) ? String.Format("( {0} )  ", cmd.Status) : String.Empty,
                cmd.Args,
                cmd.Info);
        }

        private ConsoleCommand GetCommandByName(string commandName)
        {
            return commands.ContainsKey(commandName) ?
                commands[commandName] : null;
        }

        private void LogCommand(string args)
        {
            CmdCache.Push(args);
            Program.Msg("@ " + args);
        }
        
        public void AddCommand(ConsoleCommand command)
        {
            if (commands.ContainsKey(command.Name))
            {
                throw new InvalidOperationException("Attempt to add existent command");
            }
            commands.Add(command.Name, command);
            InternalAddCommand(commandTrie, command.Name, command.Name.Length, 0);
        }

        public void RemoveCommand(ConsoleCommand command)
        {
            if (!commands.ContainsKey(command.Name))
            {
                throw new InvalidOperationException("Attempt to remove nonexistent command");
            }
            RemoveCommand(command.Name);
        }

        private void RemoveCommand(string command)
        {
            commands.Remove(command);
            InternalRemoveCommand(commandTrie, command, command.Length, 0);
        }

        public string FindNextCmd(string command)
        {
            int i = commands.IndexOfKey(command);
            if (i == commands.Count - 1)
            {
                return commands.Keys[0];
            }
            if (i > -1)
            {
                return commands.Keys[i + 1];
            }
            var result = new StringBuilder("", 256);
            InternalFindNextCmd(commandTrie, command, command.Length, 0, ref result);
            return result.ToString();
        }

        public string FindPrevCmd(string command)
        {
            int i = commands.IndexOfKey(command);
            if (i == 0)
            {
                return commands.Keys[commands.Count - 1];
            }
            if (i > 0)
            {
                return commands.Keys[i - 1];
            }
            var result = new StringBuilder(256);
            InternalFindPrevCmd(commandTrie, command, command.Length, 0, ref result);
            return result.ToString();
        }
        
        private void InternalFindNextCmd(TrieNode<char> node, string chars, int length, int index, ref StringBuilder result)
        {
            if (index == length)
            {
                if (commands.ContainsKey(chars))
                {
                    result.Append(chars);
                    return;
                }
                goto get_nearest;
            }
            char c = chars[index];
            index++;
            if (node.Children.ContainsKey(c))
            {
                InternalFindNextCmd(node.Children[c], chars, length, index, ref result);
                return;
            }
            index--;
            // exact matching sequence processed, get the nearest string from trie
            get_nearest:
            var validKey = new StringBuilder(chars.Substring(0, index), 256);
            int offset = 0;
            while (node.Children != null)
            {
                var key = '\0';
                if (index + offset < length)
                {
                    for (int i = 0; i < node.Children.Count; i++)
                    {
                        var childKey = node.Children.Keys[i];
                        var srcKey = chars[index + offset];
                        if (childKey > srcKey)
                        {
                            key = childKey;
                        }
                    }
                }
                if (key == '\0')
                {
                    key = node.Children.Keys[0];
                }
                if (key != '\0')
                {
                    validKey.Append(key);
                    node = node.Children[key];
                    offset++;
                    continue;
                }
                result = validKey;
                return;
            }
            throw new InvalidOperationException("Nonzero leaf node found");
        }

        private void InternalFindPrevCmd(TrieNode<char> node, string chars, int length, int index, ref StringBuilder result)
        {
            if (index == length)
            {
                if (commands.ContainsKey(chars))
                {
                    result.Append(chars);
                    return;
                }
                goto get_nearest;
            }
            var c = chars[index];
            index++;
            if (node.Children.ContainsKey(c))
            {
                InternalFindPrevCmd(node.Children[c], chars, length, index, ref result);
                return;
            }
            index--;
            // exact matching sequence processed, get the nearest string from trie
            get_nearest:
            var validKey = new StringBuilder(chars.Substring(0, index), 256);
            int indexOffset = 0;
            while (node.Children != null)
            {
                var key = '\0';
                if (index + indexOffset < length)
                {
                    for (int i = 0; i < node.Children.Count; i++)
                    {
                        var childKey = node.Children.Keys[i];
                        var srcKey = chars[index + indexOffset];
                        if (childKey < srcKey)
                        {
                            key = childKey;
                        }
                    }
                }
                if (key == '\0')
                {
                    key = node.Children.Keys[node.Children.Count - 1];
                }
                if (key != '\0')
                {
                    validKey.Append(key);
                    node = node.Children[key];
                    indexOffset++;
                    continue;
                }
                result = validKey;
                return;
            }
            throw new InvalidOperationException("Nonzero leaf node found");
        }

        private void InternalAddCommand(TrieNode<char> node, string chars, int length, int index)
        {
            if (node.Children == null)
            {
                node.Children = new SortedList<char, TrieNode<char>>();
            }
            if (length == index)
            {
                node.Children.Add('\0', null);
                return;
            }
            var c = chars[index];
            index++;
            TrieNode<char> newNode;
            if (node.Children.ContainsKey(c))
            {
                newNode = node.Children[c];
            }
            else
            {
                newNode = new TrieNode<char>();
                node.Children.Add(c, newNode);
            }
            InternalAddCommand(newNode, chars, length, index);
        }

        private bool InternalRemoveCommand(TrieNode<char> node, string chars, int length, int index)
        {
            // move to the end of the corresponding branch,
            // then iterate back to root and remove childless nodes
            if (length == index)
            {
                if (node.Children.ContainsKey('\0'))
                {
                    node.Children.Remove('\0');
                }
                else
                {
                    throw new InvalidOperationException("Attempt to remove nonexisting key");
                }
                if (node.Children.Count > 0)
                {
                    return true; // removal finished
                }
                if (node.Children.Count == 0)
                {
                    node.Children = null;
                    return false; // continue removal from the end of branch
                }
            }
            var c = chars[index];
            index++;
            var result = InternalRemoveCommand(node.Children[c], chars, length, index);
            if (!result)
            {
                node.Children.Remove(c);
                if (node.Children.Count == 0)
                {
                    node.Children = null;
                }
                else
                {
                    result = true;
                }
            }
            return result;
        }
        
        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Tab:
                case Keys.Shift | Keys.Tab:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            bool shiftKey = e.Modifiers.HasFlag(Keys.Shift);
            switch (e.KeyCode)
            {
                case Keys.Tab:
                    if (tabKeyState)
                    {
                        break;
                    }
                    tabKeyState = true;
                    e.Handled = true;
                    var editorText = Editor.Text.Trim();
                    Editor.Text = !shiftKey ? FindNextCmd(editorText) : FindPrevCmd(editorText);
                    return;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Tab:
                    tabKeyState = false;
                    e.Handled = true;
                    break;
            }
            base.OnKeyUp(e);
        }
    }
}
