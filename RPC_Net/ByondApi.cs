using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RPC
{
    class ByondApi
    {
        public static async Task<string> SendTopicCommandAsync(string ip, int port, string command = "status")
        {
            IPAddress address;

            try
            {
                var host = await Dns.GetHostEntryAsync(ip);
                address = host.AddressList[0];
            }
            catch { throw new Exception("Error: Connection failed"); }

            var message = BuildMessage(command);
            var buffer = new byte[4096];
            int bytesGot;

            using (Socket sender = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                try { sender.Connect(address, port); }
                catch { throw new Exception("Error: Connection failed"); }

                sender.Send(message);
                bytesGot = sender.Receive(buffer);
            }

            return ParseMessage(buffer, bytesGot);
        }

        private static byte[] BuildMessage(string command)
        {
            command = "?" + command;

            byte[] message = Encoding.ASCII.GetBytes(command);
            byte[] sendingBytes = new byte[message.Length + 10];

            sendingBytes[1] = 0x83;
            Pack(message.Length + 6).CopyTo(sendingBytes, 2);

            message.CopyTo(sendingBytes, 9);

            return sendingBytes;

            //thx to Rotem12 on byond forums
        }

        private static byte[] Pack(int num)
        {
            byte[] packed = BitConverter.GetBytes((short)num);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(packed);
            }
            return packed;
        }

        private static string ParseMessage(byte[] msgBytes, int bytesGot)
        {
            if ((msgBytes[0] != 0x00) || (msgBytes[1] != 0x83)) return null;
            string resp = Encoding.UTF8.GetString(msgBytes, 5, bytesGot - 5);
            return resp;
        }
    }
}
