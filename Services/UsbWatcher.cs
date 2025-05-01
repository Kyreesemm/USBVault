using System.Management;
using System.Threading;

namespace USBVault.Services
{
    public static class UsbWatcher
    {
        private static bool _isRunning;
        private static Thread _watcherThread;
        private static string _expectedDrive;

        public static string WaitForUsbKey()
        {
            Console.WriteLine("\n[USB-V] Ожидание USB-ключа...");

            // Проверяем уже подключенные диски
            var existingDrives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Removable)
                .Select(d => d.Name[0].ToString())
                .ToList();

            foreach (var drive in existingDrives)
            {
                if (UsbKeyManager.TryReadKey(drive) != null)
                    return drive;
            }

            // Ожидаем новое подключение
            var watcher = new ManagementEventWatcher();
            var query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            watcher.Query = query;

            string foundDrive = null;
            var resetEvent = new System.Threading.AutoResetEvent(false);

            watcher.EventArrived += (sender, e) =>
            {
                var drive = e.NewEvent.Properties["DriveName"].Value.ToString()[0].ToString();
                if (UsbKeyManager.TryReadKey(drive) != null)
                {
                    foundDrive = drive;
                    resetEvent.Set();
                }
            };

            watcher.Start();
            resetEvent.WaitOne();
            watcher.Stop();

            return foundDrive;
        }

        public static void StartWatching(string driveLetter)
        {
            _expectedDrive = driveLetter;
            _isRunning = true;
            _watcherThread = new Thread(WatchUsb);
            _watcherThread.IsBackground = true;
            _watcherThread.Start();
        }

        public static void StopWatching()
        {
            _isRunning = false;
            _watcherThread?.Join();
        }

        private static void WatchUsb()
        {
            var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
            var watcher = new ManagementEventWatcher(query);

            watcher.EventArrived += (sender, e) =>
            {
                if (!File.Exists($"{_expectedDrive}:\\vault.key"))
                {
                    Console.WriteLine("\n\nUSB-ключ извлечён! Программа будет закрыта.");
                    Environment.Exit(0);
                }
            };

            watcher.Start();
            while (_isRunning) Thread.Sleep(1000);
            watcher.Stop();
        }
    }
}