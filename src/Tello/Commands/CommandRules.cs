// <copyright file="CommandRules.cs" company="Mark Lauter">
// Copyright (c) Mark Lauter. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;

// ------------ error messages I've seen -------------
// error
// error Motor Stop
// error Not Joystick
// error Auto land
namespace Tello
{
    internal static class CommandRules
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "It's easier to read my way.")]
        static CommandRules()
        {
            var emptyArgs = new ArgumentRule[0];

            var movementArgs = new ArgumentRule[]
            {
                new IntegerRule(new IntegerRule.Range<int>(20, 500)),
            };

            var turnArgs = new ArgumentRule[]
            {
                new IntegerRule(new IntegerRule.Range<int>(1, 360)),
            };

            RulesByCommand = new Dictionary<CommandCode, CommandRule>()
            {
                { CommandCode.EnterSdkMode, new CommandRule(CommandCode.EnterSdkMode, ResponseHandleCode.Ok, "command", emptyArgs, false, true) },
                { CommandCode.Takeoff, new CommandRule(CommandCode.Takeoff, ResponseHandleCode.Ok, "takeoff", emptyArgs, false) },
                { CommandCode.Land, new CommandRule(CommandCode.Land, ResponseHandleCode.Ok, "land", emptyArgs, true) },
                { CommandCode.Stop, new CommandRule(CommandCode.Stop, ResponseHandleCode.Ok, "stop", emptyArgs, true, true) },
                { CommandCode.StartVideo, new CommandRule(CommandCode.StartVideo, ResponseHandleCode.Ok, "streamon", emptyArgs, false) },
                { CommandCode.StopVideo, new CommandRule(CommandCode.StopVideo, ResponseHandleCode.Ok, "streamoff", emptyArgs, false) },
                { CommandCode.EmergencyStop, new CommandRule(CommandCode.EmergencyStop, ResponseHandleCode.Ok, "emergency", emptyArgs, false, true) },
                { CommandCode.GetSpeed, new CommandRule(CommandCode.GetSpeed, ResponseHandleCode.Speed, "speed?", emptyArgs, false) },
                { CommandCode.GetBattery, new CommandRule(CommandCode.GetBattery, ResponseHandleCode.Battery, "battery?", emptyArgs, false) },
                { CommandCode.GetTime, new CommandRule(CommandCode.GetTime, ResponseHandleCode.Time, "time?", emptyArgs, false) },
                { CommandCode.GetWIFISnr, new CommandRule(CommandCode.GetWIFISnr, ResponseHandleCode.WIFISnr, "wifi?", emptyArgs, false) },
                { CommandCode.GetSdkVersion, new CommandRule(CommandCode.GetSdkVersion, ResponseHandleCode.SdkVersion, "sdk?", emptyArgs, false) },
                { CommandCode.GetSerialNumber, new CommandRule(CommandCode.GetSerialNumber, ResponseHandleCode.SerialNumber, "sn?", emptyArgs, false) },
                { CommandCode.Up, new CommandRule(CommandCode.Up, ResponseHandleCode.Ok, "up", movementArgs, true) },
                { CommandCode.Down, new CommandRule(CommandCode.Down, ResponseHandleCode.Ok, "down", movementArgs, true) },
                { CommandCode.Left, new CommandRule(CommandCode.Left, ResponseHandleCode.Ok, "left", movementArgs, true) },
                { CommandCode.Right, new CommandRule(CommandCode.Right, ResponseHandleCode.Ok, "right", movementArgs, true) },
                { CommandCode.Forward, new CommandRule(CommandCode.Forward, ResponseHandleCode.Ok, "forward", movementArgs, true) },
                { CommandCode.Back, new CommandRule(CommandCode.Back, ResponseHandleCode.Ok, "back", movementArgs, true) },
                { CommandCode.ClockwiseTurn, new CommandRule(CommandCode.ClockwiseTurn, ResponseHandleCode.Ok, "cw", turnArgs, true) },
                { CommandCode.CounterClockwiseTurn, new CommandRule(CommandCode.CounterClockwiseTurn, ResponseHandleCode.Ok, "ccw", turnArgs, true) },
                {
                    CommandCode.SetSpeed, new CommandRule(
                        CommandCode.SetSpeed,
                        ResponseHandleCode.Ok,
                        "speed",
                        new ArgumentRule[] { new IntegerRule(new IntegerRule.Range<int>(10, 100)) },
                        false)
                },
                {
                    CommandCode.Flip, new CommandRule(
                        CommandCode.Flip,
                        ResponseHandleCode.Ok,
                        "flip",
                        new ArgumentRule[] { new CharacterRule("lrfb") },
                        true)
                },
                {
                    CommandCode.Go, new CommandRule(
                        CommandCode.Go,
                        ResponseHandleCode.Ok,
                        "go",
                        new ArgumentRule[]
                        {
                            new IntegerRule(new IntegerRule.Range<int>(-500, 500)),
                            new IntegerRule(new IntegerRule.Range<int>(-500, 500)),
                            new IntegerRule(new IntegerRule.Range<int>(-500, 500)),
                            new IntegerRule(new IntegerRule.Range<int>(10, 100)),
                        },
                        true)
                },
                {
                    CommandCode.Curve, new CommandRule(
                        CommandCode.Curve,
                        ResponseHandleCode.Ok,
                        "curve",
                        new ArgumentRule[]
                        {
                            new IntegerRule(new IntegerRule.Range<int>(-500, 500)),
                            new IntegerRule(new IntegerRule.Range<int>(-500, 500)),
                            new IntegerRule(new IntegerRule.Range<int>(-500, 500)),
                            new IntegerRule(new IntegerRule.Range<int>(-500, 500)),
                            new IntegerRule(new IntegerRule.Range<int>(-500, 500)),
                            new IntegerRule(new IntegerRule.Range<int>(-500, 500)),
                            new IntegerRule(new IntegerRule.Range<int>(10, 60)),
                        },
                        true)
                },
                {
                    CommandCode.SetRemoteControl, new CommandRule(
                        CommandCode.SetRemoteControl,
                        ResponseHandleCode.None,
                        "rc",
                        new ArgumentRule[]
                        {
                            new IntegerRule(new IntegerRule.Range<int>(-100, 100)),
                            new IntegerRule(new IntegerRule.Range<int>(-100, 100)),
                            new IntegerRule(new IntegerRule.Range<int>(-100, 100)),
                            new IntegerRule(new IntegerRule.Range<int>(-100, 100)),
                        },
                        true,
                        true)
                },
                {
                    CommandCode.SetWiFiPassword, new CommandRule(
                        CommandCode.SetWiFiPassword,
                        ResponseHandleCode.Ok,
                        "wifi",
                        new ArgumentRule[]
                        {
                            new StringRule(),
                            new StringRule(),
                        },
                        false)
                },
                {
                    CommandCode.SetStationMode, new CommandRule(
                        CommandCode.SetStationMode,
                        ResponseHandleCode.Ok,
                        "ap",
                        new ArgumentRule[]
                        {
                            new StringRule(),
                            new StringRule(),
                        },
                        false)
                },

                // {Commands.SetMissionPadOn, new CommandRule(Commands.SetMissionPadOn, Responses.Ok,"", null) },
                // {Commands.SetMissionPadOff, new CommandRule(Commands.SetMissionPadOff,Responses.Ok, "", null) },
                // {Commands.SetMissionPadDirection, new CommandRule(Commands.SetMissionPadDirection, Responses.Ok,"", null) },
            };

            RulesByString = RulesByCommand
                .Values
                .ToDictionary((rule) => rule.Token);
        }

        private static readonly Dictionary<CommandCode, CommandRule> RulesByCommand;
        private static readonly Dictionary<string, CommandRule> RulesByString;

        public static CommandRule Rules(CommandCode commandCode)
        {
            return RulesByCommand[commandCode];
        }

        public static CommandRule Rules(string command)
        {
            return RulesByString[command];
        }
    }
}