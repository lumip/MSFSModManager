using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MSFSModManager.GUI.Views
{
    public partial class InstallPageView : UserControl
    {
        public InstallPageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}