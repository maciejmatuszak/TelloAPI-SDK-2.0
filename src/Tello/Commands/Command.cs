// <copyright file="Command.cs" company="Mark Lauter">
// Copyright (c) Mark Lauter. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Text;

namespace Tello
{
    /// <summary>
    /// https://dl-cdn.ryzerobotics.com/downloads/Tello/Tello%20SDK%202.0%20User%20Guide.pdf.
    /// </summary>
    public sealed class Command
    {
        #region ctor
        public Command()
            : this(CommandCode.EnterSdkMode)
        {
        }

        public Command(CommandCode commandCode)
            : this(commandCode, null)
        {
        }

        public Command(CommandCode commandCode, params object[] args)
        {
            this.Rule = CommandRules.Rules(commandCode);
            this.Validate(commandCode, args);
            this.Arguments = args;
            this.Immediate = this.Rule.Immediate;
        }
        #endregion

        /// <summary>
        /// Gets a value containing the command arguments.
        /// </summary>
        public object[] Arguments { get; }

        /// <summary>
        /// Gets a value indicating whether the command is executed immediately or queued for execution after the current command terminates.
        /// </summary>
        /// <remarks>
        /// Indicates whether or not the flight controller should queue the command (Immediate == false) or send the command immediately (Immediate == true).
        /// Examples of immediate commands include Set4ChannelRC and EmergencyStop.
        /// Examples of non-immediate commands include Move, Land and Flip.
        /// </remarks>
        public bool Immediate { get; }

        /// <summary>
        /// Gets the ruleset that governs the exectution of the command.
        /// </summary>
        public CommandRule Rule { get; }

        #region operators
        public static implicit operator Command(CommandCode commandCode)
        {
            return new Command(commandCode);
        }

        public static implicit operator CommandCode(Command command)
        {
            return command.Rule.CommandCode;
        }

        public static explicit operator Command(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (bytes.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes));
            }

            return (Command)Encoding.UTF8.GetString(bytes);
        }

        public static explicit operator string(Command command)
        {
            return command?.ToString();
        }

        public static explicit operator Command(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            var tokens = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            var rule = CommandRules.Rules(tokens[0]);

            if (rule.Arguments.Length != tokens.Length - 1)
            {
                throw new ArgumentOutOfRangeException($"{rule.CommandCode}: argument count mismatch. expected: {rule.Arguments.Length} actual: {tokens.Length - 1}");
            }

            if (rule.Arguments.Length == 0)
            {
                return new Command(rule.CommandCode);
            }
            else
            {
                var args = new object[rule.Arguments.Length];
                for (var i = 0; i < rule.Arguments.Length; ++i)
                {
                    args[i] = Convert.ChangeType(tokens[i + 1], rule.Arguments[i].Type);
                }

                return new Command(rule.CommandCode, args);
            }
        }

        public static explicit operator ResponseHandleCode(Command command)
        {
            return command.Rule.ResponseHandleCode;
        }

        // todo: move command timeouts to command rules
        public static explicit operator TimeSpan(Command command)
        {
            var avgspeed = 10.0; // cm/s (using a low speed to give margin of error)
            var arcspeed = 15.0; // degrees/s (tested at 30 degress/second, but want to add some margin for error)
            double distance;

            switch (command.Rule.CommandCode)
            {
                case CommandCode.EnterSdkMode:
                case CommandCode.EmergencyStop:
                case CommandCode.GetSpeed:
                case CommandCode.GetBattery:
                case CommandCode.GetTime:
                case CommandCode.GetWIFISnr:
                case CommandCode.GetSdkVersion:
                case CommandCode.GetSerialNumber:
                    return TimeSpan.FromSeconds(30);

                // todo: if I knew the set speed in cm/s I could get a better timeout value
                case CommandCode.Left:
                case CommandCode.Right:
                case CommandCode.Forward:
                case CommandCode.Back:
                    distance = (int)command.Arguments[0]; // cm
                    return TimeSpan.FromSeconds(distance / avgspeed * 10);

                case CommandCode.Go:
                    var x = (int)command.Arguments[0];
                    var y = (int)command.Arguments[1];
                    distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
                    return TimeSpan.FromSeconds(distance / avgspeed * 10);

                case CommandCode.ClockwiseTurn:
                case CommandCode.CounterClockwiseTurn:
                    var degrees = (int)command.Arguments[0];
                    return TimeSpan.FromSeconds(degrees / arcspeed * 10);

                case CommandCode.Takeoff:
                case CommandCode.SetSpeed:
                case CommandCode.SetRemoteControl:
                case CommandCode.SetWiFiPassword:
                case CommandCode.SetStationMode:
                case CommandCode.StartVideo:
                case CommandCode.StopVideo:
                case CommandCode.Flip:
                case CommandCode.Curve:
                case CommandCode.Land:
                case CommandCode.Stop:
                case CommandCode.Up:
                case CommandCode.Down:
                default:
                    return TimeSpan.FromSeconds(60);
            }
        }
        #endregion

        private void Validate(CommandCode commandCode, params object[] args)
        {
            if (args == null && this.Rule.Arguments.Length > 0)
            {
                throw new ArgumentNullException($"{commandCode}: {nameof(args)}");
            }

            if (args != null && args.Length != this.Rule.Arguments.Length)
            {
                throw new ArgumentException(
                    $"{commandCode}: argument count mismatch. expected: {this.Rule.Arguments.Length} actual: {(args == null ? 0 : args.Length)}");
            }

            if (this.Rule.Arguments.Length > 0)
            {
                for (var i = 0; i < args.Length; ++i)
                {
                    var arg = args[i];
                    if (arg == null)
                    {
                        throw new ArgumentNullException($"{commandCode}: {nameof(args)}[{i}]");
                    }

                    var argumentRule = this.Rule.Arguments[i];
                    if (!argumentRule.IsTypeAllowed(arg))
                    {
                        throw new ArgumentException($"{commandCode}: {nameof(args)}[{i}] type mismatch. expected: '{argumentRule.Type.Name}' actual: '{arg.GetType().Name}'");
                    }

                    if (!argumentRule.IsValueAllowed(Convert.ChangeType(arg, argumentRule.Type)))
                    {
                        throw new ArgumentOutOfRangeException($"{commandCode}: {nameof(args)}[{i}] argument out of range: {arg}");
                    }
                }

                switch (commandCode)
                {
                    case CommandCode.Go:
                    case CommandCode.Curve:
                        var twentyCount = 0;
                        for (var i = 0; i < args.Length - 1; ++i)
                        {
                            twentyCount += Math.Abs((int)args[i]) <= 20
                                ? 1
                                : 0;
                            if (twentyCount > 1)
                            {
                                throw new ArgumentOutOfRangeException($"{commandCode}: {nameof(args)} x, y and z can't match /[-20-20]/ simultaneously.");
                            }
                        }

                        break;
                }
            }
        }

        public override string ToString()
        {
            return CommandRules
                .Rules(this.Rule.CommandCode)
                .ToString(this.Arguments);
        }
    }
}