using System;
using PropertyChanged;

namespace PictureRandomiser
{
    [ImplementPropertyChanged]
    public class GameSettings
    {
        private int _firstNumber;
        private int _lastNumber;
        public event EventHandler<EventArgs> SettingsChanged;

        public int CountOfRows { get; set; }

        public int CountOfColumns { get; set; }

        public int DelayInSeconds { get; set; }
        public bool DisableButtonWhileGeneration { get; set; }
        public bool NonStop { get; set; }
        public int CountOfPicturesDisplay { get; set; }

        public int FirstNumber
        {
            get { return _firstNumber; }
            set
            {
                if (value >= LastNumber) return;
                _firstNumber = value;
                OnSettingsChanged();
            }
        }

        public int LastNumber
        {
            get { return _lastNumber; }
            set
            {
                if (value <= FirstNumber) return;
                _lastNumber = value;
                OnSettingsChanged();
            }
        }

        protected virtual void OnSettingsChanged()
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}