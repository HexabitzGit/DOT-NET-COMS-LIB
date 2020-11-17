using System;
using System.Collections.Generic;

namespace DOT_NET_COMS_LIB
{
    public class Message
    {
        #region Variables
        public byte H = 0x48,
             Z = 0x5A,
             // length byte (bytes)
             Destination,
             Source,
             Options,
             LSC,     // Least significance code byte.
             MSC,     // Most significance code byte.
             // Message byte array 
             // ...
             CRC;
        int Length = 0;
        public List<byte> AllMessageList;
        public byte[] Payload, AllMessage, OrganizedBuffer;
        #endregion

        /// <summary>
        /// The Hexabitz Buffer class wiki: https://hexabitz.com/docs/code-overview/array-messaging/
        /// The general constructor, all the payload parameters [Par1, Par2,...] must be included in the correct order within the Message array.
        /// </summary>
        public Message(byte Destination, byte Source,  byte Options, int Code, byte[] Payload)
        {
            this.Destination = Destination;
            this.Source = Source;
            this.Options = Options;
            LSC = (byte)(Code & 0xFF); // Get the MSC & LSC automaticly from the code.
            MSC = (byte)(Code >> 8);
            this.Payload = Payload;

            AllMessageList = new List<byte>();
            AllMessageList.Add(H);
            AllMessageList.Add(Z);
            // Length byte postion
            AllMessageList.Add(Destination);
            AllMessageList.Add(Source);
            AllMessageList.Add(Options);
            AllMessageList.Add(LSC);
            if(MSC != 0) // If the code is only one byte so the MSC
                AllMessageList.Add(MSC);

            foreach (byte item in Payload)
            {
                AllMessageList.Add(item);
            }

            Length = (AllMessageList.Count); 

            if (Length > 255)
            {
                Length -= 4;
                byte LSL = (byte)(Length & 0xFF); // Get the MSL (Most significance length byte) & LSL (Least significance length byte) automaticly from the length.
                byte MSL = (byte)(Length >> 8);
                AllMessageList.Insert(2, LSL);
                AllMessageList.Insert(3, MSL);
            }
            else
            {
                byte hexaLength = (byte)(Length - 3); // Not including H & Z delimiters, the length byte itself and the CRC byte
                                                      // so its 4 but we didn't add the CRC yet so its 3.
                AllMessageList.Insert(2, hexaLength); // Added the byte length in the correct position for the message.
            }

            CRC = GetCRC();
            AllMessageList.Add(CRC);
            AllMessage = AllMessageList.ToArray();
        }

        public Message(byte [] Buffer)
        {
            AllMessageList = new List<byte>();
            AllMessage = Buffer;
            CRC = GetCRC();
        }

        // Return the Cyclic Redundancy Check for the buffer.
        public byte GetCRC()
        {
            List<byte> organizedMesssageList = Organize(AllMessageList); // Here we are organizing the buffer to calculate the CRC for it.
            OrganizedBuffer = organizedMesssageList.ToArray();
            CRC = CRC32B(OrganizedBuffer);
            return CRC;
        }

        // Check if a givin message have a correct CRC.
        public bool IsValid()
        {
            byte Buffer_CRC = AllMessage[AllMessage.Length - 1];

            foreach (byte value in AllMessage)
            {
                AllMessageList.Add(value);
            }
            AllMessageList.RemoveAt(AllMessageList.Count - 1);
            CRC = GetCRC();
            return (CRC == Buffer_CRC);
        }

        // Get the whole buffer.
        public byte[] GetAll()
        {
            return AllMessage;
        }

        // Routin for ordering the buffer to calculate the CRC for it.
        private List<byte> Organize(List<byte> MesssageList) 
        {
            List<byte> OrganizedBuffer = new List<byte>();

            int MultiplesOf4 = 0;

            List<byte> temp = new List<byte>();
            foreach (byte item in MesssageList)
            {
                temp.Add(item);
                if (temp.Count == 4)
                {
                    MultiplesOf4++;
                    temp.Reverse();
                    foreach (byte itemReversed in temp)
                    {
                        OrganizedBuffer.Add(itemReversed);
                    }
                    temp.Clear();
                }
            }
            temp.Clear();
            if ((MesssageList.Count - OrganizedBuffer.Count) != 0)
            {
                int startingItem = MultiplesOf4 * 4;
                for (int i = startingItem; i < MesssageList.Count; i++)
                {
                    temp.Add(MesssageList[i]);
                }

                while (temp.Count < 4)
                    temp.Add(0);
                temp.Reverse();

                foreach (byte value in temp)
                {
                    OrganizedBuffer.Add(value);
                }
            }
            return OrganizedBuffer;
        }

        // Algorithm used in the Hexabitz modules hardware to calculate the correct CRC32 but we are only using the first byte in our modules. 
        private byte CRC32B(byte[] Buffer)  // Change byte to int to get the whole CRC32.
        {
            short L = (short)Buffer.Length;
            short I, J;
            uint CRC, MSB;
            CRC = 0xFFFFFFFF;
            for (I = 0; I < L; I++)
            {
                CRC ^= (((uint)Buffer[I]) << 24);
                for (J = 0; J < 8; J++)
                {
                    MSB = CRC >> 31;
                    CRC <<= 1;
                    CRC ^= (0 - MSB) & 0x04C11DB7;
                }
            }
            return (byte)CRC; // Remove (byte) to get get the full int.
        }
    }
}
