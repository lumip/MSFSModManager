using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MSFSModManager.GUI.Views
{
    public partial class PackageInstallationProgressView : UserControl
    {
        public PackageInstallationProgressView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}