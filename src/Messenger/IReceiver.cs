﻿// <copyright file="IReceiver.cs" company="Mark Lauter">
// Copyright (c) Mark Lauter. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Messenger
{
    public interface IReceiver : IObservable<IEnvelope>
    {
        void Start();

        void Stop();
    }
}



