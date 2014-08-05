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

        public void Delete()
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
                else // delete one char from the right
                {
                    buffer.Remove(cursorPos, 1);
                }
            }
            ResetSelection();
        }

        public void Backspace()
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
                else // delete one char from the left
                {
                    buffer.Remove(cursorPos - 1, 1);
                    MoveCaretLeft(false);
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

        public void MoveCaretLeft(bool shift)
        {
            MoveCaret(-1, shift);
        }

        public void MoveCaretRight(bool shift)
        {
            MoveCaret(1, shift);
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
            else // amount < 0
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
                if (amount <= cursorPos)
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
