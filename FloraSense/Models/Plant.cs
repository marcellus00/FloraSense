using System;
using Windows.UI.Xaml;

namespace FloraSense.Data
{
    public class Plant : DependencyObject
    {
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            "Name", typeof(string), typeof(Plant), new PropertyMetadata(default(string)));

        public string Name
        {
            get => (string) GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public static readonly DependencyProperty MoistureRangeProperty = DependencyProperty.Register(
            "MoistureRange", typeof(Tuple<int,int>), typeof(Plant), new PropertyMetadata(default(Tuple<int,int>)));

        public Tuple<int,int> MoistureRange
        {
            get => (Tuple<int,int>) GetValue(MoistureRangeProperty);
            set => SetValue(MoistureRangeProperty, value);
        }

        public static readonly DependencyProperty FertilityRangeProperty = DependencyProperty.Register(
            "FertilityRange", typeof(Tuple<int,int>), typeof(Plant), new PropertyMetadata(default(Tuple<int,int>)));

        public Tuple<int,int> FertilityRange
        {
            get => (Tuple<int,int>) GetValue(FertilityRangeProperty);
            set => SetValue(FertilityRangeProperty, value);
        }

        public static readonly DependencyProperty BrightnessRangeProperty = DependencyProperty.Register(
            "BrightnessRange", typeof(Tuple<int,int>), typeof(Plant), new PropertyMetadata(default(Tuple<int,int>)));

        public Tuple<int,int> BrightnessRange
        {
            get => (Tuple<int,int>) GetValue(BrightnessRangeProperty);
            set => SetValue(BrightnessRangeProperty, value);
        }

        public static readonly DependencyProperty TemperatureRangeProperty = DependencyProperty.Register(
            "TemperatureRange", typeof(Tuple<float,float>), typeof(Plant), new PropertyMetadata(default(Tuple<float,float>)));

        public Tuple<float,float> TemperatureRange
        {
            get => (Tuple<float,float>) GetValue(TemperatureRangeProperty);
            set => SetValue(TemperatureRangeProperty, value);
        }
    }
}
