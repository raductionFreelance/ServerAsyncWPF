using System.Net;
using System.Net.Sockets;
using System.Text;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;


IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11000);

Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

listener.Bind(localEndPoint);
listener.Listen(10);
Console.WriteLine("Сервер запущено!!!");

while (true)
{
    bool shouldExist = false;
    string response = "";
    try
    {

        Socket handler = listener.Accept();
        Console.WriteLine("Клієнт під'єднався!");

        byte[] bytes = new byte[1024];
        int bytesRec = handler.Receive(bytes);
        string data = Encoding.UTF8.GetString(bytes, 0, bytesRec);

    
    
        Console.WriteLine($"О {TimeOnly.FromDateTime(DateTime.Now).ToString("HH:mm")} було отримано: {data}");
        if (data == "Привіт сервере!")
        {
            response = "Повідомлення отримано";
        }else if (data == "1")
        {
            response = DateTime.Now.ToString("dd.MM.yyyy");
        }
        else if (data == "2") {
            response = TimeOnly.FromDateTime(DateTime.Now).ToString("HH:mm");
        }else if (data == "3")
        {
            response = "Сервер вимикається...";
            shouldExist = true;
        }
        
        
        byte[] msg = Encoding.UTF8.GetBytes(response);
        handler.Send(msg);

        if (shouldExist)
        {
            Console.WriteLine("Сирвер вимкнено!");
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            break;
        }

        

        Console.WriteLine("Запит виведено");
    }

    catch (Exception e)
    {
        Console.WriteLine(e.ToString());
    }
}
