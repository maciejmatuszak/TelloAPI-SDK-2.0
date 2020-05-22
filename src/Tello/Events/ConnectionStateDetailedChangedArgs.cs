// <copyright file="ConnectionStateChangedArgs.cs" company="Mark Lauter">
// Copyright (c) Mark Lauter. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using Tello.State;

namespace Tello.Events
{
    public sealed class ConnectionStateDetailedChangedArgs : EventArgs
    {
        public ConnectionStateDetailedChangedArgs(ConnectionStateEnum connectionState)
        {
            this.ConnectionState = connectionState;
        }

        public ConnectionStateEnum ConnectionState { get; }
    }
}
