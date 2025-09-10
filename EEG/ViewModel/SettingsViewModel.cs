using EEG.Tool;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEG.ViewModel
{
    public class SettingsViewModel: ViewModelBase
    {
        public KsEEG ksEEG { get;}
        public SettingsViewModel(KsEEG ksEEG)
        {
            this.ksEEG = ksEEG;
        }

    }
}
