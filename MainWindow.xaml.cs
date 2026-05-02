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
using System.Net.Sockets;
using System.Net;



namespace ServerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static IPAddress ipAddr = IPAddress.Parse("127.0.0.1");

        private static IPEndPoint remoteEP = new IPEndPoint(ipAddr, 11000);


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Response.Clear();
            Socket socket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(remoteEP);


            Response.Text=($"Підключено до {socket.RemoteEndPoint}");

            string message = "Привіт сервере!";
            byte[] msg = Encoding.UTF8.GetBytes(message);
            int byteSent = socket.Send(msg);

            byte[] bytes = new byte[1024];
            int byteRec = socket.Receive(bytes);
            Response.Text += ($"\nВідповідь сервера: {Encoding.UTF8.GetString(bytes, 0, byteRec)}");

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            Response.Clear();
            Socket socket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(remoteEP);

            byte[] msg = Encoding.UTF8.GetBytes(Input.Text);
            int byteSent = socket.Send(msg);

            byte[] bytes = new byte[1024];
            int byteRec = socket.Receive(bytes);
            Response2.Text = ($"Відповідь сервера: {Encoding.UTF8.GetString(bytes, 0, byteRec)}");

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}