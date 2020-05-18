// <copyright file="CommandRule.cs" company="Mark Lauter">
// Copyright (c) Mark Lauter. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// ------------ error messages I've seen -------------
// error
// error Motor Stop
// error Not Joystick
// error Auto land
namespace Tello
{
    /// <summary>
    /// CommandRule governs the exection of a command.
    /// </summary>
    public sealed class CommandRule
    {
        internal CommandRule(
            CommandCode commandCode,
            ResponseHandleCode responseHandleCode,
            string token,
            ArgumentRule[] arguments,
            bool mustBeInflight,
            bool immediate = false)
        {
            this.CommandCode = commandCode;
            this.ResponseHandleCode = responseHandleCode;
            this.Token = token;
            this.Arguments = arguments;
            this.MustBeInFlight = mustBeInflight;
            this.Immediate = immediate;
        }

        public CommandCode CommandCode { get; }

        public ResponseHandleCode ResponseHandleCode { get; }

        public string Token { get; }

        public ArgumentRule[] Arguments { get; }

        public bool MustBeInFlight { get; }

        public bool Immediate { get; }

        public string ToString(params object[] args)
        {
            var result = this.Token;

            if (args != null)
            {
                foreach (var arg in args)
                {
                    result += $" {arg}";
                }
            }

            return result;
        }
    }
}