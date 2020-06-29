using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSynthesizer.Windows.ViewModels
{
    public class WaveFormViewViewModel : BindableBase
    {
        public WaveFormViewViewModel()
        {

        }

        private DelegateCommand _saveImageCmd;
        public DelegateCommand SaveImageCommand =>
            _saveImageCmd ?? (_saveImageCmd = new DelegateCommand(ExecuteSaveImageCommand));

        void ExecuteSaveImageCommand()
        {
        }
    }
}
