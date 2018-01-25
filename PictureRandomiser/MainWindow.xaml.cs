using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PictureRandomiser.Image;

namespace PictureRandomiser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static readonly DependencyProperty GameSettingsProperty = DependencyProperty.Register(
            "GameSettings", typeof(GameSettings), typeof(MainWindow), new PropertyMetadata(default(GameSettings)));

        public GameSettings GameSettings
        {
            get { return (GameSettings) GetValue(GameSettingsProperty); }
            set { SetValue(GameSettingsProperty, value); }
        }


        public static readonly DependencyProperty CurrentImageSourceProperty = DependencyProperty.Register(
            "CurrentImageSource", typeof(ImageSource), typeof(MainWindow), new PropertyMetadata(default(ImageSource)));

        public BitmapImage CurrentImageSource
        {
            get { return (BitmapImage) GetValue(CurrentImageSourceProperty); }
            set { SetValue(CurrentImageSourceProperty, value); }
        }


        private readonly Random _random = new Random();
        private volatile int _totalNumbersOnGeneration;
        private List<int> _generatedIds;
        public int? CurrentId { get; set; }
        public int CountOfGameNumbers { get; set; }
        private bool _forceStop;

        public static readonly DependencyProperty PicturesProperty = DependencyProperty.Register(
            "Pictures", typeof(IList<Picture>), typeof(MainWindow), new PropertyMetadata(default(IList<Picture>)));

        private readonly object _syncRoot = new object();

        public IList<Picture> Pictures
        {
            get { return (IList<Picture>) GetValue(PicturesProperty); }
            set { SetValue(PicturesProperty, value); }
        }

        public MainWindow()
        {
            GameSettings = new GameSettings
            {
                CountOfColumns = 5,
                CountOfRows = 2,
                DelayInSeconds = 10,
                DisableButtonWhileGeneration = true,
                NonStop = false,
                CountOfPicturesDisplay = 10
            };
            InitializeComponent();
        }

        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            _generatedIds = new List<int>(Pictures.Count);
            CurrentImageSource = null;
            CurrentId = null;
            lock (_syncRoot)
            {
                _forceStop = true;
            }

            _totalNumbersOnGeneration = 0;
            PicturesGrid.Children.Clear();
            GenerateButton.IsEnabled = true;
        }

        private void GenerateNumberButtonClick(object sender, RoutedEventArgs e)
        {
            lock (_syncRoot)
            {
                _forceStop = false;
            }

            if (_totalNumbersOnGeneration == GameSettings.CountOfPicturesDisplay)
            {
                if (MessageBox.Show("Results are full. Do you want to clean it and start again?", "Attantion",
                        MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    ResetButtonClick(null, null);
                    _forceStop = false;
                }
                else
                    return;
            }

            _totalNumbersOnGeneration++;
            var delayInSeconds = GameSettings.DelayInSeconds;
            if (GameSettings.DisableButtonWhileGeneration)
                GenerateButton.IsEnabled = false;
            var picturesCount = Pictures.Count;
            var countOfSteps = GetCountOfSteps(delayInSeconds * 1000);
            Task.Factory.StartNew(() =>
            {
                if (delayInSeconds > 0)
                    for (int i = 0; i < countOfSteps; i++)
                    {
                        if (_forceStop)
                            return;
                        CurrentId = GetUnicalRandomNumber(picturesCount);
                        Dispatcher.Invoke(
                            () => CurrentImageSource = _forceStop ? null : Pictures[CurrentId.Value].Image);
                        var delay = GetDelayMilliseconds((double) i / countOfSteps);
                        Thread.Sleep(delay);
                    }
                else
                    Dispatcher.Invoke(() => CurrentId = GetUnicalRandomNumber(picturesCount));
            }).ContinueWith(x =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (_forceStop)
                        return;
                    if (CurrentId != null)
                    {
                        _generatedIds.Add(CurrentId.Value);
                        PicturesGrid.Children.Add(
                            new System.Windows.Controls.Image {Source = Pictures[CurrentId.Value].Image});
                    }

                    if (GameSettings.NonStop && !_forceStop)
                        Dispatcher.Invoke(() => GenerateNumberButtonClick(null, null));
                    else
                        GenerateButton.IsEnabled = true;
                });
            });
        }

        private int GetCountOfSteps(int delayInMillisec, double restPercent = 1)
        {
            if (delayInMillisec < 0)
                return 0;
            int countOfSteps = 0;
            while (restPercent > 0)
            {
                restPercent = restPercent - (double) GetDelayMilliseconds(1 - restPercent) / delayInMillisec;
                countOfSteps++;
            }

            return countOfSteps;
        }

        private int GetDelayMilliseconds(double percent)
        {
            if (percent < .7) return 120;
            if (percent < .8) return 150;
            if (percent < .9) return 200;
            if (percent < .95) return 300;
            return 450;
        }

        private int GetUnicalRandomNumber(int count)
        {
            var number = _random.Next(0, count);
            while (_generatedIds.Contains(number))
                number = _random.Next(0, count);
            return number;
        }

        private void SettingsMenuClick(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(GameSettings);
            settingsWindow.ShowDialog();
        }

        private void GetPhotosButtonClick(object sender, RoutedEventArgs e)
        {
            var selectDialog = new OpenFileDialog();
            selectDialog.Multiselect = true;
            selectDialog.ShowDialog();
            var files =
                selectDialog.FileNames.Where(
                    x =>
                        x.Substring(x.Length - 3, 3).ToLower() == "jpg" ||
                        x.Substring(x.Length - 3, 3).ToLower() == "png").ToArray();
            if (!files.Any())
                return;
            Pictures =
                files.Select(
                    (x, i) =>
                    {
                        ImageHelper.RotateImageByExifOrientationData(x, x, ImageFormat.Jpeg);
                        var pic = new Picture
                        {
                            Id = i
                        };
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = new Uri(x);
                        image.DecodePixelWidth = 500;
                        image.EndInit();
                        pic.Image = image;
                        return pic;
                    }).ToArray();
            GenerateButton.IsEnabled = true;
            ResetButton.IsEnabled = true;
            CountOfGameNumbers = Pictures.Count;
            GameSettings.CountOfPicturesDisplay = CountOfGameNumbers > GameSettings.CountOfPicturesDisplay
                ? GameSettings.CountOfPicturesDisplay
                : CountOfGameNumbers;
            _generatedIds = new List<int>(Pictures.Count);
        }
    }
}