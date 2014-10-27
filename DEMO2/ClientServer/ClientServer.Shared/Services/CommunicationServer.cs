using ClientServer.Models;
using System;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace ClientServer
{
    class CommunicationServer
    {
        public event EventHandler<Result> DataReceived;
        private StreamSocket connectionSocket;

        public async void StartListeningAsync()
        {
            if (connectionSocket != null)
            {
                throw new Exception("Already connected");
            }

            StreamSocketListener listenSocket = new StreamSocketListener();
            listenSocket.ConnectionReceived += ListenSocket_ConnectionReceived;
            await listenSocket.BindServiceNameAsync("5000");
        }
        
        private async void ListenSocket_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            connectionSocket = args.Socket;

            DataReader reader = new DataReader(connectionSocket.InputStream);
            try
            {
                while (true)
                {
                    uint sizeFieldCount = await reader.LoadAsync(sizeof(long));
                    if (sizeFieldCount != sizeof(long))
                        return;

                    uint type = reader.ReadUInt32();
                    uint resultLength = (uint)reader.ReadInt32();
                    uint actualResultLength = await reader.LoadAsync(resultLength);
                    
                    if (resultLength != actualResultLength)
                        return;

                    var byteResult = new byte[resultLength];
                    reader.ReadBytes(byteResult);

                    if (DataReceived != null)
                        DataReceived(this, new Result { Value = byteResult, ResultType = (ResultType)type });
                }
            }
            catch
            {
            }
        }
    }
}
