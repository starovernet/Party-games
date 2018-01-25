using System.Windows;

namespace PictureRandomiser.Image
{
    public class MyImage : System.Windows.Controls.Image
    {
        public static readonly RoutedEvent SourceChangedEvent = EventManager.RegisterRoutedEvent(
            "SourceChanged", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MyImage));

        static MyImage()
        {
            SourceProperty.OverrideMetadata(typeof(MyImage), new FrameworkPropertyMetadata(SourcePropertyChanged));
        }

        public event RoutedEventHandler SourceChanged
        {
            add { AddHandler(SourceChangedEvent, value); }
            remove { RemoveHandler(SourceChangedEvent, value); }
        }

        private static void SourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.Controls.Image image = obj as System.Windows.Controls.Image;
            image?.RaiseEvent(new RoutedEventArgs(SourceChangedEvent));
        }
    }
}