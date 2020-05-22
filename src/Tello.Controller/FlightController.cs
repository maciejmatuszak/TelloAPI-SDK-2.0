﻿// <copyright file="FlightController.cs" company="Mark Lauter">
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
    public sealed class FlightController : Observer<IResponse<string>>, IFlightController
    {
        private readonly TelloMessenger messenger;

        public FlightController(ITransceiver transceiver)
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
                this.ExceptionThrown?.Invoke(this, new ExceptionThrownArgs(new TelloException("FlightController Error", error)));
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
                    this.isFlying = true;
                    break;
                case CommandCode.Land:
                case CommandCode.EmergencyStop:
                    this.isFlying = false;
                    break;
                case CommandCode.StartVideo:
                    this.isVideoStreaming = true;
                    this.VideoStreamingStateChanged?.Invoke(this, new VideoStreamingStateChangedArgs(this.isVideoStreaming));
                    break;
                case CommandCode.StopVideo:
                    this.isVideoStreaming = false;
                    this.VideoStreamingStateChanged?.Invoke(this, new VideoStreamingStateChangedArgs(this.isVideoStreaming));
                    break;

                case CommandCode.Left:
                    this.Position = this.Position.Move(CardinalDirections.Left, (int)((Command)response.Request.Data).Arguments[0]);
                    this.PositionChanged?.Invoke(this, new PositionChangedArgs(this.Position));
                    break;
                case CommandCode.Right:
                    this.Position = this.Position.Move(CardinalDirections.Right, (int)((Command)response.Request.Data).Arguments[0]);
                    this.PositionChanged?.Invoke(this, new PositionChangedArgs(this.Position));
                    break;
                case CommandCode.Forward:
                    this.Position = this.Position.Move(CardinalDirections.Front, (int)((Command)response.Request.Data).Arguments[0]);
                    this.PositionChanged?.Invoke(this, new PositionChangedArgs(this.Position));
                    break;
                case CommandCode.Back:
                    this.Position = this.Position.Move(CardinalDirections.Back, (int)((Command)response.Request.Data).Arguments[0]);
                    this.PositionChanged?.Invoke(this, new PositionChangedArgs(this.Position));
                    break;
                case CommandCode.ClockwiseTurn:
                    this.Position = this.Position.Turn(ClockDirections.Clockwise, (int)((Command)response.Request.Data).Arguments[0]);
                    this.PositionChanged?.Invoke(this, new PositionChangedArgs(this.Position));
                    break;
                case CommandCode.CounterClockwiseTurn:
                    this.Position = this.Position.Turn(ClockDirections.CounterClockwise, (int)((Command)response.Request.Data).Arguments[0]);
                    this.PositionChanged?.Invoke(this, new PositionChangedArgs(this.Position));
                    break;
                case CommandCode.Go:
                    this.Position = this.Position.Go((int)((Command)response.Request.Data).Arguments[0], (int)((Command)response.Request.Data).Arguments[1]);
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
                                    throw new TelloException($"'{command}' expecting response '{ResponseHandleCode.Ok.ToString().ToLowerInvariant()}' returned message '{response.Message}' at {response.Timestamp.ToString("o")} after {response.TimeTaken.TotalMilliseconds}ms");
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
                        throw new TelloException($"{command} returned message '{response.Message}' at {response.Timestamp.ToString("o")} after {response.TimeTaken.TotalMilliseconds}ms");
                    }
                }
                else
                {
                    this.OnError(new TelloException($"{command} returned message '{response.Message}' at {response.Timestamp.ToString("o")} after {response.TimeTaken.TotalMilliseconds}ms", response.Exception));
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

        public event EventHandler<ConnectionStateChangedArgs> ConnectionStateChanged;

        public event EventHandler<PositionChangedArgs> PositionChanged;

        public event EventHandler<VideoStreamingStateChangedArgs> VideoStreamingStateChanged;

        public InterogativeState InterogativeState { get; } = new InterogativeState();

        public Vector Position { get; private set; } = new Vector();

        public ITelloState State { get; private set; } = new TelloState();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "_ is discard.")]
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
                while (this.isConnected)
                {
                    await Task.Delay(1000 * 10);
                    this.GetBattery();
                }
            });
        }

        #region controller state
        private bool isConnected = false;
        private bool isFlying = false;
        private bool isVideoStreaming = false;

        private bool CanManeuver => this.isConnected && this.isFlying;

        private bool CanTakeoff => this.isConnected && !this.isFlying;
        #endregion

        #region enter/exit sdk mode (connect/disconnect)
        public async Task<bool> Connect()
        {
            if (!this.isConnected)
            {
                var response = await this.messenger.SendAsync(CommandCode.EnterSdkMode);
                this.isConnected = response != null && response.Success;
                if (this.isConnected)
                {
                    this.RunKeepAliveAsync();
                    this.ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedArgs(this.isConnected));
                }
            }

            return this.isConnected;
        }

        public void Disconnect()
        {
            if (this.isConnected)
            {
                this.isConnected = false;
                this.ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedArgs(this.isConnected));
            }
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

        public async void GetWIFISNR()
        {
            await this.messenger.SendAsync(CommandCode.GetWIFISnr);
        }
        #endregion

        #region maneuver
        public async void TakeOff()
        {
            if (this.CanTakeoff)
            {
                await this.messenger.SendAsync(CommandCode.Takeoff);
            }
        }

        public async void Land()
        {
            if (this.CanManeuver)
            {
                await this.messenger.SendAsync(CommandCode.Land);
            }
        }

        public async Task EmergencyStop()
        {
            await this.messenger.SendAsync(CommandCode.EmergencyStop);
            this.isFlying = false;
            this.Disconnect();
        }

        public async Task Stop()
        {
            if (this.CanManeuver)
            {
                await this.messenger.SendAsync(CommandCode.Stop);
            }
        }

        public async void Set4ChannelRC(int leftRight, int forwardBackward, int upDown, int yaw)
        {
            if (this.CanManeuver)
            {
                var cmd = new Command(CommandCode.SetRemoteControl, new object[] { leftRight, forwardBackward, upDown, yaw });
                Debug.WriteLine(cmd.ToString());
                await this.messenger.SendAsync(CommandCode.SetRemoteControl, leftRight, forwardBackward, upDown, yaw);
            }
        }

        public async void Curve(int x1, int y1, int z1, int x2, int y2, int z2, int speed)
        {
            if (this.CanManeuver)
            {
                await this.messenger.SendAsync(CommandCode.Curve, x1, y1, z1, x2, y2, z2, speed);
            }
        }

        public async void Flip(CardinalDirections direction)
        {
            if (this.CanManeuver)
            {
                await this.messenger.SendAsync(CommandCode.Flip, (char)(CardinalDirection)direction);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sides">3 to 15.</param>
        /// <param name="length">length of each side. 20 to 500 in cm.</param>
        /// <param name="speed">cm/s 10 to 100.</param>
        /// /// <param name="clockDirection">Clock direction.</param>
        public void FlyPolygon(int sides, int length, int speed, ClockDirections clockDirection)
        {
            if (!this.CanManeuver)
            {
                return;
            }

            if (sides < 3 || sides > 15)
            {
                throw new ArgumentOutOfRangeException($"{nameof(sides)} allowed values: 3 to 15");
            }

            this.SetSpeed(speed);

            var turnMethod = default(Action<int>);
            switch (clockDirection)
            {
                case ClockDirections.Clockwise:
                    turnMethod = this.TurnClockwise;
                    break;
                case ClockDirections.CounterClockwise:
                    turnMethod = this.TurnCounterClockwise;
                    break;
            }

            var angle = (int)Math.Round(360.0 / sides);
            for (var i = 0; i < sides; ++i)
            {
                this.GoForward(length);
                turnMethod(angle);
            }
        }

        public async void Go(int x, int y, int z, int speed)
        {
            if (this.CanManeuver)
            {
                await this.messenger.SendAsync(CommandCode.Go, x, y, z, speed);
            }
        }

        public async void GoBackward(int cm)
        {
            if (this.CanManeuver)
            {
                await this.messenger.SendAsync(CommandCode.Back, cm);
            }
        }

        public async void GoDown(int cm)
        {
            if (this.CanManeuver)
            {
                await this.messenger.SendAsync(CommandCode.Down, cm);
            }
        }

        public async void GoForward(int cm)
        {
            if (this.CanManeuver)
            {
                await this.messenger.SendAsync(CommandCode.Forward, cm);
            }
        }

        public async void GoLeft(int cm)
        {
            if (this.CanManeuver)
            {
                await this.messenger.SendAsync(CommandCode.Left, cm);
            }
        }

        public async void GoRight(int cm)
        {
            if (this.CanManeuver)
            {
                await this.messenger.SendAsync(CommandCode.Right, cm);
            }
        }

        public async void GoUp(int cm)
        {
            if (this.CanManeuver)
            {
                await this.messenger.SendAsync(CommandCode.Up, cm);
            }
        }

        public async void TurnClockwise(int degress)
        {
            if (this.CanManeuver)
            {
                await this.messenger.SendAsync(CommandCode.ClockwiseTurn, degress);
            }
        }

        public async void TurnCounterClockwise(int degress)
        {
            if (this.CanManeuver)
            {
                await this.messenger.SendAsync(CommandCode.CounterClockwiseTurn, degress);
            }
        }

        public void TurnLeft(int degress)
        {
            this.TurnCounterClockwise(degress);
        }

        public void TurnRight(int degress)
        {
            this.TurnClockwise(degress);
        }

        public void SetHeight(int cm)
        {
            if (this.CanManeuver && this.State != null)
            {
                var delta = cm - this.State.HeightInCm;
                if (delta >= 20 && delta <= 500)
                {
                    if (delta < 0)
                    {
                        this.GoDown(Math.Abs(delta));
                    }
                    else
                    {
                        this.GoUp(delta);
                    }
                }
            }
        }

        #endregion

        #region drone configuration
        public async void SetSpeed(int speed)
        {
            if (this.isConnected)
            {
                await this.messenger.SendAsync(CommandCode.SetSpeed, speed);
            }
        }

        public async void SetStationMode(string ssid, string password)
        {
            if (this.isConnected)
            {
                await this.messenger.SendAsync(CommandCode.SetStationMode, ssid, password);
            }
        }

        public async void SetWIFIPassword(string ssid, string password)
        {
            if (this.isConnected)
            {
                await this.messenger.SendAsync(CommandCode.SetWiFiPassword, ssid, password);
            }
        }
        #endregion

        #region video state
        public async void StartVideo()
        {
            if (this.isConnected && !this.isVideoStreaming)
            {
                await this.messenger.SendAsync(CommandCode.StartVideo);
            }
        }

        public async void StopVideo()
        {
            if (this.isConnected && this.isVideoStreaming)
            {
                await this.messenger.SendAsync(CommandCode.StopVideo);
            }
        }
        #endregion

        #endregion
    }
}
