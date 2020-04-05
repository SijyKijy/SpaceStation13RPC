using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RPC
{
    class ByondTopic
    {
        public static async Task<string> SendTopicCommandAsync(string hostAddress, string port, string command)
        {
            try
            {
                var message = BuildMessage(command);
                var buffer = new byte[4096];
                if (!IPAddress.TryParse(hostAddress, out var address))
                {
                    var host = await Dns.GetHostEntryAsync(hostAddress);
                    address = host.AddressList[0];
                }
                var endPoint = new IPEndPoint(address, int.Parse(port));

                Socket sender = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(endPoint);

                sender.Send(message);

                int bytesGot = sender.Receive(buffer);

                sender.Shutdown(SocketShutdown.Both);

                return ParseMessage(buffer, bytesGot);
            }
            catch { return null; }
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
