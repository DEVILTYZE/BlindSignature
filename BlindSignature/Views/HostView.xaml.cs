using BlindSignature.ViewModels;

namespace BlindSignature.Views
{
    public partial class HostView
    {
        public HostView(HostViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}