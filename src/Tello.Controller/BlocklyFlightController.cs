// <copyright file="FlightController.cs" company="Mark Lauter">
// Copyright (c) Mark Lauter. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Messenger;
using Tello.Events;
using Tello.Messaging;
using Tello.State;

namespace Tello.Controller
{
    public sealed class BlocklyFlightController : Observer<IResponse<string>>
    {
        private readonly TelloMessenger messenger;
        private ConnectionStateEnum connectionstate;

        public BlocklyFlightController(ITransceiver transceiver)
            : base()
        {
            this.messenger = new TelloMessenger(transceiver ?? throw new ArgumentNullException(nameof(transceiver)));
            this.Subscribe(this.messenger);
        }

        #region Observer<IResponse<string>> - transceiver reponse handling

        public override void OnError(Exception error)
        {
            try
            {
                this.ExceptionThrown?.Invoke(this,
                    new ExceptionThrownArgs(new TelloException("FlightController Error", error)));
            }
            catch
            {
            }
        }

        private void HandleOk(IResponse<string> response, Command command)
        {
            switch ((CommandCode)command)
            {
                case CommandCode.Takeoff:
                case CommandCode.Land:
                case CommandCode.EmergencyStop:
                case CommandCode.StartVideo:
                    this.VideoStreamingStateChanged?.Invoke(this,
                        new VideoStreamingStateChangedArgs(true));
                    break;
                case CommandCode.StopVideo:
                    this.VideoStreamingStateChanged?.Invoke(this,
                        new VideoStreamingStateChangedArgs(false));
                    break;

                case CommandCode.Left:
                    this.Position = this.Position.Move(CardinalDirections.Left,
                        (int)((Command)response.Request.Data).Arguments[0]);
                    this.PositionChanged?.Invoke(this, new PositionChangedArgs(this.Position));
                    break;
                case CommandCode.Right:
                    this.Position = this.Position.Move(CardinalDirections.Right,
                        (int)((Command)response.Request.Data).Arguments[0]);
                    this.PositionChanged?.Invoke(this, new PositionChangedArgs(this.Position));
                    break;
                case CommandCode.Forward:
                    this.Position = this.Position.Move(CardinalDirections.Front,
                        (int)((Command)response.Request.Data).Arguments[0]);
                    this.PositionChanged?.Invoke(this, new PositionChangedArgs(this.Position));
                    break;
                case CommandCode.Back:
                    this.Position = this.Position.Move(CardinalDirections.Back,
                        (int)((Command)response.Request.Data).Arguments[0]);
                    this.PositionChanged?.Invoke(this, new PositionChangedArgs(this.Position));
                    break;
                case CommandCode.ClockwiseTurn:
                    this.Position = this.Position.Turn(ClockDirections.Clockwise,
                        (int)((Command)response.Request.Data).Arguments[0]);
                    this.PositionChanged?.Invoke(this, new PositionChangedArgs(this.Position));
                    break;
                case CommandCode.CounterClockwiseTurn:
                    this.Position = this.Position.Turn(ClockDirections.CounterClockwise,
                        (int)((Command)response.Request.Data).Arguments[0]);
                    this.PositionChanged?.Invoke(this, new PositionChangedArgs(this.Position));
                    break;
                case CommandCode.Go:
                    this.Position = this.Position.Go((int)((Command)response.Request.Data).Arguments[0],
                        (int)((Command)response.Request.Data).Arguments[1]);
                    this.PositionChanged?.Invoke(this, new PositionChangedArgs(this.Position));
                    break;

                case CommandCode.SetSpeed:
                    this.InterogativeState.Speed = (int)((Command)response.Request.Data).Arguments[0];
                    break;

                case CommandCode.Stop:
                    break;
                case CommandCode.Up:
                    break;
                case CommandCode.Down:
                    break;
                case CommandCode.Curve:
                    break;
                case CommandCode.Flip:
                    break;

                case CommandCode.SetRemoteControl:
                    break;
                case CommandCode.SetWiFiPassword:
                    break;
                case CommandCode.SetStationMode:
                    break;

                default:
                    break;
            }
        }

        public override void OnNext(IResponse<string> response)
        {
            try
            {
                response = new TelloResponse(response);
                var command = (Command)response.Request.Data;

                if (response.Success)
                {
                    if (response.Message != ResponseHandleCode.Error.ToString().ToLowerInvariant())
                    {
                        switch (command.Rule.ResponseHandleCode)
                        {
                            case ResponseHandleCode.Ok:
                                if (response.Message == ResponseHandleCode.Ok.ToString().ToLowerInvariant())
                                {
                                    this.HandleOk(response, command);
                                }
                                else
                                {
                                    throw new TelloException(
                                        $"'{command}' expecting response '{ResponseHandleCode.Ok.ToString().ToLowerInvariant()}' returned message '{response.Message}' at {response.Timestamp.ToString("o")} after {response.TimeTaken.TotalMilliseconds}ms");
                                }

                                break;
                            case ResponseHandleCode.Speed:
                                this.InterogativeState.Speed = Int32.Parse(response.Message);
                                break;
                            case ResponseHandleCode.Battery:
                                this.InterogativeState.Battery = Int32.Parse(response.Message);
                                break;
                            case ResponseHandleCode.Time:
                                this.InterogativeState.Time = Int32.Parse(response.Message);
                                break;
                            case ResponseHandleCode.WIFISnr:
                                this.InterogativeState.WIFISnr = response.Message;
                                break;
                            case ResponseHandleCode.SdkVersion:
                                this.InterogativeState.SdkVersion = response.Message;
                                break;
                            case ResponseHandleCode.SerialNumber:
                                this.InterogativeState.SerialNumber = response.Message;
                                break;
                            case ResponseHandleCode.None:
                            default:
                                break;
                        }
                    }
                    else
                    {
                        throw new TelloException(
                            $"{command} returned message '{response.Message}' at {response.Timestamp.ToString("o")} after {response.TimeTaken.TotalMilliseconds}ms");
                    }
                }
                else
                {
                    this.OnError(new TelloException(
                        $"{command} returned message '{response.Message}' at {response.Timestamp.ToString("o")} after {response.TimeTaken.TotalMilliseconds}ms",
                        response.Exception));
                }

                this.ResponseReceived?.Invoke(this, new ResponseReceivedArgs(response as TelloResponse));
            }
            catch (Exception ex)
            {
                this.OnError(new TelloException(ex.Message, ex));
            }
        }

