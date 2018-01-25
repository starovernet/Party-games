using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Bingo
{
    public partial class MainWindow
    {
        public static readonly DependencyProperty CurrentNumberProperty = DependencyProperty.Register(
            "CurrentNumber", typeof(int?), typeof(MainWindow), new PropertyMetadata(default(int?)));

        public int? CurrentNumber
        {
            get { return (int?) GetValue(CurrentNumberProperty); }
            set { SetValue(CurrentNumberProperty, value); }
        }

        private IList<int> _generatedNumbers;

        public static readonly DependencyProperty GameSettingsProperty = DependencyProperty.Register(
            "GameSettings", typeof(GameSettings), typeof(MainWindow), new PropertyMetadata(default(GameSettings)));

        public GameSettings GameSettings
        {
            get { return (GameSettings) GetValue(GameSettingsProperty); }
            set { SetValue(GameSettingsProperty, value); }
        }

        private int CountOfGameNumbers => GameSettings.LastNumber - GameSettings.FirstNumber + 1;

        private readonly Random _random = new Random();
        private volatile int _totalNumbersOnGeneration;

        public MainWindow()
        {
            GameSettings = new GameSettings
            {
                CountOfColumns = 10,
                CountOfRows = 5,
                LastNumber = 50,
                FirstNumber = 1,
                DelayInSeconds = 2,
                DisableButtonWhileGeneration = true
            };
            InitializeComponent();
            _generatedNumbers = new List<int>(CountOfGameNumbers);
            GameSettings.SettingsChanged += GameSettingsOnSettingsChanged;
        }

        private void GameSettingsOnSettingsChanged(object sender, EventArgs eventArgs)
        {
            ResetButtonClick(null, null);
        }

        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            _generatedNumbers = new List<int>(CountOfGameNumbers);
            CurrentNumber = null;
            _totalNumbersOnGeneration = 0;
            NumbersGrid.Children.Clear();
        }

        private void GenerateNumberButtonClick(object sender, RoutedEventArgs e)
        {
            if (_totalNumbersOnGeneration == CountOfGameNumbers)
            {
                if (MessageBox.Show("Results are full. Do you want to clean it and start again?", "Attantion",
                        MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    ResetButtonClick(null, null);
                else
                    return;
            }
            _totalNumbersOnGeneration++;
            var delay = GameSettings.DelayInSeconds;
            if (GameSettings.DisableButtonWhileGeneration)
                GenerateButton.IsEnabled = false;
            Task.Factory.StartNew(() =>
            {
                if (delay > 0)
                    for (int i = 0; i < delay * 10; i++)
                    {
                        Thread.Sleep(120);
                        Dispatcher.Invoke(() => CurrentNumber = GetUnicalRandomNumber());
                    }
                else
                    Dispatcher.Invoke(() => CurrentNumber = GetUnicalRandomNumber());
            }).ContinueWith(x =>
            {
                Dispatcher.Invoke(() =>
                {
                    GenerateButton.IsEnabled = true;
                    _generatedNumbers.Add(CurrentNumber.Value);
                    NumbersGrid.Children.Add(new Button {Content = CurrentNumber, IsEnabled = false, FontSize = 18});
                });
            });
        }

        private int GetUnicalRandomNumber()
        {
            var number = _random.Next(GameSettings.FirstNumber, GameSettings.LastNumber + 1);
            while (_generatedNumbers.Contains(number))
            {
                number = _random.Next(GameSettings.FirstNumber, GameSettings.LastNumber + 1);
            }
            return number;
        }


        private void SettingsMenuClick(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(GameSettings);
            settingsWindow.ShowDialog();
        }
    }
}