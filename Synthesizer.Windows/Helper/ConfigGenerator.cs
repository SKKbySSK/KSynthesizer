using KSynthesizer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Synthesizer.Windows.Helper
{
    class ConfigGenerator
    {
        class ModulationConfig : OscillatorConfig
        {
            private Action<KSynthesizer.Filters.FrequencyModulationFilter> setupModulator;
            public ModulationConfig(Action<KSynthesizer.Filters.FrequencyModulationFilter> setupModulator)
            {
                this.setupModulator = setupModulator;
            }

            //protected override IAudioSource CreateSource(int sampleRate)
            //{
            //    var source = base.CreateSource(sampleRate);
            //    var mod = new KSynthesizer.Filters.FrequencyModulationFilter(source);
            //    setupModulator(mod);
            //    return mod;
            //}
        }

        public static List<OscillatorConfig> GenerateConfig((KSynthesizer.Sources.FunctionType, float)[] freqs, Action<KSynthesizer.Filters.FrequencyModulationFilter> setupModulator)
        {
            var configurations = new List<OscillatorConfig>();
            foreach(var data in freqs)
            {
                configurations.Add(new ModulationConfig(setupModulator)
                {
                    Function = data.Item1,
                    Frequency = data.Item2,
                });
            }
            return configurations;
        }
    }
}
