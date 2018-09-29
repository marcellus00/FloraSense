using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace FloraSense
{
    public sealed partial class MeasurementView : UserControl
    {
        public MeasurementView()
        {
            this.InitializeComponent();
        }

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string Problems
        {
            get => (string)GetValue(ProblemsProperty);
            set => SetValue(ProblemsProperty, value);
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Unit
        {
            get => (string)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        public ValueStatus Status
        {
            get => (ValueStatus)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(MeasurementView), PropertyMetadata.Create("🐇"));
        public static readonly DependencyProperty ProblemsProperty =
            DependencyProperty.Register("Problems", typeof(string), typeof(MeasurementView), PropertyMetadata.Create(string.Empty));
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(MeasurementView), PropertyMetadata.Create("3000"));
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(MeasurementView), PropertyMetadata.Create(" µS/cm"));
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(ValueStatus), typeof(MeasurementView), PropertyMetadata.Create(default(ValueStatus)));

        public enum ValueStatus
        {
            Normal,
            TooLow,
            TooHigh
        }
    }
}
