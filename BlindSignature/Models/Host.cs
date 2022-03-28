using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using BlindSignature.Annotations;

namespace BlindSignature.Models
{
    public sealed class Host : INotifyPropertyChanged
    {
        private readonly string _name;
        private string _ipAddress;

        public string Name
        {
            get => _name;
            init
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                if (value.Any(symbol => !char.IsDigit(symbol) && symbol != '.'))
                {
                    MessageBox.Show("Неправильный адрес", "Ошибка");
                    return;
                }
                
                _ipAddress = value;
                OnPropertyChanged(nameof(IpAddress));
            }
        }

        public Host(string name, string ipAddress)
        {
            Name = name;
            IpAddress = ipAddress;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}