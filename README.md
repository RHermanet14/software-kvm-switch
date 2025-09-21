Project overview:
A software kvm switch that uses C# and TCP sockets to send mouse and keyboard inputs from one machine to another

Features:
- Add up to 4 servers either to the top, bottom, left, or right of the main monitor
- Simple interface to easily modify connection settings
- Clipboard sharing
- Server info preference saving

How to run:
1. Navigate to the "Client" folder on your main device. Locate "KvmSwitch.ClientUI.exe". Either run the file directly from the folder or create a desktop shortcut for it.
2. Navigate to the "Server" folder on up to four other devices. Locate "KvmSwitch.ServerUI.exe". Either run the file directly from the folder or create a desktop shortcut for it.
3. The server interface will have a toggleable button to display the IP. In the client, click "Add New Server", then enter this IP into the first box. Then, enter the desired port (Default: 11111), the desired margin (recommended: 1), then whatever direction the server display is to the client. Repeat process with multiple servers and click "Save Preferences".
4. Click "Start Client" in the client and "Start Server" in the server. Move your mouse to the specified edge of the screen and it should appear on the server side!
5. Click "Stop Client" or "Stop Server" when you want that side of the connection to close.

If I were to do it all over again:
- I would use C or C++ instead of C# so it would be easier to make a Linux version for functionality across different operating systems
- Use only low level procs instead of raw input devices. The raw input device for the keyboard did not work for the low level keyboard proc, so I put the event handler in the keyboard proc and it worked fine. Assuming there is a function to get the hardware movement of the mouse (I didn't check because it was already working), I would remove the raw input device for the mouse to simplify the process of getting mouse inputs.