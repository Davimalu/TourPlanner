using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace TourPlanner.Views
{
    /// <summary>
    /// Interaction logic for MenuBar.xaml
    /// </summary>
    public partial class MenuBar : UserControl
    {
        public MenuBar()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// handles switching between light and dark theme
        /// </summary>
        private void ThemeToggleSwitch_Click(object sender, RoutedEventArgs e)
        {
            // This method is directly dependent on WPF / WPF-UI, thus it can reside in the Code Behind
            bool isDarkTheme = ThemeToggleSwitch.IsChecked ?? false;
            
            if (isDarkTheme)
            {
                // Apply Dark Theme
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                    Wpf.Ui.Appearance.ApplicationTheme.Dark,
                    Wpf.Ui.Controls.WindowBackdropType.None,
                    true
                );
            }
            else
            {
                // Apply Light Theme
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                    Wpf.Ui.Appearance.ApplicationTheme.Light,
                    Wpf.Ui.Controls.WindowBackdropType.None, // For whatever reason, this is the only way to get the light theme to work | https://github.com/lepoco/wpfui/issues/1222
                    true
                );
            }
        }
        
        // Theme Switching seems to be really buggy | https://github.com/lepoco/wpfui/issues
    }
}
