// <copyright file="DroneSimulator.cs" company="Mark Lauter">
// Copyright (c) Mark Lauter. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Messenger.Simulator;
using Tello.Simulator.Messaging;
using Tello.State;

namespace Tello.Simulator
{
    public sealed class DroneSimulator
    {
        // todo: 1. execute the appropriate command simulation
        // todo: 2. update state
        // todo: 3. notify state transmitter
        // todo: 4. compose and return the appropriate command response
        private bool isVideoStreaming = false;
        private bool isFlying = false;
        private Vector position = new Vector();
        private ITelloState state = new TelloState();
        private int speed = 0;
        private int height = 0;
        private bool inCommandMode = false;

        public DroneSimulator()
        {
            this.MessageHandler = new DroneMessageHandler(this.DroneSimulator_CommandReceived);
            this.StateTransmitter = new StateTransmitter();
            this.VideoTransmitter = new VideoTransmitter();

            this.VideoThread();
            this.StateThread();
        }

        public IDroneMessageHandler MessageHandler { get; }

        public IDroneTransmitter StateTransmitter { get; }

        public IDroneTransmitter VideoTransmitter { get; }

        private async void VideoThread()
        {
            var bytes = Encoding.UTF8.GetBytes("this is fake video data");
            await Task.Run(async () =>
            {
                var spinWait = default(SpinWait);
                while (true)
                {
                    if (this.isVideoStreaming)
                    {
                        // (VideoTransmitter as VideoTransmitter).AddVideoSegment(Array.Empty<byte>());
                        (this.VideoTransmitter as VideoTransmitter).AddVideoSegment(bytes);
                        await Task.Delay(1000 / 30);
                    }
                    else
                    {
                        spinWait.SpinOnce();
                    }
                }
            });
        }

        private async void StateThread()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (this.inCommandMode)
                    {
                        (this.StateTransmitter as StateTransmitter).SetState(this.state);
                    }

                    await Task.Delay(1000 / 5);
                }
            });
        }

        private string DroneSimulator_CommandReceived(Command command)
        {
            try
            {
                return this.Invoke(command);
            }
            catch (Exception ex)
            {
                return $"error {ex.GetType().Name}: {ex.Message}";
            }
        }

        private string Invoke(Command command)
        {
            if (command != CommandCode.EnterSdkMode && !this.inCommandMode)
            {
                throw new TelloException("Call EnterSdkMode first.");
            }

            if (!this.isFlying && command.Rule.MustBeInFlight)
            {
                throw new TelloException("Call Takeoff first.");
            }

            switch (command.Rule.ResponseHandleCode)
            {
                case ResponseHandleCode.Ok:
                    this.HandleOk(command);
                    this.state = new TelloState(this.position);
                    return "ok";
                case ResponseHandleCode.Speed:
                    return this.speed.ToString();
                case ResponseHandleCode.Battery:
                    return "99";
                case ResponseHandleCode.Time:
                    return "0";
                case ResponseHandleCode.WIFISnr:
                    return "unk";
                case ResponseHandleCode.SdkVersion:
                    return "Sim V1";
                case ResponseHandleCode.SerialNumber:
                    return "SIM-1234";
                case ResponseHandleCode.None:
                    return String.Empty;
                default:
                    throw new NotSupportedException();
            }
        }

        private void HandleOk(Command command)
        {
            switch ((CommandCode)command)
            {
                case CommandCode.EnterSdkMode:
                    this.inCommandMode = true;
                    break;

                case CommandCode.Takeoff:
                    this.height = 20;
                    this.isFlying = true;
                    break;

                case CommandCode.EmergencyStop:
                case CommandCode.Land:
                    this.height = 0;
                    this.isFlying = false;
                    break;

                case CommandCode.StartVideo:
                    this.isVideoStreaming = true;
                    break;
                case CommandCode.StopVideo:
                    this.isVideoStreaming = false;
                    break;

                case CommandCode.Left:
                    this.position = this.position.Move(CardinalDirections.Left, (int)command.Arguments[0]);
                    break;
                case CommandCode.Right:
                    this.position = this.position.Move(CardinalDirections.Right, (int)command.Arguments[0]);
                    break;
                case CommandCode.Forward:
                    this.position = this.position.Move(CardinalDirections.Front, (int)command.Arguments[0]);
                    break;
                case CommandCode.Back:
                    this.position = this.position.Move(CardinalDirections.Back, (int)command.Arguments[0]);
                    break;
                case CommandCode.ClockwiseTurn:
                    this.position = this.position.Turn(ClockDirections.Clockwise, (int)command.Arguments[0]);
                    break;
                case CommandCode.CounterClockwiseTurn:
                    this.position = this.position.Turn(ClockDirections.CounterClockwise, (int)command.Arguments[0]);
                    break;
                case CommandCode.Go:
                    this.position = this.position.Go((int)command.Arguments[0], (int)command.Arguments[1]);
                    break;

                case CommandCode.SetSpeed:
                    this.speed = (int)command.Arguments[0];
                    break;

                case CommandCode.Stop:
                    break;

                case CommandCode.Up:
                    this.height += (int)command.Arguments[0];
                    break;
                case CommandCode.Down:
                    this.height -= (int)command.Arguments[0];
                    if (this.height < 0)
                    {
                        this.height = 0;
                    }

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
                    throw new NotSupportedException();
            }
        }
    }
}
