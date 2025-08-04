Project overview:
A software kvm switch that uses C# and TCP sockets to send mouse coordinates from one machine to another

Desired Features:
- Send mouse and keyboard inputs to other device when move mouse off main device's screen
- Remote tab sharing in firefox (Marionette?)
- Desktop application UI to determine which side of main screen to move off of in order to make it appear on the other screen.
- Other stuff as I come up with it

Client:
- Uses .NET Secret Manager (for now) to store IP
- Send inputs to server(s) depending on which end of screen mouse is on
- Uses RawInputDevice to get mouse (and eventually keyboard) inputs
- Send this pair (of floats) through a TCP socket

Server:
- Recieve inputs and use them
- Interpret data recieved through sockets as real inputs

UI:
- Setting IP / Port
- Have visual representation of screen placement (e.g. putting laptop on left side of monitor has the right side of the laptop screen connected to the left side of the monitor)