        #endregion

        #region ITelloController

        public event EventHandler<ResponseReceivedArgs> ResponseReceived;

        public event EventHandler<ExceptionThrownArgs> ExceptionThrown;

        public event EventHandler<ConnectionStateDetailedChangedArgs> ConnectionStateChanged;

        public event EventHandler<PositionChangedArgs> PositionChanged;

        public event EventHandler<VideoStreamingStateChangedArgs> VideoStreamingStateChanged;

        public InterogativeState InterogativeState { get; } = new InterogativeState();

        public Vector Position { get; private set; } = new Vector();

        public ITelloState State { get; private set; } = new TelloState();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules",
            "SA1313:Parameter names should begin with lower-case letter", Justification = "_ is discard.")]
        public void UpdateState(object _, StateChangedArgs e)
        {
            this.State = e.State;
        }

        /// <summary>
        /// Tello auto lands after 15 seconds without commands as a safety mesasure, so we're going to send a keep alive message every 5 seconds.
        /// </summary>
        private async void RunKeepAliveAsync()
        {
            await Task.Run(async () =>
            {
                while (this.ConnectionState == ConnectionStateEnum.Connected)
                {
                    await Task.Delay(1000 * 10);
                    this.GetBattery();
                }
            });
        }

        private ConnectionStateEnum ConnectionState
        {
            get
            {
                return this.connectionstate;
            }
            set
            {
                if (this.connectionstate != value)
                {
                    this.connectionstate = value;
                    this.ConnectionStateChanged?.Invoke(this,
                        new ConnectionStateDetailedChangedArgs(this.connectionstate));
                }
            }
        }

        private bool IsConnected => this.ConnectionState == ConnectionStateEnum.Connected;

        #region enter/exit sdk mode (connect/disconnect)

        public async Task<ConnectionStateEnum> Connect()
        {
            // we can only connect in Disconnected state
            if (this.ConnectionState != ConnectionStateEnum.Disconnected)
            {
                return this.ConnectionState;
            }

            this.ConnectionState = ConnectionStateEnum.Connecting;

            // reset any pending commands
            this.ResetMessenger();

            // send the req
            var response = await this.messenger.SendAsync(CommandCode.EnterSdkMode);
            if (response != null && response.Success)
            {
                // success
                this.ConnectionState = ConnectionStateEnum.Connected;
                // Start the keep alive task
                this.RunKeepAliveAsync();
            }
            else
            {
                this.ConnectionState = ConnectionStateEnum.Disconnected;
            }

            return this.ConnectionState;
        }

        public void Disconnect()
        {
            if (this.ConnectionState == ConnectionStateEnum.Disconnected)
            {
                return;
            }

            this.ResetMessenger();

            this.ConnectionState = ConnectionStateEnum.Disconnected;
        }

        #endregion

        #region state interogation

        public async void GetBattery()
        {
            await this.messenger.SendAsync(CommandCode.GetBattery);
        }

        public async void GetSdkVersion()
        {
            await this.messenger.SendAsync(CommandCode.GetSdkVersion);
        }

        public async void GetSpeed()
        {
            await this.messenger.SendAsync(CommandCode.GetSpeed);
        }

        public async void GetTime()
        {
            await this.messenger.SendAsync(CommandCode.GetTime);
        }

        public async void GetSerialNumber()
        {
            await this.messenger.SendAsync(CommandCode.GetSerialNumber);
        }

        public void ResetMessenger()
        {
            this.messenger.Reset();
        }

        public async Task<TelloResponse> SendImmediateAssync(TelloRequest req)
        {
            return await this.messenger.SendImmediateAssync(req);
        }
        
        public async Task<TelloResponse> SendViaQueueAssync(TelloRequest req)
        {
            return await this.messenger.SendViaQueueAssync(req);
        }

        public async void GetWIFISNR()
        {
            await this.messenger.SendAsync(CommandCode.GetWIFISnr);
        }

        #endregion


        #region drone configuration

        public async void SetSpeed(int speed)
        {
            if (this.IsConnected)
            {
                await this.messenger.SendAsync(CommandCode.SetSpeed, speed);
            }
        }

        public async void SetStationMode(string ssid, string password)
        {
            if (this.IsConnected)
            {
                await this.messenger.SendAsync(CommandCode.SetStationMode, ssid, password);
            }
        }

        public async void SetWIFIPassword(string ssid, string password)
        {
            if (this.IsConnected)
            {
                await this.messenger.SendAsync(CommandCode.SetWiFiPassword, ssid, password);
            }
        }

        #endregion

        #region video state

        public async void StartVideo()
        {
            if (this.IsConnected)
            {
                await this.messenger.SendAsync(CommandCode.StartVideo);
            }
        }

        public async void StopVideo()
        {
            if (this.IsConnected)
            {
                await this.messenger.SendAsync(CommandCode.StopVideo);
            }
        }

        #endregion

        #endregion
    }
}