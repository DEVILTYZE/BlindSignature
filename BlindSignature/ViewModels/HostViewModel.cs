using System.ComponentModel;
using System.Runtime.CompilerServices;
using BlindSignature.Annotations;
using BlindSignature.Helpers;
using BlindSignature.Models;

namespace BlindSignature.ViewModels
{
    public sealed class HostViewModel : INotifyPropertyChanged
    {
        private readonly Host _ourHost, _otherHost;

        public Host OurHost
        {
            get => _ourHost;
            init
            {
                _ourHost = value;
                OnPropertyChanged(nameof(OurHost));
            }
        }
        
        public Host OtherHost
        {
            get => _otherHost;
            init
            {
                _otherHost = value;
                OnPropertyChanged(nameof(OtherHost));
            }
        }

        public HostViewModel()
        {
            OurHost = new Host(ConstHelper.LocalName, ConstHelper.LocalAddress);
            OtherHost = new Host(ConstHelper.LocalName, ConstHelper.LocalAddress);
        }

        public HostViewModel(string ourName, string ourIp, string otherName, string otherIp)
        {
            OurHost = new Host(ourName, ourIp);
            OtherHost = new Host(otherName, otherIp);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}