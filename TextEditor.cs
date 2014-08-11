using System;
using System.Text;
using System.Windows.Forms;

namespace xr
{
    public sealed class TextEditor
    {
        public struct SelectionRange
        {
            public int StartIndex;
            public int Length;

            public SelectionRange(int startIndex, int length)
            {
                StartIndex = startIndex;
                Length = length;
            }
        }

        private StringBuilder buffer;
        private SelectionRange selection;
        private int cursorPos;

        public TextEditor(int capacity)
        {
            selection = new SelectionRange(0, 0);
            buffer = new StringBuilder(capacity);
        }

        public StringBuilder Buffer
        {
            get { return buffer; }
        }

        public SelectionRange Selection
        {
            get { return selection; }
        }

        public int CursorPos
        {
            get { return cursorPos; }
        }

        public void Reset()
        {
            cursorPos = 0;
            ResetSelection();
            buffer.Clear();
        }

        public void ResetSelection()
        {
            selection.StartIndex = cursorPos;
            selection.Length = 0;
        }

        public string Text
        {
            get
            {
                return buffer.ToString();
            }
            set
            {
                Reset();
                buffer.Append(value);
                End(false);
            }
        }

        public void Insert(string s)
        {
            if (selection.Length == 1)
            {
                Delete();
                buffer.Insert(cursorPos, s);
            }
            else if (selection.Length != 0)
            {
                Delete();
                buffer.Insert(cursorPos, s);
                MoveCaretRight(false);
            }
            else
            {
                buffer.Insert(cursorPos, s);
                MoveCaretRight(false);
            }
            ResetSelection();
        }

        public void Insert(char c)
        {
            if (selection.Length == 1)
            {
                Delete();
                buffer.Insert(cursorPos, c);
            }
            else if (selection.Length != 0)
            {
                Delete();
                buffer.Insert(cursorPos, c);
                MoveCaretRight(false);
            }
            else
            {
                buffer.Insert(cursorPos, c);
                MoveCaretRight(false);
            }
            ResetSelection();
        }

        public void Delete(bool ctrl = false)
        {
            var bufferLength = buffer.Length;
            if (cursorPos == bufferLength && selection.Length == 0)
            {
                // rightmost position and nothing selected => nothing to delete
                return;
            }
            if (bufferLength > 0)
            {
                if (selection.Length != 0)
                {
                    cursorPos = selection.StartIndex;
                    buffer.Remove(selection.StartIndex, selection.Length);
                }
                else // delete from the right
                {
                    var endIndex = ctrl ? BreakTest(1) : cursorPos + 1;
                    buffer.Remove(cursorPos, endIndex - cursorPos);
                }
            }
            ResetSelection();
        }

        public void Backspace(bool ctrl = false)
        {
            if (cursorPos == 0 && selection.Length == 0)
            {
                // leftmost position and nothing selected => nothing to delete
                return;
            }
            if (buffer.Length > 0)
            {
                if (selection.Length != 0)
                {
                    cursorPos = selection.StartIndex;
                    buffer.Remove(selection.StartIndex, selection.Length);
                }
                else // delete from the left
                {
                    var startIndex = ctrl ? BreakTest(-1) : cursorPos - 1;
                    var length = cursorPos - startIndex;
                    MoveCaret(-length, false);
                    buffer.Remove(startIndex, length);
                }
            }
            ResetSelection();
        }

        public void SelectAll()
        {
            var bufferLength = buffer.Length;
            selection.StartIndex = 0;
            selection.Length = bufferLength;
            cursorPos = bufferLength;
        }

        public void Copy()
        {
            if (selection.Length > 0)
            {
                Clipboard.SetText(buffer.ToString().Substring(selection.StartIndex, selection.Length));
            }
        }

        public void Paste()
        {
            if (!Clipboard.ContainsText())
            {
                return;
            }
            if (selection.Length > 0)
            {
                Delete();
            }
            var text = Clipboard.GetText();
            buffer.Insert(cursorPos, text);
            cursorPos += text.Length;
            ResetSelection();
        }

        public void Cut()
        {
            if (selection.Length <= 0)
            {
                return;
            }
            Copy();
            Delete();
        }

        public void Home(bool shift)
        {
            MoveCaret(-cursorPos, shift);
        }

        public void End(bool shift)
        {
            MoveCaret(buffer.Length - cursorPos, shift);
        }

