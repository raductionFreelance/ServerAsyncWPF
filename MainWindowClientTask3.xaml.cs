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
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatLibrary
{
    public enum ChatMode { Human, Computer}

    public class Messenger
    { 
        private TcpClient _Client;
        private NetworkStream _Stream;
        private readonly string[] _botPhrases = {
            "Привіт! Як справи?",
            "Цікава думка, розкажи більше.",
            "Я просто бот, але я тебе слухаю.",
            "Ого, ніколи про це не думав!",
            "Сьогодні чудовий день для коду!"
        };

        public event Action<string> MessageReceived;
        public event Action<string> StatusChanged;
        public event Action ConnectionClosed;

        public async Task StartAsServer(int port)
        {
            TcpListener listener = new TcpListener(System.Net.IPAddress.Any, port);
            listener.Start();
            _Client = await listener.AcceptTcpClientAsync();
            _Stream = _Client.GetStream();
            _ = ListenAsync();
        }

        public async Task ConnectAsClient(string ip, int port)
        {
            _Client = new TcpClient();
            await _Client.ConnectAsync(ip, port);
            _Stream = _Client.GetStream();
            StatusChanged?.Invoke("Підключено до сервера!");
            _ = ListenAsync();
        }

        public async Task SendMessage(string message) {
            if (_Stream == null) return;

            byte[] data = Encoding.UTF8.GetBytes(message);
            await _Stream.WriteAsync(data, 0, data.Length);

            if (message.Trim().Equals("Bye", StringComparison.OrdinalIgnoreCase)) Close();
        }

        private async Task ListenAsync()
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int bytesRead = await _Stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; 
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    MessageReceived?.Invoke(message);

                    if (message.Trim().Equals("Bye", StringComparison.OrdinalIgnoreCase))
                    {
                        Close();
                        break;
                    }
                }
            }
            catch (Exception){}
            finally
            {
                ConnectionClosed?.Invoke();
            }
        }

        public string GetBotResponse()
        {
            Random rand = new Random();
            return _botPhrases[rand.Next(_botPhrases.Length)];
        }

        public void Close()
        {
            _Stream?.Close();
            _Client?.Close();
        }
    }
}

namespace ChatClientWPF
{
    public partial class MainWindow : Window
    {
        private Messenger _messenger = new Messenger();
        private bool IsBotMode => ChatModeComboBox.SelectedIndex == 1;
        
        public MainWindow()
        {
            InitializeComponent();
            
            _messenger.MessageReceived += HandleIncomingMessage;
            _messenger.StatusChanged += (status) => Dispatcher.Invoke(() => Title = status);
            _messenger.ConnectionClosed += () => Dispatcher.Invoke(() => ChatDisplay.Text += "[Зв'язок розірвано]\n");
        }

        public async void connectToServer(object sender, RoutedEventArgs e)
        {
            int port = int.Parse(PortInput.Text);
            await _messenger.ConnectAsClient(IpInput.Text, port);
        }
        
        private void HandleIncomingMessage(string message)
        {
            Dispatcher.Invoke(async () =>
            {
                ChatDisplay.Text += $"Сервер: {message}\n";
                if (IsBotMode && !message.Equals("Bye", StringComparison.OrdinalIgnoreCase))
                {
                    string GetbotResponse = _messenger.GetRandomPhrase();
                    await _messenger.SendMessage(GetbotResponse);
                    ChatDisplay.Text += $"Бот: {GetbotResponse}\n";
                    ChatDisplay.ScrollToEnd();
                }
            });
        }

        public async void sendMessage(object sender, RoutedEventArgs e)
        {
            string message = MessageInput.Text;

            await _messenger.SendMessage(message);
            ChatDisplay.Text += $"Ви: {message}";

            MessageInput.Clear();
            ChatDisplay.ScrollToEnd();
        }
    }
}
