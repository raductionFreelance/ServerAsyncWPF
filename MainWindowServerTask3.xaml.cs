using ChatLibrary;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
    public enum ChatMode { Human, Computer }

    public class Messenger
    {
        private TcpListener _listener;
        private TcpClient _client;
        private NetworkStream _stream;

        private readonly string[] _botPhrases = {
            "Привіт! Чим можу допомогти?",
            "Цікаво, розкажи про це детальніше.",
            "Я просто сервер, але я тебе розумію.",
            "Напевно, ти правий!",
            "Сьогодні чудовий день для чату!"
        };

        public event Action<string> MessageReceived; 
        public event Action<string> StatusChanged;   

        public async Task StartServer(int port)
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                StatusChanged?.Invoke($"Сервер запущено на порту {port}. Очікування...");

                _client = await _listener.AcceptTcpClientAsync();
                _stream = _client.GetStream();
                StatusChanged?.Invoke("Клієнт підключився!");

                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"Помилка запуску: {ex.Message}");
            }
        }

        public async Task SendMessage(string message)
        {
            if (_stream == null || !_client.Connected) return;

            byte[] data = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);

            if (message.Trim().Equals("Bye", StringComparison.OrdinalIgnoreCase))
                Disconnect();
        }

        private async Task ReceiveMessagesAsync()
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (_client != null && _client.Connected)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    MessageReceived?.Invoke(message);

                    if (message.Trim().Equals("Bye", StringComparison.OrdinalIgnoreCase))
                    {
                        Disconnect();
                        break;
                    }
                }
            }
            catch { }
            finally { Disconnect(); }
        }

        public string GetRandomPhrase()
        {
            return _botPhrases[new Random().Next(_botPhrases.Length)];
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
            _listener?.Stop();
            StatusChanged?.Invoke("З'єднання закрито.");
        }
    }
}


namespace ChatServerWPF
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
        }

        private async void StartServer(object sender, RoutedEventArgs e)
        {
            int port = int.Parse(PortInput.Text);
            await _messenger.StartServer(port);
        }
        private void HandleIncomingMessage(string message)
        {
            Dispatcher.Invoke(async () =>
            {
                ChatDisplay.Text += $"Клієнт: {message}\n";
                if (IsBotMode && !message.Equals("Bye", StringComparison.OrdinalIgnoreCase))
                {
                    string botResponse = _messenger.GetRandomPhrase();
                    await _messenger.SendMessage(botResponse);
                    ChatDisplay.Text += $"Бот: {botResponse}\n";
                }
            });
        }

        private async void SendMessage(object sender, RoutedEventArgs e)
        {
            string message = MessageInput.Text;
            await _messenger.SendMessage(message);
            ChatDisplay.Text += $"Сервер: {message}\n";
            MessageInput.Clear();
        }
    }
}
