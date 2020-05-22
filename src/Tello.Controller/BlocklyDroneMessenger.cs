using System;
using Messenger;
using Tello.Events;
using Tello.State;

namespace Tello.Controller
{
    public class BlocklyDroneMessenger
    {
        private readonly ITransceiver transceiver;
        private readonly IReceiver stateReceiver;
        private readonly IReceiver videoReceiver;

        public BlocklyDroneMessenger(
            ITransceiver transceiver,
            IReceiver stateReceiver,
            IReceiver videoReceiver)
        {
            this.transceiver = transceiver ?? throw new ArgumentNullException(nameof(transceiver));
            this.stateReceiver = stateReceiver ?? throw new ArgumentNullException(nameof(stateReceiver));
            this.videoReceiver = videoReceiver ?? throw new ArgumentNullException(nameof(videoReceiver));

            this.Controller = new BlocklyFlightController(this.transceiver);

            this.Controller.ConnectionStateChanged +=
                (object sender, ConnectionStateDetailedChangedArgs e) =>
                {
                    if (e.ConnectionState == ConnectionStateEnum.Connected)
                    {
                        this.StartLisenters();
                    }
                    else
                    {
                        this.StopListeners();
                    }
                };

            this.StateObserver = new StateObserver(this.stateReceiver);
            this.StateObserver.StateChanged += this.Controller.UpdateState;
            this.Controller.PositionChanged += this.StateObserver.UpdatePosition;

            this.VideoObserver = new VideoObserver(this.videoReceiver);
        }

        #region Listeners

        private void StartLisenters()
        {
            this.stateReceiver.Start();
            this.videoReceiver.Start();
        }

        private void StopListeners()
        {
            this.stateReceiver.Stop();
            this.videoReceiver.Stop();
        }

        #endregion

        public BlocklyFlightController Controller { get; }

        public IStateObserver StateObserver { get; }

        public IVideoObserver VideoObserver { get; }
    }
}