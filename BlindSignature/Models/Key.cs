using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BlindSignature.Annotations;
using BlindSignature.Helpers;

namespace BlindSignature.Models
{
    public sealed class Key : INotifyPropertyChanged
    {
        private int _module;
        private int _value;

        public int Module
        {
            get => _module;
            set
            {
                _module = value;
                OnPropertyChanged(nameof(Module));
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public Key(int module, int value)
        {
            Module = module;
            Value = value;
        }

        public Key(byte[] array) => GetDataFromByteArray(array);

        public byte[] GetHashValue()
        {
            var moduleHash = Encoding.UTF8.GetBytes(Module.ToString());
            var valueHash = Encoding.UTF8.GetBytes(Value.ToString());

            return moduleHash.Concat(ConstHelper.Separator).Concat(valueHash).ToArray();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void GetDataFromByteArray(byte[] array)
        {
            var (index1, index2) = ByteArrayHelper.GetOffsetOfSeparator(array);
            Value = int.Parse(Encoding.UTF8.GetString(new ArraySegment<byte>(array, index2, 
                array.Length - index2)));
            Module = int.Parse(Encoding.UTF8.GetString(new ArraySegment<byte>(array, 0, index1)));
        }
    }
}