# DOT-NET-COMS-LIB
Hexabitz .Net Communication Library




The following documentation is about connecting and interacting with Hexabitz modules using .Net framework.

The library contains two classes:

## Message.cs

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

 	

* `public byte GetCRC()`: Method to calculate the CRC (Cyclic redundancy check) for the message array.
 	

* `public byte[] GetAll()`: Method to get the full message bytes.
 	

* `private List<byte> Organize(List<byte> MesssageList)`: Private method to organize the message bytes in the correct order as described [here](https://hexabitz.com/docs/code-overview/array-messaging/ "https://hexabitz.com/docs/code-overview/array-messaging/") in **Calculating and Comparing CRC Codes** section.
 	

* `private byte CRC32B(byte[] Buffer)`: Algorithm used in the Hexabitz modules hardware to calculate the correct CRC32 but we are only using the first byte in our modules.


## HexInterface.cs

Which will represent the Interface between the Message Class and the platform we are going to connect the Hexabitz modules with here we are using .Net and C# programming language.


The class contains the following methods:
* `public HexaInterface(string COM)`: To initialize an instance providing COM number only.
* `Start()` and `End()`: to open and close the Serial port.
* `public byte Options()`: To create the options byte.
* `public void SendMessage(byte Destination, byte Source, byte Options, int Code, byte[] Payload)`: 
The parameters are:

Source and Destination are IDs of source and destination modules.

Options: is a byte that contains some option bits and flags.

Message: The data we want to send to Hexabitz modules in the correct order according to the functionality.

* `private void Receive()`: The receiving method to listen to the port if we got any response from it.
* `private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)`: Method the .Net platform provides to respond to the received data from the SerialPort.
* `private string to_right_hex(string hex)`: To correct the values received from the port.
* Region for the options byte enums in [Options](https://hexabitz.com/docs/code-overview/array-messaging/ "https://hexabitz.com/docs/code-overview/array-messaging/") section.

And also contains the message codes enum to specify the codes to be used in the communication.


# Usage

## Sending ##
The **Message** Class is the base class to create and the right message with its CRC but the **HexInterface** class can be modified according to the platform you are willing
to use with Hexabitz modules, here is an example of using and implementing the library using C# language.

First, create instance of the HexInterface and give it a string representing the COM port number your are using.

`HexaInterface HexInter = new HexaInterface("5");`  

or if we want to get it for example from a ComboBox value using windows forms:  

`HexaInterface HexInter = new HexaInterface(COM.Value.ToString());`  

to send a message with the payload (see documentation for correct bytes order)  

`HexInter.SendMessage(Destination, Source, Options, Code, Payload);`  

but before sending the message we must chek the `Options` global variables 
<pre>
<code>public string Opt8_Next_Message = "0";
public string Opt67_Response_Options = "01";
public string Opt5_Reserved = "0";
public string Opt34_Trace_Options = "00";
public string Opt2_16_BIT_Code = "1";
public string Opt1_Extended_Flag = "0";
</code>
</pre>

As described about the parameters above, we have the `Payload` array must be constructed in the right order to get the modules do the right job.  
The following example is about communicating with the H26R0x module to send and recive data from it:  
We have two variables `Period` and `Time` and with the data type : `uint`  

We are going to get the 4 byte array for them (C#):  

`byte[] PeriodBytes = BitConverter.GetBytes(period);`  
`byte[] TimeBytes = BitConverter.GetBytes(time);`  

and we've got also (for this example) `Channel`, `PortModule` and `Module` as bytes to be included in the `Payload` array, so the final will be:  

<pre>
<code>byte[] Payload = {
                channel,
                periodBytes[3],
                periodBytes[2],
                periodBytes[1],
                periodBytes[0],

                timeBytes[3],
                timeBytes[2],
                timeBytes[1],
                timeBytes[0],

                modulePort,
                module};
</code>
</pre>

Now we can call the `SendMessage` with the correct `Options` and `Payload`

## Recieving ##
Receiving bytes from a serial port is platform-dependant. We'll show in the following examples how to recive and parse a 4 byte float value using the HexInterface `Recieve` method.

Here we have the `Recieve` Method:

<pre>
<code>// The receiving method to listen to the port if we got any respond from it.
    private void Receive()
    {
        Port.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
        try { Port.Open(); } catch { }
    }
</code>
</pre>

where `Port` is defined as:  
`SerialPort = new SerialPort("COM" + COM, 921600, Parity.None, 8, StopBits.One); // The default values to be used in the connection.`

and `Port_DataReceived` is defind as: 
<pre>
<code>// Method the .Net platform provids to responed to recieved data from SerialPort.
    private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        int bytes_count = 0;
        byte[] buffer = new byte[4];
        bytes_count = Port.Read(buffer, 0, 4);

        string D0 = to_right_hex(buffer[3].ToString("X"));
        string D1 = to_right_hex(buffer[2].ToString("X"));
        string D2 = to_right_hex(buffer[1].ToString("X"));
        string D3 = to_right_hex(buffer[0].ToString("X"));
        string value = D3 + D2 + D1 + D0;

        int IntRep = int.Parse(value, NumberStyles.AllowHexSpecifier); // here is the representation of the value in Integer value
        float FloatRep = BitConverter.ToSingle(BitConverter.GetBytes(IntRep), 0); // convert the int value to float
        
    }
</code>
</pre>

where `to_right_hex` is defind as:  
<pre>
<code>// To correct the values recieved from the port.
        private string to_right_hex(string hex)
        {
            switch (hex)
            {
                case "A": hex = "0" + hex; break;
                case "B": hex = "0" + hex; break;
                case "C": hex = "0" + hex; break;
                case "D": hex = "0" + hex; break;
                case "E": hex = "0" + hex; break;
                case "F": hex = "0" + hex; break;
            }
            return hex;
        }
</code>
</pre>




