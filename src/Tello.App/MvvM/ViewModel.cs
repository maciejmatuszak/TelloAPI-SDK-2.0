﻿// <copyright file="ViewModel.cs" company="Mark Lauter">
// Copyright (c) Mark Lauter. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace Tello.App.MvvM
{
    public class OpenEventArgs : EventArgs
    {
        public OpenEventArgs()
            : this(null)
        {
        }

        public OpenEventArgs(Dictionary<string, object> args)
        {
            this.Args = args != null
                ? new Dictionary<string, object>(args)
                : new Dictionary<string, object>();
        }

        public Dictionary<string, object> Args { get; }
    }

    public class ClosingEventArgs : EventArgs
    {
        public bool CanClose { get; set; }
    }

    public class ViewModel : PropertyChangedNotifier
    {
        protected readonly IUINotifier UserNotifier;

        public ViewModel(IUIDispatcher dispatcher, IUINotifier userNotifier)
            : base(dispatcher)
        {
            this.DisplayName = $"#{this.GetType().Name}#";
            this.UserNotifier = userNotifier ?? throw new ArgumentNullException(nameof(userNotifier));
        }

        public string DisplayName { get; set; }

        public void Open(OpenEventArgs args = null)
        {
            this.OnOpen(args);
        }

        public bool Close()
        {
            if (this.CanClose)
            {
                var args = new ClosingEventArgs { CanClose = true };
                this.OnClosing(args);
                this.CanClose = args.CanClose;
            }

            return this.CanClose;
        }

        private bool canClose = true;

        public bool CanClose { get => this.canClose; set => this.SetProperty(ref this.canClose, value); }

        protected virtual void OnOpen(OpenEventArgs args) { }

        protected virtual void OnClosing(ClosingEventArgs args) { }
    }
}
