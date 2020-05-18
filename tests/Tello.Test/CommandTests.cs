// <copyright file="CommandTests.cs" company="Mark Lauter">
// Copyright (c) Mark Lauter. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tello.Test
{
    [TestClass]
    public class CommandTests
    {
        [TestMethod]
        public void CardinalDirections_enum_cast_tostring()
        {
            Assert.AreEqual('b', (char)(CardinalDirection)CardinalDirections.Back);
            Assert.AreEqual('f', (char)(CardinalDirection)CardinalDirections.Front);
            Assert.AreEqual('l', (char)(CardinalDirection)CardinalDirections.Left);
            Assert.AreEqual('r', (char)(CardinalDirection)CardinalDirections.Right);
        }

        [TestMethod]
        public void Commands_enum_cast_to_command()
        {
            var command = new Command(CommandCode.EnterSdkMode);
            Assert.AreEqual(CommandCode.EnterSdkMode, command.Rule.CommandCode);
            Assert.IsTrue(command.Immediate);
            Assert.IsNull(command.Arguments);

            command = new Command(CommandCode.Forward, 20);
            Assert.AreEqual(CommandCode.Forward, command.Rule.CommandCode);
            Assert.IsFalse(command.Immediate);
            Assert.IsNotNull(command.Arguments);
            Assert.AreEqual(1, command.Arguments.Length);

            command = new Command(CommandCode.Flip, (char)(CardinalDirection)CardinalDirections.Back);
            Assert.AreEqual(CommandCode.Flip, command.Rule.CommandCode);
            Assert.IsFalse(command.Immediate);
            Assert.IsNotNull(command.Arguments);
            Assert.AreEqual(1, command.Arguments.Length);

            Assert.ThrowsException<ArgumentNullException>(() => new Command(CommandCode.Forward));

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Command(CommandCode.Forward, 5));

            Assert.ThrowsException<ArgumentException>(() => new Command(CommandCode.Forward, 20, 20));
        }
    }
}
