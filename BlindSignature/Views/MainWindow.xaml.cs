using System;
using System.Windows;
using System.Windows.Controls;
using BlindSignature.Annotations;
using BlindSignature.ViewModels;

namespace BlindSignature.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly SignViewModel _model;
        
        public MainWindow()
        {
            InitializeComponent();

            _model = (SignViewModel)DataContext;
            CheckBox1.IsChecked = CheckBox0.IsChecked = true;
        }

        private void InfoHostButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new HostView(_model.HostModel) { Owner = this };
            window.ShowDialog();
        }

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_model.SendMessage())
                MessageBox.Show("Текст сообщения пуст", "Внимание", MessageBoxButton.OK, 
                    MessageBoxImage.Information);
        }
        
        private void WaitButton_OnClick(object sender, RoutedEventArgs e) => _model.WaitConnection();

        private void ExportLogsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_model.ExportLogs())
                MessageBox.Show("Логи пусты", "Внимание", MessageBoxButton.OK, 
                    MessageBoxImage.Information);
        }

        private void StopWaitButton_OnClick(object sender, RoutedEventArgs e) => _model.IsServerWorking = false;

        private void MainWindow_OnClosed([CanBeNull] object sender, EventArgs e)
        {
            _model.IsServerWorking = false;
            _model.WaitThread();
        }

        private void CheckBox_OnChangeCheckValue(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var isParsed = int.TryParse(checkBox.Name[^1..], out var number);            
            
            if (checkBox.IsChecked is null || !isParsed)
                return;

            _model.IsRandomValues[number] = checkBox.IsChecked.Value;
        }
    }
}