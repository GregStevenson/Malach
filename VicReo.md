Getting Started with VICREO Listener.
VICREO Listener is a small program that sits on your machine waiting for incoming TCP connection/commands. It uses pre-defined commands to simulate key-presses on your machine. You can use this program to preform hotkey actions from remote

Controllable by Companion VICREO-Listener can be controlled by Companion, select the instance: VICREO-Hotkey for that
How does it work
Download and install VICREO-Listener app for your OS (Windows/macOS/Linux)
Check for the log in the app to see on which IP addresses the listener will react
Installation
Download the right software package for your operating system (Windows, macOS, or Linux) and launch the program on the machine you would like to control (host).

On your client machine (the one you are sending commands from), send TCP string to the right IP-address from the host, to port 10001 (or change the port to something you like)

Issues?
If you have any issues or suggestions, please create one over at GitHub: https://github.com/bitfocus/companion-module-vicreo-hotkey/issues

Usage
You’ll send an object to the listener. The application first looks at a key called

"type"
.
The following types are available;

press (simulate a keypress)
pressSpecial (special keys)
combination (simulate 2 keys)
trio (simulate 3 keys)
quartet (simulate 4 keys)
down (simulate a key down)
up (simulate a key up)
processOSX (send keys to a process on mac via AppleScript)
string (type a string)
shell (perform a shell command)
file (open a file)
mousePosition (set the mouse cursor to a position)
mouseClick (simulate mouse clicks - single, double, left, right)
mouseClickHold (hold down a mouse button)
mouseClickRelease (release a held mouse button)
mouseScroll (scroll the mouse wheel vertically and horizontally)
getMousePosition (retrieve the x/y position of the mouse cursor)
setWindowToForeGround (bring a window to foreground - Windows only)
subscribe (allow to subscribe to certain information)
mousePosition
unsubscribe
mousePosition
Password
As from version 3.0.0 its needed to at a password option when sending your object. When in the listener the password is not filled, you can send an empty password. When a password is filled, you will need to Hash the password (MD5 hash) so the listener can check that the object is valid.

To support older connection methods, leave the password empty in the Listener.

Example key press
For keypresses create a object like this;

{ "key":"c", "type":"press", "password":"d41d8cd98f00b204e9800998ecf8427e" }
Example key combination
{ "key":"tab", "type":"combination", "modifiers":["alt"], "password":"d41d8cd98f00b204e9800998ecf8427e" }
Example key trio
{ "key":"left", "type":"trio", "modifiers":["ctrl","shift"], "password":"d41d8cd98f00b204e9800998ecf8427e" }
Put the modifiers in an array.
Alt/option, command(win), Ctrl and Shift are supported.

Example processOSX
{ "key":"tab", "type":"processOSX","processName":"Microsoft PowerPoint", "modifiers":["alt"], "password":"d41d8cd98f00b204e9800998ecf8427e"}
Example string message
{ "type":"string","msg":"C:/Barco/InfoT1413.pdf", "password":"d41d8cd98f00b204e9800998ecf8427e" }
Example open file
Open a file on the local system;

{ "type":"file","path":"C:/Barco/InfoT1413.pdf", "password":"d41d8cd98f00b204e9800998ecf8427e"}
Example shell
To perform a shell command on the system;

{ "type":"shell","shell":"dir", "password":"d41d8cd98f00b204e9800998ecf8427e"}
Example set mouse position
To set the mouse to a certain place

{ "type":"mousePosition","x":"500","y":"500", "password":"d41d8cd98f00b204e9800998ecf8427e"}
Example mouse click
Click the mouse, provide button (left, right), double (true for double click)

{ "type":"mouseClick","button":"left","double":"false", "password":"d41d8cd98f00b204e9800998ecf8427e"}
Example mouse click hold
Hold down a mouse button (useful for drag operations)

{ "type":"mouseClickHold","button":"left", "password":"d41d8cd98f00b204e9800998ecf8427e"}
Example mouse click release
Release a held mouse button

{ "type":"mouseClickRelease","button":"left", "password":"d41d8cd98f00b204e9800998ecf8427e"}
Example mouse scroll
Scroll the mouse wheel vertically and horizontally

