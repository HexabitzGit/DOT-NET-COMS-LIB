using System;
using System.Globalization;
using System.IO.Ports;
using System.Linq;

namespace DOT_NET_COMS_LIB
{
    public class HexaInterface
    {
        SerialPort Port;
        string COM;
        int BaudRate;

        public string Opt8_Next_Message = "0";
        public string Opt67_Response_Options = "01";
        public string Opt5_Reserved = "0";
        public string Opt34_Trace_Options = "00";
        public string Opt2_16_BIT_Code = "1";
        public string Opt1_Extended_Flag = "0";
        public byte[] AllMessage;

        public HexaInterface(string COM, int BaudRate)
        {
            this.COM = COM;
            this.BaudRate = BaudRate;
        }

        // Method to send the buffer to Hexabitz modules.
        public void SendMessage(byte Destination, byte Source, int Code, byte[] Payload)
        {
            string optionsString = Opt8_Next_Message +
                                   Opt67_Response_Options +
                                   Opt5_Reserved +
                                   Opt34_Trace_Options +
                                   Opt2_16_BIT_Code +
                                   Opt1_Extended_Flag;
            byte Options = GetBytes(optionsString)[0];  // 00100010 // 0x22

            Start();
            Message _Message = new Message(Destination, Source, Options, Code, Payload);
            AllMessage = _Message.GetAll();  // We get the whole buffer bytes to be sent to the Hexabitz modules.
            try { Port.Write(AllMessage, 0, AllMessage.Length); } catch (Exception exp) { Console.WriteLine("Connection Error"); }
            End();
        }

        public void Start()
        {
            Port = new SerialPort("COM" + COM, BaudRate, Parity.None, 8, StopBits.One); // The default values to be used in the connection.
            try { Port.Open(); } catch (Exception exp) { }
        }

        public void End()
        {
            try { Port.Close(); } catch (Exception exp) { }
            Port.Dispose();
        }
        
        // The reviceing method to listen to the port if we got any response from it.
        private void Receive()
        {
            Port.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
            try { Port.Open(); } catch { }
        }

        // Method the .Net platform provids to responed to recieved data from SerialPort.
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytes_count = 0;
            byte[] buffer = new byte[4];
            bytes_count = Port.Read(buffer, 0, 4);

            string D0 = to_right_hex(buffer[3].ToString("X"));
            string D1 = to_right_hex(buffer[2].ToString("X"));
            string D2 = to_right_hex(buffer[1].ToString("X"));
            string D3 = to_right_hex(buffer[0].ToString("X"));
            string weight = D3 + D2 + D1 + D0;

            int IntRep = int.Parse(weight, NumberStyles.AllowHexSpecifier);
            float f = BitConverter.ToSingle(BitConverter.GetBytes(IntRep), 0);
            float rounded = (float)(Math.Round(f, 2));
            if (rounded < 0.1f)
                rounded = 0;
        }

        // To correct the values recieved from the port.
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

        //get byte from string
        public static byte[] GetBytes(string bitString)
        {
            return Enumerable.Range(0, bitString.Length / 8).
                Select(pos => Convert.ToByte(
                    bitString.Substring(pos * 8, 8),
                    2)
                ).ToArray();
        }

        // Enum for the codes to be sent to the modules
        public enum Message_Codes
        {
            // H01R0 (Led Module)
            CODE_H01R0_ON			     =	100,
            CODE_H01R0_OFF			     =	101,
            CODE_H01R0_TOGGLE		     =	102,
            CODE_H01R0_COLOR		     =	103,
            CODE_H01R0_PULSE	       	 =	104,
            CODE_H01R0_SWEEP		     =	105,
            CODE_H01R0_DIM			     =	106,

            // H0FR6x
            CODE_H0FR6_ON		         =	750,
            CODE_H0FR6_OFF			     =	751,
            CODE_H0FR6_TOGGLE		     =	752,
            CODE_H0FR6_PWM			     =	753,

            // H26R0x (Load-Cell Module)
            CODE_H26R0_STREAM_PORT_GRAM  = 1901,
            CODE_H26R0_STREAM_PORT_KGRAM = 1902,
            CODE_H26R0_STREAM_PORT_OUNCE = 1903,
            CODE_H26R0_STREAM_PORT_POUND = 1904,
            CODE_H26R0_STOP              = 1905,
            CODE_H26R0_ZEROCAL           = 1910,
        }

        #region Options_Byte
        // 8th bit (MSB): Long messages flag. If set, then message parameters continue in the next message.
        public enum Options8_Next_Message
        {
            FALSE = 0,
            TRUE = 1,
        }

        // 6-7th bits:
        public enum Options67_Response_Options
        {
            SEND_BACK_NO_RESPONSE = 00,
            SEND_RESPONSES_ONLY_MESSAGES = 01,
            SEND_RESPONSES_ONLY_TO_CLI_COMMANDS = 10,
            SEND_RESPONSES_TO_EVERYTHING = 11,
        }

        // 5th bit: reserved.
        public enum Options5_Reserved
        {
            FALSE = 0,
            TRUE = 1,
        }

        // 3rd-4th bits:
        public enum Options34_Trace_Options
        {
            SHOW_NO_TRACE = 00,
            SHOW_MESSAGE_TRACE = 01,
            SHOW_MESSAGE_RESPONSE_TRACE = 10,
            SHOW_TRACE_FOR_BOTH_MESSAGES_AND_THEIR_RESPONSES = 11,
        }

        // 2nd bit: Extended Message Code flag. If set, then message codes are 16 bits.
        public enum Options2_16_BIT_Code
        {
            FALSE = 0,
            TRUE = 1,
        }

        // 1st bit (LSB): Extended Options flag. If set, then the next byte is an Options byte as well.
        public enum Options1_Extended_Flag
        {
            FALSE = 0,
            TRUE = 1,
        }
        #endregion
    }
}
