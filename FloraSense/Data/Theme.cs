using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace FloraSense
{
    public class Theme : DependencyObject
    {
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            "Name", typeof(string), typeof(Theme), new PropertyMetadata(default(string)));

        public string Name
        {
            get => (string) GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            "Brush", typeof(Brush), typeof(Theme), new PropertyMetadata(default(Brush)));

        public Brush Brush
        {
            get => (Brush) GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }

        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register(
            "TextColor", typeof(Brush), typeof(Theme), new PropertyMetadata(default(Brush)));

        public Brush TextColor
        {
            get => (Brush) GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }
    }
}
