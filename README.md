Project overview:
A software kvm switch that uses C# and TCP sockets to send mouse coordinates from one machine to another

Desired Features:
- Send mouse and keyboard inputs to other device when move mouse off main device's screen
- Remote tab sharing in firefox (Marionette?)
- Desktop application UI to determine which side of main screen to move off of in order to make it appear on the other screen.
- Other stuff as I come up with it

Client:
- Recieve inputs and use them

Server:
- Send inputs to client(s) depending on which end of screen mouse is on

UI:
- Setting IP / Port
- Have visual representation of screen placement (e.g. putting laptop on left side of monitor has the right side of the laptop screen connected to the left side of the monitor)