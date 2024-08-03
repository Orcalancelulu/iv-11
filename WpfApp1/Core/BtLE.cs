using System;
using System.Threading;
using System.Windows;
using Windows.Storage.Streams;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Data;


namespace WpfApp1.Core
{
    static class BtLE
    {
        public static BluetoothLEAdvertisementWatcher Watcher 
        { 
            get 
            {
                return watcher;
            } 
        }
        public static BluetoothLEDevice  BluetoothDevice 
        {
            get 
            {
                return bluetoothDevice;
            } 
        }

        public static bool IsConnected
        {
            get
            {
                return isConnected;
            }
        }

        public static GattCharacteristic DataCharacteristic 
        { 
            get 
            {
                return BtLEDataCharacteristic;
            }
        }

        public static GattCharacteristic SettingsCharacteristic
        {
            get
            {
                return BtLESettingsCharacteristic;
            }
        }

        private static BluetoothLEAdvertisementWatcher watcher;
        private static BluetoothLEDevice bluetoothDevice;
        private static GattCharacteristic BtLEDataCharacteristic;
        private static GattCharacteristic BtLESettingsCharacteristic;

        private static bool isConnected = false;

        public static void connect(string localName)
        {
            watcher = new BluetoothLEAdvertisementWatcher();
            watcher.Received += OnAdvertisementReceived;
            watcher.Start();

        }

        public static async Task<bool> waitTillConnected()
        {
            while (true)
            {
                if (isConnected) return true;
                await Task.Delay(1000);
            }
        }

        private static async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {

            string localName = eventArgs.Advertisement.LocalName;
            if (localName == "IV-11_DISPLAY")
            {
                Console.WriteLine(localName + ", " + eventArgs.BluetoothAddress);

                bluetoothDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress);
                watcher.Stop();

                GattDeviceServicesResult result = await bluetoothDevice.GetGattServicesAsync(); //only for it to connect

                isConnected = true;

                Console.WriteLine("Status: " + bluetoothDevice.ConnectionStatus);

                foreach (GattDeviceService service in result.Services)
                {
                    if (service.Uuid == BluetoothUuidHelper.FromShortId(0x181A)) //search for uuid
                    {
                        GattCharacteristicsResult resultChar = await service.GetCharacteristicsAsync();

                        if (resultChar.Status == GattCommunicationStatus.Success)
                        {
                            var characteristics = resultChar.Characteristics;
                            foreach (GattCharacteristic characteristic in characteristics)
                            {
                                if (characteristic.Uuid == BluetoothUuidHelper.FromShortId(0x2A6E)) //only interested in this characteristic
                                {
                                    BtLEDataCharacteristic = characteristic;

                                    string dataStr = "01001110010011100100111001001110";
                                    int nBytes = dataStr.Length / 8;
                                    var bytesAsStrings =
                                        Enumerable.Range(0, nBytes)
                                                  .Select(i => dataStr.Substring(8 * i, 8));


                                    List<int> Pow2 = new List<int> { 128, 64, 32, 16, 8, 4, 2, 1 };

                                    byte[] byteArray = new byte[4];

                                    string[] byteStrings = { "11111111", "11111111", "11111111", "11111111" };


                                    for (int b = 0; b < 4; b++)
                                    {
                                        int byteResult = 0;
                                        string byteString = byteStrings[b];

                                        for (int i = 0; i < byteString.Length; i++)
                                        {
                                            byteResult += (byteString[i] == '0') ? 0 : Pow2[i];
                                        }

                                        byteArray[b] = Convert.ToByte(byteResult);
                                    }


                                    var writer = new DataWriter();

                                    writer.WriteBytes(byteArray);

                                    GattCommunicationStatus resultWrite = await characteristic.WriteValueAsync(writer.DetachBuffer());
                                    if (resultWrite == GattCommunicationStatus.Success)
                                    {
                                        // Successfully wrote to device
                                        Console.WriteLine("wrote to device");

                                    }
                                } else if (characteristic.Uuid == BluetoothUuidHelper.FromShortId(0x2B1E))
                                {
                                    BtLESettingsCharacteristic = characteristic;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