        public void MoveCaretLeft(bool shift, bool ctrl = false)
        {
            var offset = -1;
            if (ctrl)
            {
                offset = BreakTest(-1) - cursorPos;
            }
            MoveCaret(offset, shift);
        }

        public void MoveCaretRight(bool shift, bool ctrl = false)
        {
            var offset = 1;
            if (ctrl)
            {
                offset = BreakTest(1) - cursorPos;
            }
            MoveCaret(offset, shift);
        }

        private int BreakTest(int dir)
        {
            const string breakChars = "., \"~`%^&*_+-=/|\\#$()[]{}<>:;!?";
            Predicate<char> isBreakChar = c => breakChars.IndexOf(c) >= 0;
            if (buffer.Length == 0)
            {
                return 0;
            }
            var pos = cursorPos;
            if (dir < 0)
            {
                if (pos <= 1)
                {
                    return 0;
                }
                var prevPos = pos;
                while (pos >= 1 && !isBreakChar(buffer[pos - 1]))
                {
                    pos--;
                }
                if (pos != prevPos)
                {
                    return pos;
                }
                while (pos >= 1 && isBreakChar(buffer[pos - 1]))
                {
                    pos--;
                }
                while (pos >= 1 && !isBreakChar(buffer[pos - 1]))
                {
                    pos--;
                }
            }
            else
            {
                if (pos >= buffer.Length - 1)
                {
                    return buffer.Length;
                }
                while (pos < buffer.Length && !isBreakChar(buffer[pos]))
                {
                    pos++;
                }
                while (pos < buffer.Length && isBreakChar(buffer[pos]))
                {
                    pos++;
                }
            }
            return pos;
        }

        private void MoveCaret(int amount, bool shift)
        {
            if (amount > 0)
            {
                if (shift) // expand/shrink selection by <amount> chars to the right
                {
                    var prevCursorPos = cursorPos;
                    amount = InternalMoveCaret(amount);
                    if (amount > 0)
                    {
                        if (prevCursorPos >= selection.StartIndex + selection.Length)
                        {
                            selection.Length += amount;
                        }
                        else if (amount > selection.Length)
                        {
                            selection.StartIndex += selection.Length;
                            selection.Length = amount - selection.Length;
                        }
                        else
                        {
                            selection.StartIndex += amount;
                            selection.Length -= amount;
                        }
                    }
                }
                else
                {
                    if (cursorPos == selection.StartIndex)
                    {
                        // jump over selection
                        InternalMoveCaret(selection.Length);
                    }
                    if (amount > selection.Length)
                    {
                        InternalMoveCaret(amount - selection.Length);
                    }
                    ResetSelection();
                }
            }
            else if (amount < 0)
            {
                if (shift) // expand/shrink selection by <amount> chars to the left
                {
                    var prevCursorPos = cursorPos;
                    amount = -InternalMoveCaret(amount);
                    if (amount > 0)
                    {
                        if (prevCursorPos <= selection.StartIndex)
                        {
                            selection.StartIndex -= amount;
                            selection.Length += amount;
                        }
                        else if (amount > selection.Length)
                        {
                            selection.StartIndex -= amount - selection.Length;
                            selection.Length = amount - selection.Length;
                        }
                        else
                        {
                            selection.Length -= amount;
                        }
                    }
                }
                else
                {
                    if (cursorPos == selection.StartIndex + selection.Length)
                    {
                        // jump over selection
                        InternalMoveCaret(-selection.Length);
                    }
                    if (-amount > selection.Length)
                    {
                        InternalMoveCaret(amount - selection.Length);
                    }
                    ResetSelection();
                }
            }
            else
            {
                ResetSelection();
            }
        }
        
        private int InternalMoveCaret(int amount)
        {
            if (amount > 0) // move right
            {
                if (cursorPos == buffer.Length)
                {
                    return 0;
                }
                if (amount <= buffer.Length - cursorPos)
                {
                    cursorPos += amount;
                    return amount;
                }
                else
                {
                    cursorPos = buffer.Length;
                    return buffer.Length - cursorPos;
                }
            }
            else // move left
            {
                if (cursorPos == 0)
                {
                    return 0;
                }
                if (cursorPos + amount >= 0)
                {
                    cursorPos += amount;
                    return amount;
                }
                else
                {
                    amount = cursorPos;
                    cursorPos = 0;
                    return amount;
                }
            }
        }
    }
}
