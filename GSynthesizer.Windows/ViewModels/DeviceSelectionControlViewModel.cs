using NAudio.CoreAudioApi;
using NAudio.Wave;
using Prism.Commands;
using Prism.Mvvm;
using SoundIOSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSynthesizer.Windows.ViewModels
{
    public class DeviceSelectionControlViewModel : BindableBase
    {
        private readonly DeviceSettings deviceSettings;
        private readonly MainSynthesizer mainSynthesizer;

        public DeviceSelectionControlViewModel(DeviceSettings device, MainSynthesizer synthesizer)
        {
            deviceSettings = device;
            mainSynthesizer = synthesizer;
            SelectedDevice = Devices.FirstOrDefault();
            deviceSettings.PropertyChanged += DeviceSettings_PropertyChanged;

            DeviceSelectionEnabled = !deviceSettings.IsInitialized;
            ButtonText = deviceSettings.IsInitialized ? "Stop" : "Start";
        }

        private void DeviceSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DeviceSelectionEnabled = !deviceSettings.IsInitialized;
            ButtonText = deviceSettings.IsInitialized ? "Stop" : "Start";
        }

        public MMDeviceCollection Devices { get; } = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);


        private MMDevice selectedDevice;
        public MMDevice SelectedDevice
        {
            get { return selectedDevice; }
            set { SetProperty(ref selectedDevice, value); }
        }

        private bool deviceSelectionEnabled;
        public bool DeviceSelectionEnabled
        {
            get { return deviceSelectionEnabled; }
            set { SetProperty(ref deviceSelectionEnabled, value); }
        }

        private string buttonText;
        public string ButtonText
        {
            get { return buttonText; }
            set { SetProperty(ref buttonText, value); }
        }

        private DelegateCommand buttonCommand;
        public DelegateCommand ButtonCommand =>
            buttonCommand ?? (buttonCommand = new DelegateCommand(ExecuteButtonCommand));

        void ExecuteButtonCommand()
        {
            if (deviceSettings.IsInitialized)
            {
                mainSynthesizer.Stop();
                deviceSettings.DisposeOutput();
            }
            else
            {
                deviceSettings.Initialize(SelectedDevice);
                mainSynthesizer.Play(deviceSettings.Output);
            }
        }
    }
}
