using System.Collections.Generic;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace MiFlora
{
    public class SensorData
    {
        public string DeviceId;

        public int Moisture;
        public ushort Conductivity; // Fertility
        public uint Brightness;
        public float Temperature;
        public int Battery;

        public string Name;
        public string Verison;

        public string SerialNumber;
        public ErrorType Error;
        public string ErrorDetails;

        public bool IsBad => Moisture == 0 && Conductivity == 0 && Brightness == 0 && Battery == 0 && Temperature == 0f;

        public enum ErrorType
        {
            None,
            BluetoothOff,
            CannotConnect,
            CommError,
            InvalidOperation
        }
    }
}
