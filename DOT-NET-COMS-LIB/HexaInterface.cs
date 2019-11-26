using System;
using System.Globalization;
using System.IO.Ports;

namespace DOT_NET_COMS_LIB
{
    public class HexaInterface
    {
        SerialPort Port;
        string COM;

        public string Extended_Flag = "0";
        public byte[] AllMessage;
        public HexaInterface(string COM)
        {
            this.COM = COM;
        }

        public void Start()
        {
            Port = new SerialPort("COM" + COM, 921600, Parity.None, 8, StopBits.One); // The default values to be used in the connection.
            try { Port.Open(); } catch { }
        }

        public void End()
        {
            try { Port.Close(); } catch { }
            Port.Dispose();
        }

        public byte Options (Options1_Extended_Flag options1_Extended_Flag,
                                Options2_16_BIT_Code options2_16_BIT_Code,
                                Options34_Trace_Options options34_Trace_Options,
                                Options5_Reserved options5_Reserved,
                                Options67_Response_Options options67_Response_Options,
                                Options8_Next_Message options8_Next_Message)  // 00100010 // 0x22
        {
            string optionsString = "" + options1_Extended_Flag +
                                        options2_16_BIT_Code +
                                        options34_Trace_Options +
                                        options5_Reserved +
                                        options67_Response_Options +
                                        options8_Next_Message;

            char[] charArray = optionsString.ToCharArray();
            Array.Reverse(charArray);
            optionsString = new string(charArray);
            return byte.Parse(optionsString);
        }

        // Method to send the buffer to Hexabitz modules.
        public void SendMessage(byte Destination, byte Source, byte Options, int Code, byte[] Payload) 
        {

            Start();
            Message _Message = new Message(Destination, Source, Options, Code, Payload);
            AllMessage = _Message.GetAll();  // We get the whole buffer bytes to be sent to the Hexabitz modules.
            
            try { Port.Open(); } catch { }
            try { Port.Write(AllMessage, 0, AllMessage.Length); } catch { Console.WriteLine("Connection Error"); }
            try { Port.Close(); } catch { }
            End();
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

        // Enum for the codes to be sent to the modules
        public enum Message_Codes
        {
            // H26R0x
            CODE_H26R0_STREAM_PORT_GRAM = 1901,
            CODE_H26R0_STREAM_PORT_KGRAM = 1902,
            CODE_H26R0_STREAM_PORT_OUNCE = 1903,
            CODE_H26R0_STREAM_PORT_POUND = 1904,
            CODE_H26R0_STOP = 1905,
            CODE_H26R0_ZEROCAL = 1910,
        }


        #region Options

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
