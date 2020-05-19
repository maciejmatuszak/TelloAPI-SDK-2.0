// <copyright file="TelloMessenger.cs" company="Mark Lauter">
// Copyright (c) Mark Lauter. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Messenger;

namespace Tello.Messaging
{
    public class TelloMessenger : Messenger<string>
    {
        private readonly ConcurrentQueue<TelloRequest> commands = new ConcurrentQueue<TelloRequest>();
        private readonly ITransceiver transceiver;

        public TelloMessenger(ITransceiver transceiver)
        {
            this.transceiver = transceiver ?? throw new ArgumentNullException(nameof(transceiver));
            this.ProcessCommandQueueAsync();
        }

        public void Enqueue(TelloRequest request)
        {
            this.commands.Enqueue(request);
        }

        public Task<TelloResponse> SendAsync(CommandCode commandCode, params object[] args)
        {
            return this.SendAsync(new Command(commandCode, args));
        }

        public async Task<TelloResponse> SendAsync(Command command)
        {
            if (command.Immediate)
            {
                return new TelloResponse(await this.transceiver.SendAsync(new TelloRequest(command)));
            }
            else
            {
                Debug.WriteLine($"{nameof(this.SendAsync)}: '{command}' command queue is {this.commands.Count} deep.");
                this.Enqueue(new TelloRequest(command));
                return await Task.FromResult<TelloResponse>(null);
            }
        }

        private async void ProcessCommandQueueAsync()
        {
            await Task.Run(async () =>
            {
                var spinWait = default(SpinWait);
                while (true)
                {
                    try
                    {
                        if (!this.commands.IsEmpty && this.commands.TryDequeue(out var request))
                        {
                            Debug.WriteLine($"{nameof(this.ProcessCommandQueueAsync)}: command queue is {this.commands.Count} deep.");

                            Debug.WriteLine($"{nameof(this.ProcessCommandQueueAsync)}: request.Message '{request.Message}'");
                            Debug.WriteLine($"{nameof(this.ProcessCommandQueueAsync)}: request.Timeout '{request.Timeout}'");

                            var response = new TelloResponse(await this.transceiver.SendAsync(request));
                            Debug.WriteLine($"{nameof(this.ProcessCommandQueueAsync)}: response.Success '{response.Success}'");
                            Debug.WriteLine($"{nameof(this.ProcessCommandQueueAsync)}: response.Message '{response.Message}'");

                            this.ReponseReceived(response);
                        }
                        else
                        {
                            spinWait.SpinOnce();
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ExceptionThrown(ex);
                    }
                }
            });
        }
    }
}
