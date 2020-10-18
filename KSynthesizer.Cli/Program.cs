using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace KSynthesizer.Cli
{
    class Program : ConsoleAppBase
    {
        static async Task Main(string[] args)
        {
            // target T as ConsoleAppBase.
            await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);
        }

        private SynthesizerPlayer player = new SynthesizerPlayer();

        public void Run([Option("d", "Output device index")] int? device = null,
            [Option("r", "Sample rate")] int sampleRate = 48000)
        {
            var dev = player.Output.DefaultDevice;
            if (device != null)
            {
                dev = player.Output.Devices[device.Value];
            }

            var format = new AudioFormat(sampleRate, 1, 32);
            player.Initialize(dev, format);
            player.Run(new TestToneInput());
        }
    }
}
