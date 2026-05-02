using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _isAuthenticated = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Authorization(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_client == null || !_client.Connected) {
                    _client = new TcpClient();
                    await _client.ConnectAsync("127.0.0.1", 11000);
                    _stream = _client.GetStream();
                    _ = ReceiveMessagesAsync();
                }

                string credentials = $"{Password.Text}";
                byte[] data = Encoding.UTF8.GetBytes(credentials);
                await _stream.WriteAsync(data, 0, data.Length);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if(_stream != null && _client.Connected)
            {
                try {
                    byte[] data = Encoding.UTF8.GetBytes("1");
                    await _stream.WriteAsync(data, 0, data.Length);
                } catch { }
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            _stream = _client.GetStream();
            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Dispatcher.Invoke(() => Response.Text += $"{message}\n");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                    break;
                }
            }

            CloseConnection();
        }

        private void CloseConnection()
        {
            _stream?.Close();
            _client?.Close();
            _stream = null;
            _client = null;
        }
    }
}