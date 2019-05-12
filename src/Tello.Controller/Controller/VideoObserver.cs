﻿using Messenger;
using System;
using System.Collections.Generic;

namespace Tello.Controller
{
    internal sealed class VideoObserver : Observer<IEnvelope>
    {
        public VideoObserver(IReceiver receiver) : base(receiver)
        {
        }

        public override void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public override void OnNext(IEnvelope message)
        {
            _videoSegments.Enqueue(message);
        }

        private Queue<IEnvelope> _videoSegments = new Queue<IEnvelope>();

        public Queue<IEnvelope> VideoSegments
        {
            get
            {
                var result = _videoSegments;
                _videoSegments = new Queue<IEnvelope>();
                return result;
            }
        }
    }
}