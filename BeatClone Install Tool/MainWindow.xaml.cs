using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace BeatClone_Install_Tool
{
    public partial class MainWindow : Window
    {
        private readonly string adbPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "adb.exe");

        public MainWindow()
        {
            InitializeComponent();
            CheckDeviceOnStartup();
        }

        // ==========================================
        // LOGGING
        // ==========================================
        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText(message + Environment.NewLine);
                OutputTextBox.ScrollToEnd();
            });
        }

        private void ResetProgress()
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = 0;
                ProgressBar.Maximum = 100;
            });
        }

        private void SetBusyState(bool busy)
        {
            Dispatcher.Invoke(() =>
            {
                InstallApkButton.IsEnabled = !busy;
                DeployAssetsButton.IsEnabled = !busy;
                CopyUserFileButton.IsEnabled = !busy;
                RebootButton.IsEnabled = !busy;
            });
        }

        // ==========================================
        // GENERIC ADB COMMAND
        // ==========================================
        private async Task<string> RunAdb(string arguments)
        {
            return await Task.Run(() =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    return output + error;
                }
            });
        }

        // ==========================================
        // DEVICE CHECK
        // ==========================================
        private async void CheckDeviceOnStartup()
        {
            await CheckDevice();
        }

        private async void BtnCheckDevice_Click(object sender, RoutedEventArgs e)
        {
            await CheckDevice();
        }

        private async Task CheckDevice()
        {
            Log("Checking device connection...");

            string result = await RunAdb("devices");

            bool deviceConnected = result
                .Split('\n')
                .Any(line => line.Trim().EndsWith("\tdevice"));

            Dispatcher.Invoke(() =>
            {
                if (deviceConnected)
                {
                    DeviceStatusIndicator.Fill = Brushes.Green;
                    DeviceStatusText.Text = "Device Connected";
                    Log("Device detected.");
                }
                else
                {
                    DeviceStatusIndicator.Fill = Brushes.Red;
                    DeviceStatusText.Text = "No Device";
                    Log("No device detected.");
                }
            });
        }

        // ==========================================
        // INSTALL APK
        // ==========================================
        private async void InstallApkButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "APK files (*.apk)|*.apk";

            if (dialog.ShowDialog() != true)
                return;

            ResetProgress();
            SetBusyState(true);

            string apkPath = dialog.FileName;

            Log("Installing APK...");

            await RunAdb($"install -r \"{apkPath}\"");

            Log("APK install finished.");
            SetBusyState(false);
        }

        // ==========================================
        // DEPLOY ASSETS (NEW APK METHOD)
        // ==========================================
        private async void DeployAssetsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "ZIP files (*.zip)|*.zip";

            if (dialog.ShowDialog() != true)
                return;

            ResetProgress();
            SetBusyState(true);

            string zipPath = dialog.FileName;
            string extractPath = Path.Combine(Path.GetTempPath(), "beatstar_assets");

            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            Log("Extracting ZIP...");
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            Log("Extraction complete.");

            string finalPath = "/sdcard/beatstar/files";

            Log("Creating folder...");
            await RunAdb($"shell rm -r \"{finalPath}\"");
            await RunAdb($"shell mkdir -p \"{finalPath}\"");

            var folders = Directory.GetDirectories(extractPath);
            int total = folders.Length;
            int current = 0;

            Dispatcher.Invoke(() =>
            {
                ProgressBar.Maximum = total;
                ProgressBar.Value = 0;
            });

            Log("Pushing folders to location...");

            foreach (var folder in folders)
            {
                string folderName = Path.GetFileName(folder);
                Log("Pushing: " + folderName);

                await RunAdb($"push \"{folder}\" \"{finalPath}\"");

                current++;
                Dispatcher.Invoke(() =>
                {
                    ProgressBar.Value = current;
                });
            }
            Log("Deployment complete. Please open the game and allow your assets to sync fully!");
            SetBusyState(false);
        }

        // ==========================================
        // COPY USER FILE
        // ==========================================
        private async void CopyUserFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            if (dialog.ShowDialog() != true)
                return;

            ResetProgress();
            SetBusyState(true);

            string filePath = dialog.FileName;
            string devicePath = "/sdcard/beatstar";

            await RunAdb($"shell mkdir -p \"{devicePath}\"");
            await RunAdb($"push \"{filePath}\" \"{devicePath}\"");

            Log("User file copied successfully.");
            SetBusyState(false);
        }

        // ==========================================
        // REBOOT DEVICE
        // ==========================================
        private async void RebootButton_Click(object sender, RoutedEventArgs e)
        {
            Log("Rebooting device...");
            await RunAdb("reboot");
            Log("Reboot command sent.");
        }

        // ==========================================
        // CLEAR LOG
        // ==========================================
        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Clear();
        }

        // ==========================================
        // HYPERLINK HANDLER
        // ==========================================
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
        }
    }
}