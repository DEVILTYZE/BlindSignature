using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using BlindSignature.Annotations;
using BlindSignature.Helpers;
using BlindSignature.Models;

namespace BlindSignature.ViewModels
{
    public sealed class SignViewModel : INotifyPropertyChanged
    {
        private readonly HostViewModel _hostModel;
        private string _exceptionMessage, _message, _logs;
        private Key _openKey;
        private readonly Key _closedKey;
        private int _randomValue;
        private bool _isThreadWaiting;
        private bool _isServerWorking;

        private bool _isConnected;
        private Thread _thread;
        private TcpClient _client;
        private TcpListener _server;

        public HostViewModel HostModel
        {
            get => _hostModel;
            init
            {
                _hostModel = value;
                OnPropertyChanged(nameof(HostModel));
            }
        }
        
        public string ExceptionMessage
        {
            get => _exceptionMessage;
            set
            {
                _exceptionMessage = value;
                OnPropertyChanged(nameof(ExceptionMessage));
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

        public string Logs
        {
            get => _logs;
            set
            {
                _logs = value;
                OnPropertyChanged(nameof(Logs));
            }
        }

        public Key OpenKey
        {
            get => _openKey;
            set
            {
                _openKey = value;
                OnPropertyChanged(nameof(OpenKey));
            }
        }

        public Key ClosedKey
        {
            get => _closedKey;
            init
            {
                _closedKey = value;
                OnPropertyChanged(nameof(ClosedKey));
            }
        }

        public int RandomValue
        {
            get => _randomValue;
            set
            {
                _randomValue = value;
                OnPropertyChanged(nameof(RandomValue));
            }
        }

        public bool IsThreadWaiting
        {
            get => _isThreadWaiting;
            set
            {
                _isThreadWaiting = value;
                OnPropertyChanged(nameof(IsThreadWaiting));
            }
        }

        public bool IsServerWorking
        {
            get => _isServerWorking;
            set
            {
                _isServerWorking = value;
                OnPropertyChanged(nameof(IsServerWorking));
            }
        }
        
        public SignViewModel()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
                HostModel = new HostViewModel();

            IsThreadWaiting = true;
            Logs = string.Empty;
            (OpenKey, ClosedKey) = BlindSignatureGenerator.GenerateRandomKeys();
        }

        public bool SendMessage()
        {
            if (string.IsNullOrEmpty(Message))
                return false;
            
            IsThreadWaiting = false;
            
            _thread = new Thread(SendMessage);
            _thread.Start((HostModel.OtherHost.Name, ConstHelper.Port));
            WaitThread();

            return true;
        }

        private void SendMessage(object obj)
        {
            const int dataLength = ConstHelper.StreamLength * 16;
            
            var (name, port) = ((string, int))obj;
            ExceptionMessage = Logs = string.Empty;
            NetworkStream stream = null;
            var data = new byte[dataLength];

            try
            {
                // Set client to send status.
                _client = new TcpClient(name, port);

                // Create a message.
                var message = new MessageModel(HostModel.OurHost.Name, Message) { IsSigned = true };
                Logs += "Оригинальное сообщение: " + message.Message + "\r\n";
                
                // Get open key.
                stream = _client.GetStream();
                stream.Write(ByteArrayHelper.GetCommandAndDataByteArray(ConstHelper.CommandGetOpenKey));
                stream.Read(data, 0, data.Length);
                
                if (ByteArrayHelper.IsInvalidData(data))
                    throw new Exception("Открытый ключ равняется null!");
                
                OpenKey = new Key(ByteArrayHelper.RemoveEndSeparator(data));
                Logs += "Открытый ключ: " + OpenKey.Module + " / " + OpenKey.Exponent + "\r\n";

                // Get signed message by us.
                var number = new IntNumberArray(message.GetByteArray());
                var originalMessageNumber = (IntNumberArray)number.Clone();
                Logs += "Чистое сообщение: " + number + "\r\n";
                (RandomValue, number) = BlindSignatureGenerator.SignByOpenKey(number, OpenKey);
                Logs += "Отправленное подписанное сообщение: " + number + "\r\n";
                
                // Sent signed message by us.
                data = ByteArrayHelper.GetCommandAndDataByteArray(ConstHelper.CommandSignMessage, number.GetBytes());
                stream.Write(data, 0, data.Length);

                // Get signed message by us and other user.
                data = new byte[dataLength];
                stream.Read(data, 0, data.Length);
                data = ByteArrayHelper.RemoveEndSeparator(data);
                
                if (ByteArrayHelper.IsInvalidData(data))
                    throw new Exception("Подписанное сообщение равняется null!");
                
                number = IntNumberArray.GetNumber(data);
                Logs += "Полученное подписанное сообщение: " + number + "\r\n";

                // Remove our sign.
                number = BlindSignatureGenerator.RemoveSignByOpenKey(number, OpenKey, RandomValue);
                Logs += "Сообщение с удалённой подписью: " + number + "\r\n";

                // Check sign.
                // var messageByteArray = message.GetByteArray();
                stream.Write(ByteArrayHelper.GetCommandAndDataByteArray(ConstHelper.CommandSignMessage, 
                    originalMessageNumber.GetBytes()));
                data = new byte[dataLength];
                stream.Read(data, 0, data.Length);
                data = ByteArrayHelper.RemoveEndSeparator(data);
                
                if (ByteArrayHelper.IsInvalidData(data))
                    throw new Exception("Подписанное сообщение равняется null!");
                
                originalMessageNumber = IntNumberArray.GetNumber(data);
                Logs += "Полученное подписанное сообщение: " + originalMessageNumber + "\r\n";
                Logs += "Статус подписи: " + (number.CompareTo(originalMessageNumber) == 0 ? "УСПЕШНО\r\n" : "НЕУСПЕШНО\r\n");
                
                // End connection.
                stream.Write(ByteArrayHelper.GetCommandAndDataByteArray(ConstHelper.CommandEndConnection));
            }
            catch (ArgumentNullException e)
            {
                ExceptionMessage = "Ошибка соединения! " + e.Message;
            }
            catch (SocketException e)
            {
                ExceptionMessage = "Ошибка соединения! " + e.Message;
            }
            catch (Exception e)
            {
                ExceptionMessage = "Ошибка! " + e.Message;
            }
            finally
            {
                stream?.Close();
                
                if (_client.Connected)
                    _client.Close();
            }
        }

        public void WaitConnection()
        {
            IsServerWorking = true;
            IsThreadWaiting = false;
            
            _thread = new Thread(new ParameterizedThreadStart(WaitConnection));
            _thread.Start((IPAddress.Parse(HostModel.OurHost.IpAddress), ConstHelper.Port));
            WaitThread();
        }

        private void WaitConnection(object obj)
        {
            var (ipAddress, port) = ((IPAddress, int))obj;
            ExceptionMessage = Logs = string.Empty;
            NetworkStream stream = null;

            try
            {
                // Set server to listen.
                _server = new TcpListener(ipAddress, port);
                _server.Start();

                while (true)
                {
                    // Waiting connection.
                    Logs += "Ожидание подключения...\r\n";
                    var client = _server.AcceptTcpClient();
                    Logs += "Подключено\r\n";
                    _isConnected = true;

                    while (_isConnected)
                    {
                        var data = new byte[ConstHelper.StreamLength * 16];
                        stream = client.GetStream();
                        
                        do stream.Read(data, 0, data.Length);
                        while (stream.DataAvailable);
                        
                        // Get command and data.
                        data = ByteArrayHelper.RemoveEndSeparator(data);
                        var (index, offset) = ByteArrayHelper.GetOffsetOfSeparator(data);
                        string command;
                        var number = new IntNumberArray();

                        if (index != -1)
                        {
                            command = Encoding.UTF8.GetString(new ArraySegment<byte>(data, 0, index).ToArray());
                            //data = new ArraySegment<byte>(data, offset, data.Length - offset).ToArray();
                            number = IntNumberArray.GetNumber(new ArraySegment<byte>(data, offset, data.Length - offset).ToArray());
                        }
                        else command = Encoding.UTF8.GetString(data);

                        Logs += "Команда: " + command + "\r\nДанные: " + (index != -1 ? number : "нет данных") + "\r\n";

                        // Get and send our response.
                        var response = SelectAndExecuteCommand(command, number).Concat(ConstHelper.Separator).ToArray();
                        stream.Write(response, 0, response.Length);
                        
                        if (string.CompareOrdinal(command, ConstHelper.CommandEndConnection) != 0)
                            Logs += "Отправленные данные: " + string.Join(" ", response) + "\r\n";
                    }

                    if (IsServerWorking)
                        continue;

                    client.Close();
                    break;
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10004)
                    Logs += "Передача прервана вручную";
                else
                    ExceptionMessage = "Ошибка соединения! " + e.Message;
            }
            catch (Exception e)
            {
                ExceptionMessage = "Ошибка! " + e.Message;
            }
            finally
            {
                stream?.Close();
                _server.Stop();
            }
        }

        public bool ExportLogs()
        {
            if (Logs.Length == 0)
                return false;
            
            IsThreadWaiting = false;
            
            _thread = new Thread(() =>
            {
                using var sw = new StreamWriter("log_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".txt");
                sw.Write(Logs);
            });
            _thread.Start();
            WaitThread();

            return true;
        }

        public void WaitThread()
        {
            if (_thread is null || !_thread.IsAlive)
            {
                IsThreadWaiting = true;
                return;
            }
            
            var thread = new Thread(() => 
            { 
                if (IsServerWorking && (_server is null || _server.Server.Blocking))
                    while (true)
                    {
                        if (!IsServerWorking && _server is not null)
                        {
                            _server.Stop();
                            _isConnected = false;
                            break;
                        }
                        
                        Thread.Sleep(20);
                    }

                _thread.Join();
                IsThreadWaiting = true;
            });
            thread.Start();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private IEnumerable<byte> SelectAndExecuteCommand(string command, [CanBeNull] IntNumberArray number)
        {
            var commands = new (string, Func<IEnumerable<byte>>)[]
            {
                (ConstHelper.CommandGetOpenKey, () => OpenKey.GetBytes()),
                (ConstHelper.CommandSignMessage, () => BlindSignatureGenerator.SignByClosedKey(number, ClosedKey).GetBytes()),
                (ConstHelper.CommandEndConnection, () =>
                {
                    _isConnected = false; 
                    return Encoding.UTF8.GetBytes("Соединение завершено");
                })
            };

            foreach (var (commandString, function) in commands)
                if (IsEqualsCommands(command, commandString) && function is not null)
                    return function.Invoke();

            return new[] { byte.MaxValue };
        }

        private static bool IsEqualsCommands(string command1, string command2)
        {
            if (command1.Length != command2.Length)
                return false;

            return !command1.Where((thisCommand, i) => thisCommand != command2[i]).Any();
        }
    }
}