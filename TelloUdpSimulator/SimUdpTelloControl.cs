using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Messenger;

namespace TelloUdpSimulator
{
    public sealed class SimUdpTelloControl : Receiver, IObserver<IEnvelope>
    {
        private readonly int port;
        private UdpClient client;
        private IPEndPoint remoteEndpoint;

        private CancellationTokenSource cancellationTokenSource = null;

        public SimUdpTelloControl(int port)
        {
            this.port = port;
        }

        public Task StartListener()
        {
            if (this.cancellationTokenSource == null)
            {
                this.cancellationTokenSource = new CancellationTokenSource();
                var task = Task.Run(async () => await this.Listen(this.cancellationTokenSource.Token));
                return task;
            }

            return null;
        }

        protected override async Task Listen(CancellationToken cancellationToken)
        {
            if (cancellationToken == null)
            {
                throw new ArgumentNullException(nameof(cancellationToken));
            }

            var wait = default(SpinWait);

            using (var unsubscriber = Subscribe(this))
            {
                using (this.client = new UdpClient(this.port))
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            if (client.Available > 0)
                            {
                                var udpReceiveResult = await this.client.ReceiveAsync();
                                remoteEndpoint = udpReceiveResult.RemoteEndPoint;
                                this.MessageReceived(new Envelope(udpReceiveResult.Buffer));
                            }
                            else
                            {
                                wait.SpinOnce();
                            }
                        }
                        catch (Exception ex)
                        {
                            this.ExceptionThrown(ex);
                        }
                    }

                    this.client = null;
                }
            }
        }


        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        private void SendResponse(string response)
        {
            Debug.WriteLine($"RESPONSE: {response}");
            var data = Encoding.UTF8.GetBytes(response);
            if (client == null)
            {
                return;
            }

            client.Send(data, data.Length, remoteEndpoint);
        }

        public void OnNext(IEnvelope envelope)
        {
            string command = envelope.Data != null && envelope.Data.Length > 0
                ? Encoding.UTF8.GetString(envelope.Data).Trim()
                : String.Empty;

            Debug.WriteLine($"RECEIVED: {command}");
            
            if (command.Equals("command"))
            {
                SendResponse("ok");
            }
            else if (command.Equals("emergency"))
            {

                SendResponse("ok");
            }
            else if (command.Equals("battery?"))
            {

                SendResponse("75");
            }
            else if (command.Equals("takeoff"))
            {

                SendResponse("ok");
            }
            else if (command.Equals("land"))
            {

                SendResponse("ok");
            }
            else if (command.StartsWith("forward"))
            {

                SendResponse("ok");
            }
            else if (command.StartsWith("back"))
            {

                SendResponse("ok");
            }
            else if (command.StartsWith("left"))
            {

                SendResponse("ok");
            }
            else if (command.StartsWith("right"))
            {

                SendResponse("ok");
            }
            
            else
            {
                SendResponse("error");
            }
            
        }
    }
}