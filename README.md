# DOT-NET-COMS-LIB
Hexabitz .Net Communication Library




The following documentation is about connecting and interacting with Hexabitz modules using .Net framework.

The library contains two classes:

**Message.cs**

Which will represent the message array class to be sent to Hexabitz modules that contains the required bytes ( H | Z | Length | Destination | Source | Options | Code | Par. 1 | Par. 2 | ------- | CRC8) where (Par. 1 | Par. 2 | -------)  are the parameters for the payload.
For more info about the Array messaging visit: https://hexabitz.com/docs/code-overview/array-messaging/

The class contains the following methods:

 	

* `Public Message (byte Destination, byte Source, byte Options, int Code, byte[] Payload)`   
The main constructor for the message with the parameters:

Source and Destination are IDs of source and destination modules.
Options: is a byte that contains some option bits and flags. 
Code: Is a 16-bit variable representing Message Codes.
Payload: The data we want to send to the Hexabitz modules.

And for each Message there is a CRC in the last byte to check the integrity of the message array.

 	

* `public byte GetCRC()`: Method to calculate the CRC for the message array.
 	

* `public byte[] GetAll()`: Method to get the full message bytes.
 	

* `private List<byte> Organize(List<byte> MesssageList)`: Private method to organize the message bytes in the correct order as described [here](https://hexabitz.com/docs/code-overview/array-messaging/ "https://hexabitz.com/docs/code-overview/array-messaging/") in **Calculating and Comparing CRC Codes** section.
 	

* `private byte CRC32B(byte[] Buffer)`: Algorithm used in the Hexabitz modules hardware to calculate the correct CRC32 but we are only using the first byte in our modules.



    


**HexInterface.cs**

Which will represent the Interface between the Message Class and the platform we are going to connect the Hexabitz modules with here we are using .Net and C# programming language.


The class contains the following methods:
* `public HexaInterface(string COM)`: To initialize an instance providing COM number only.
* `Start()` and `End()` to open and close the Serial port.
* `public void SendMessage(byte Destination, byte Source, byte Options, int Code, byte[] Message)`: 
The parameters are:
Source and Destination are IDs of source and destination modules.
Options: is a byte that contains some option bits and flags.
Message: The data we want to send to Hexabitz modules in the correct order according to the functionality.
* `private void Receive()`: The reviceing method to listen to the port if we got any response from it.
* `private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)`: Method the .Net platform provides to respond to the received data from the SerialPort.
* `private string to_right_hex(string hex)`: To correct the values received from the port.

And also contains the message codes enum to specify the codes to be used in the communication.

