﻿using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace FloraSense
{
    public sealed partial class MeasurementView : UserControl
    {
        public MeasurementView()
        {
            InitializeComponent();
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
        
        public Brush ForegroundNormal
        {
            get => (Brush)GetValue(ForegroundNormalProperty);
            set => SetValue(ForegroundNormalProperty, value);
        }

        public Brush ForegroundWarning
        {
            get => (Brush)GetValue(ForegroundWarningProperty);
            set => SetValue(ForegroundWarningProperty, value);
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
        public static readonly DependencyProperty ForegroundNormalProperty =
            DependencyProperty.Register("ForegroundNormal", typeof(Brush), typeof(MeasurementView), PropertyMetadata.Create(new SolidColorBrush(Colors.White)));
        public static readonly DependencyProperty ForegroundWarningProperty =
            DependencyProperty.Register("ForegroundWarning", typeof(Brush), typeof(MeasurementView), PropertyMetadata.Create(new SolidColorBrush(Colors.Red)));

        public enum ValueStatus
        {
            Normal,
            TooLow,
            TooHigh
        }

        private void MeasurementView_OnLayoutUpdated(object sender, object e)
        {
            Foreground = string.IsNullOrEmpty(Problems) ? ForegroundNormal : ForegroundWarning;
        }
    }
}
