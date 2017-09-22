using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace DownloadUpload
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private BackgroundWorker _uploader;
        private BackgroundWorker _downloader;
        private string _ip;
        private int _port;
        private string _path;
        
        private ProgressWindow(string ip, int port, string path)
        {
            InitializeComponent();
            _ip = ip;
            _port = port;
            _path = path;
            _downloader = this.FindResource("downloader") as BackgroundWorker;
            _downloader.DoWork += Download;
            _downloader.ProgressChanged += ShowProgress;
            _downloader.RunWorkerCompleted += CloseWindow;
            _downloader.WorkerReportsProgress = true;
            _uploader = this.FindResource("uploader") as BackgroundWorker;
            _uploader.DoWork += Upload;
            _uploader.ProgressChanged += ShowProgress;
            _uploader.RunWorkerCompleted += CloseWindow;
            _uploader.WorkerReportsProgress = true;
        }

        private void ShowProgress(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage;
            var span = (TimeSpan)e.UserState;
            lblTime.Content = span.ToString();
        }

        private void CloseWindow(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                this.Close();
            }
            else
            {
                tbxError.Visibility = Visibility.Visible;
                tbxError.Text = e.Error.Message;
            }
        }

        public static void ShowDownloading(string ip, int port, string folder)
        {
            var window = new ProgressWindow(ip, port, folder);
            window.Show();
            window._downloader.RunWorkerAsync();
        }

        private void Download(object sender, DoWorkEventArgs e)
        {
            using (var client = new TcpClient())
            {
                client.Connect(_ip, _port);
                using (var stream = client.GetStream())
                using (var reader = new BinaryReader(stream))
                {
                    var filename = reader.ReadString();
                    var fileSize = reader.ReadInt64();
                    var chunkSize = (int)Math.Min(fileSize / 100, Int32.MaxValue);

                    using (var file = File.Open(Path.Combine(_path, filename), FileMode.Create))
                    {
                        byte[] chunk = new byte[chunkSize];
                        long size = 0L;
                        int count;
                        var start = DateTime.Now;
                        while ((count = stream.Read(chunk, 0, chunkSize)) != 0)
                        {
                            file.Write(chunk, 0, count);
                            size += count;
                            var span = DateTime.Now - start;
                            _downloader.ReportProgress((int)(size * 100 / fileSize), span);
                        }
                    }
                }
            }
        }



        public static void ShowUploading(string ip, int port, string filePath)
        {
            var window = new ProgressWindow(ip, port, filePath);
            window.Show();
            window._uploader.RunWorkerAsync();
        }

        private void Upload(object sender, DoWorkEventArgs e)
        {
            var filename = Path.GetFileName(_path);
            var fileSize = new FileInfo(_path).Length;
            var listener = new TcpListener(IPAddress.Parse(_ip), _port);
            listener.Start();
            using (var client = listener.AcceptTcpClient())
            using (var stream = client.GetStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(filename);
                writer.Write(fileSize);
                var chunkSize = (int)Math.Min(fileSize / 100, Int32.MaxValue);
                using (var file = File.Open(_path, FileMode.Open))
                {
                    byte[] chunk = new byte[chunkSize];
                    long size = 0L;
                    int count;
                    var start = DateTime.Now;
                    while ((count = file.Read(chunk, 0, chunkSize)) != 0)
                    {
                        stream.Write(chunk, 0, count);
                        size += count;
                        var span = DateTime.Now - start;
                        _uploader.ReportProgress((int)(size * 100 / fileSize), span);
                    }
                }
            }
            listener.Stop();
        }

    }
}
