using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MSFSModManager.GUI.Views
{
    public partial class DependencyResolutionPageView : UserControl
    {
        public DependencyResolutionPageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}