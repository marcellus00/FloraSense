using System;

namespace FloraSense.Data
{
    [Serializable]
    public class MinMaxInt
    {
        public MinMaxInt() { }

        public MinMaxInt(int min, int max)
        {
            Min = min;
            Max = max;
        }
        
        public int Min { get; set; }
        public int Max { get; set; }
    }

    [Serializable]
    public class MinMaxFloat
    {
        public MinMaxFloat() { }

        public MinMaxFloat(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Min { get; set; }
        public float Max { get; set; }
    }
}