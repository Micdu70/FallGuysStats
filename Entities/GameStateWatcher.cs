using System;
using System.Diagnostics;
using System.Threading;

namespace FallGuysStats {
    public class GameStateWatcher {
        private const int UpdateDelay = 3000;

        private bool running;
        private bool stop;
        private Thread watcher;

        public event Action<string> OnError;

        public void Start() {
            if (this.running) { return; }

#if Debug
            Debug.WriteLine("GameStateWatcher is starting!");
#endif

            Stats.IsGameHasBeenClosed = false;
            this.stop = false;
            this.watcher = new Thread(CheckGameState) { IsBackground = true };
            this.watcher.Start();
        }

        private void CheckGameState() {
            this.running = true;
            while (!stop) {
                try {
                    if (!Stats.InShow) {
                        this.stop = true;
                        this.running = false;
                        return;
                    }
                    Process[] fallGuysProcessName = Process.GetProcessesByName("FallGuys_client_game");
                    if (fallGuysProcessName.Length == 0) {
                        Stats.IsGameHasBeenClosed = true;
                        this.stop = true;
                        this.running = false;
                    }
                } catch (Exception ex) {
                    this.OnError?.Invoke(ex.Message);
                }
                Thread.Sleep(UpdateDelay);
            }
        }
    }
}