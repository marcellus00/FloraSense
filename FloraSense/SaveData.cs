using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
// ReSharper disable InvertIf

namespace FloraSense
{
    public class SaveData
    {
        public static void Clear()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            foreach (var container in localSettings.Containers)
                localSettings.DeleteContainer(container.Key);
        }

        public static void Save<T>(T save)
        {
            var serializer = new XmlSerializer(typeof(T));

            string data = null;
            using (var writer = new StringWriter())
            {
                try
                {
                    serializer.Serialize(writer, save);
                    data = writer.ToString();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            if(!string.IsNullOrEmpty(data))
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values[nameof(T)] = data;
            }
        }

        public static T Load<T>()
        {
            var serializer = new XmlSerializer(typeof(T));

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var value = (string) localSettings.Values[nameof(T)];

            var data = default(T);

            if (!string.IsNullOrEmpty(value))
                using (var reader = new StringReader(value))
                {
                    try
                    {
                        data = (T) serializer.Deserialize(reader);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }

            return data;
        }
    }
}
