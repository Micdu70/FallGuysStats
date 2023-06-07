using System;
using System.Diagnostics;
using System.Threading;

namespace FallGuysStats {
    public class GameStateWatcher {
        private const int UpdateDelay = 2000;

        private bool running;
        private bool stop;
        private Thread watcher;

        private bool fallGuysProcessFound;

        public event Action<string> OnError;

        public void Start() {
            if (this.running) { return; }

#if Debug
            Debug.WriteLine("GameStateWatcher is starting!");
#endif

            this.stop = false;
            this.watcher = new Thread(this.CheckGameState) { IsBackground = true };
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
                    this.fallGuysProcessFound = false;
                    foreach (Process process in Process.GetProcesses()) {
                        if (process.ProcessName == "FallGuys_client_game") {
                            Stats.IsGameRunning = true;
                            this.fallGuysProcessFound = true;
                            break;
                        }
                    }
                    if (!this.fallGuysProcessFound) {
                        Stats.IsGameRunning = false;
                        Stats.IsGameHasBeenClosed = true;
                        this.stop = true;
                        this.running = false;
                    }
                } catch (Exception ex) {
                    this.OnError?.Invoke(ex.ToString());
                }
                Thread.Sleep(UpdateDelay);
            }
        }
    }
}