{ "type":"mouseScroll","vertical":"3","horizontal":"0", "password":"d41d8cd98f00b204e9800998ecf8427e"}
Example set window to foreground (Windows only)
Bring a specific window to the foreground

{ "type":"setWindowToForeGround","windowTitle":"Notepad", "password":"d41d8cd98f00b204e9800998ecf8427e"}
Example subscription
Subscribe to certain events

{ "type":"subscribe","name":"mousePosition", "password":"d41d8cd98f00b204e9800998ecf8427e"}
Example unsubscribe
Unsubscribe to certain events

{ "type":"unsubscribe","name":"mousePosition", "password":"d41d8cd98f00b204e9800998ecf8427e"}
Keys
The following keys are supported:

Key Description Notes
backspace Forward delete (delete key on extended keyboards)
delete Backspace key
enter Return/Enter key (main keyboard)
tab Tab key
escape Escape key
up Up arrow key
down Down arrow key
right Right arrow key
left Left arrow key
home Home key
end End key
pageup Page Up key
pagedown Page Down key
f1 Function key F1
f2 Function key F2
f3 Function key F3
f4 Function key F4
f5 Function key F5
f6 Function key F6
f7 Function key F7
f8 Function key F8
f9 Function key F9
f10 Function key F10
f11 Function key F11
f12 Function key F12
f13 Function key F13
f14 Function key F14
f15 Function key F15
f16 Function key F16
f17 Function key F17
f18 Function key F18
f19 Function key F19
f20 Function key F20
command Command key (⌘)
alt Option/Alt key
right_alt Right Option key
control Control key
right_ctrl Right Control key
shift Shift key
right_shift Right Shift key
caps_lock Caps Lock key
fn Function (fn) modifier key
space Space bar
printscreen Print Screen key No Mac support
insert Insert key No Mac support
pause Pause/Break key Windows only
scrolllock Scroll Lock key Windows only
numlock Num Lock key Windows only
win Windows key / Start key Windows only
leftwindows Left Windows key Windows only
rightwindows Right Windows key Windows only
keypaddecimal Keypad Decimal (.)
keypadmultiply Keypad Multiply (_)
keypadplus Keypad Plus (+)
keypadclear Keypad Clear
keypaddivide Keypad Divide (/)
keypadenter Keypad Enter
keypadminus Keypad Minus (-)
keypadequals Keypad Equals (=)
numpad_0 Numpad 0 No Linux support
numpad_1 Numpad 1 No Linux support
numpad_2 Numpad 2 No Linux support
numpad_3 Numpad 3 No Linux support
numpad_4 Numpad 4 No Linux support
numpad_5 Numpad 5 No Linux support
numpad_6 Numpad 6 No Linux support
numpad_7 Numpad 7 No Linux support
numpad_8 Numpad 8 No Linux support
numpad_9 Numpad 9 No Linux support
leftmouse Left mouse button Windows only
rightmouse Right mouse button Windows only
middlemouse Middle mouse button Windows only
x1mouse Extra mouse button 1 Windows only
x2mouse Extra mouse button 2 Windows only
comma Comma (,) Windows only
asterisk Asterisk (_) Windows only
plus Plus (+) Windows only
pipe Pipe (|) Windows only
minus Minus (-) Windows only
period Period (.) Windows only
slash Forward slash (/) Windows only
backslash Backslash (\) Windows only
audio_mute Mute the volume
audio_vol_down Lower the volume
audio_vol_up Increase the volume
audio_play Play
audio_stop Stop
audio_pause Pause
audio_prev Previous Track
audio_next Next Track
audio_rewind Rewind (fast backward) Linux only
audio_forward Fast Forward Linux only
audio_repeat Repeat Linux only
audio_random Random (Shuffle) Linux only
stop Stop media playback Windows only
play Play media Windows only
pause Pause media Windows only
launchpad Launchpad key (opens Launchpad)
missioncontrol Mission Control key (opens Mission Control)
lights_mon_down Monitor brightness down Alias for F14 on some keyboards
lights_mon_up Monitor brightness up Alias for F15 on some keyboards
Complete your gift to make an impact
