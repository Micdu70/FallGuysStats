using System;
#if Debug
using System.Diagnostics;
#endif
using System.Net.NetworkInformation;
using System.Threading;

namespace FallGuysStats {
    public class ServerPingWatcher {
        private const int UpdateDelay = 2000;

        private bool running;
        private bool stop;
        private Thread watcher;

        private readonly Ping pingSender = new Ping();
        private PingReply reply;

        private readonly Random random = new Random();
        private int randomElement;
        private readonly int[] moreDelayValues = { 100, 200, 300, 400, 500 };
        private int addMoreRandomDelay;

        public void Start() {
            if (this.running) { return; }

#if Debug
            Debug.WriteLine("ServerPingWatcher is starting!");
#endif

            this.stop = false;
            this.watcher = new Thread(CheckServerPing) { IsBackground = true };
            this.watcher.Start();
        }

        private void CheckServerPing() {
            this.running = true;
            while (!stop) {
                try {
                    if (!Stats.InShow || string.IsNullOrEmpty(Stats.CurrentServerIp)) {
                        this.stop = true;
                        this.running = false;
                        return;
                    }
                    this.reply = this.pingSender.Send(Stats.CurrentServerIp, 1000, new byte[32]);
                    if (this.reply.Status == IPStatus.Success) {
                        Stats.IsLastServerPingFailed = false;
                        Stats.LastGoodServerPing = this.reply.RoundtripTime;
                    } else {
                        Stats.IsLastServerPingFailed = true;
                    }
                    this.randomElement = this.random.Next(0, this.moreDelayValues.Length);
                    this.addMoreRandomDelay = this.moreDelayValues[this.randomElement];
                } catch {
                    Stats.IsLastServerPingFailed = true;
                    this.randomElement = this.random.Next(0, this.moreDelayValues.Length);
                    this.addMoreRandomDelay = this.moreDelayValues[this.randomElement];
                }
                Thread.Sleep(UpdateDelay + addMoreRandomDelay);
            }
        }
    }
}