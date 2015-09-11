xrConsole
=========

Simple and fast S.T.A.L.K.E.R. server like console control written in C#
---

![Screenshot](https://raw.githubusercontent.com/nitrocaster/xrConsole/master/screenshot.png)

**Features:**

* Text-based log coloring
* Adjustable log and command history buffers
* Scrolling by mouse wheel and PageUp/PageDown
* Built-in command system with autocompletion
* Full-featured command editor:
  * Ctrl+X: cut
  * Ctrl+C: copy to clipboard
  * Ctrl+V: paste from clipboard
  * Ctrl+A: select all
  * Del/Ctrl+Del: erase right character/word
  * Backspace/Ctrl+Backspace: erase left character/word
  * Home: move cursor to the beginning of line
  * End: move cursor to the end of line
  * Insert: toggle edit/insert mode
  * Ctrl enables fast navigation move (almost like in any text editor)
  * Shift enables selection mode
  * Tab/Shift+Tab shows next/previous available command
* Header block for any static/dynamic data that have to be always visible
* Adjustable font
* Can be easily integrated into any Windows Forms application

**Known problems:**

* No smooth scrolling (does anyone need it?)
* Limited color customization: log line color is detected by its first characters
* Log and command line can't be selected with mouse (would be really useful)

Bugs
---
Any bug reports and pull requests appreciated. Open an issue [here](https://github.com/nitrocaster/xrConsole/issues).

Credits:
---

Me - nitrocaster

GSC Game World for the great game and its bugs :)
