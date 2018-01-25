using System.Windows;

namespace Bingo
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public GameSettings GameSettings { get; set; }

        public SettingsWindow(GameSettings gameSettings)
        {
            GameSettings = gameSettings;
            InitializeComponent();
        }
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}