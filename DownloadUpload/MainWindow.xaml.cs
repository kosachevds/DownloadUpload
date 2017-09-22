using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using System.Net;
using System.IO;

namespace DownloadUpload
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int MinPort = 1024;
        private const int MaxPort = 65535;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void tabs_TabSelected(object sender, SelectionChangedEventArgs e)
        {
            if (tiDownload.IsSelected)
                this.Title = tiDownload.Header.ToString();
            else if (tiUpload.IsSelected)
                this.Title = tiUpload.Header.ToString();
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Multiselect = false
            };
            if (fileDialog.ShowDialog() == true)
                tbUploadFilePath.Text = fileDialog.FileName;
        }

        private void btnFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new WinForms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == WinForms.DialogResult.OK)
                tbDownloadFolder.Text = folderDialog.SelectedPath;
        }

        private bool IsValidPort(string input)
        {
            int port;
            if (!Int32.TryParse(input, out port))
                return false;
            return port >= MinPort && port <= MaxPort;
        }

        private bool IsValidAddressPort(string ip, string port)
        {
            IPAddress address;
            if (!IPAddress.TryParse(ip, out address))
            {
                ShowError("Invalid ip-address");
                return false;
            }
            else if (!IsValidPort(port))
            {
                ShowError($"Invalid port. From {MinPort} to {MaxPort}");
                return false;
            }
            return true;
        }

        private bool FileIsExists(string path)
        {
            if (File.Exists(tbUploadFilePath.Text)) 
                return true;
            ShowError("Invalid file path");
            return false;
        }

        private bool FolderIsExists(string path)
        {
            if (Directory.Exists(tbDownloadFolder.Text))
                return true;
            ShowError("Invalid folder path");
            return false;
        }

        private void StartUploading(object sender, RoutedEventArgs e)
        {
            var filePath = tbUploadFilePath.Text;
            if (!FileIsExists(filePath))
                return;
            var ip = tbUploadIp.Text;
            if (!IsValidAddressPort(ip, tbUploadPort.Text))
                return;
            var port = Int32.Parse(tbUploadPort.Text);
            try
            {
                ProgressWindow.ShowUploading(ip, port, filePath);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void StartDownloading(object sender, RoutedEventArgs e)
        {
            var folderPath = tbDownloadFolder.Text;
            if (!FolderIsExists(folderPath))
                return;
            var ip = tbDownloadIp.Text;
            if (!IsValidAddressPort(ip, tbDownloadPort.Text))
                return;
            var port = Int32.Parse(tbDownloadPort.Text);
            try
            {
                ProgressWindow.ShowDownloading(ip, port, folderPath);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private static void ShowError(string content)
        {
            MessageBox.Show(content, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
