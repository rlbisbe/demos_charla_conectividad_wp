using ClientServer.Models;
using System;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace ClientServer
{
    class CommunicationClient
    {
        private StreamSocket connectionSocket;

        public async Task ConnectAsync(string server, string port)
        {
            connectionSocket = new StreamSocket();
            var serverHost = new HostName(server);
            await connectionSocket.ConnectAsync(serverHost, port);
        }

        public async Task SendMessage(string msg)
        {
            if (connectionSocket == null || msg == "") return;
            DataWriter writer = new DataWriter(connectionSocket.OutputStream);

            writer.WriteUInt32((uint)ResultType.Text);
            writer.WriteInt32((int)writer.MeasureString(msg));
            writer.WriteString(msg);

            await TryStore(writer);
        }

        public async Task SendByteArray(byte[] bytes)
        {
            if (connectionSocket == null || bytes.Length == 0) return;
            DataWriter writer = new DataWriter(connectionSocket.OutputStream);

            writer.WriteUInt32((uint)ResultType.Image);
            writer.WriteInt32(bytes.Length);
            writer.WriteBytes(bytes);

            await TryStore(writer);
        }

        private static async Task TryStore(DataWriter writer)
        {
            try
            {
                await writer.StoreAsync();
                writer.DetachStream();
            }
            catch
            {
            }
        }
    }
}
