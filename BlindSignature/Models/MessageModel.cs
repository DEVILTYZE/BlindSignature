using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BlindSignature.Annotations;
using BlindSignature.Helpers;

namespace BlindSignature.Models
{
    public sealed class MessageModel : INotifyPropertyChanged
    {
        private string _author, _message;
        private DateTime _dateOfCreation;
        private bool _isSigned;

        public string Author
        {
            get => _author;
            set
            {
                _author = value;
                OnPropertyChanged(nameof(Author));
            }
        }
        
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }
        
        public DateTime DateOfCreation
        {
            get => _dateOfCreation;
            set
            {
                _dateOfCreation = value;
                OnPropertyChanged(nameof(DateOfCreation));
            }
        }
        
        public bool IsSigned
        {
            get => _isSigned;
            set
            {
                _isSigned = value;
                OnPropertyChanged(nameof(IsSigned));
            }
        }

        public MessageModel(string author, string message)
        {
            Author = author;
            Message = message;
            DateOfCreation = DateTime.Now;
        }

        public MessageModel(int hash) => GetDataFromByteArray(BitConverter.GetBytes(hash));

        public override int GetHashCode() => BitConverter.ToInt32(GetByteArray());

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private byte[] GetByteArray()
        {
            var authorHash = Encoding.UTF8.GetBytes(Author).Concat(ConstHelper.Separator);
            var messageHash = Encoding.UTF8.GetBytes(Message).Concat(ConstHelper.Separator);
            var dateHash = Encoding.UTF8.GetBytes(DateOfCreation.ToString(CultureInfo.CurrentCulture))
                .Concat(ConstHelper.Separator);
            var isSignedHash = IsSigned ? new byte[]{ 1 } : new byte[]{ 0 };

            authorHash = authorHash.Concat(messageHash);
            authorHash = authorHash.Concat(dateHash);
            return authorHash.Concat(isSignedHash).ToArray();
        }

        private void GetDataFromByteArray(byte[] array)
        {
            IsSigned = array[^1] == 1;
            
            Array.Resize(ref array, array.Length - 2);
            var pair = ByteArrayHelper.GetOffsetOfSeparator(array);
            
            if (pair.Key != -1)
            {
                var dateString = Encoding.UTF8.GetString(new ArraySegment<byte>(array, pair.Value, 
                    array.Length - pair.Value));
                DateOfCreation = DateTime.Parse(dateString, CultureInfo.CurrentCulture);
                Array.Resize(ref array, pair.Key - 1);
                pair = ByteArrayHelper.GetOffsetOfSeparator(array);
                
                if (pair.Key != -1)
                    Message = Encoding.UTF8.GetString(new ArraySegment<byte>(array, pair.Value, 
                        array.Length - pair.Value));
            }
            
            Author = Encoding.UTF8.GetString(new ArraySegment<byte>(array, 0, pair.Key));
        }
    }
}