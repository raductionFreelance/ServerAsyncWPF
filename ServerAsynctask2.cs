using System;
using System.Collections.Generic; 
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;    

class MultiClientServer
{
    private static readonly Random rnd = new Random();

    static async Task Main(string[] args)
    {
        int port = 11000;
        TcpListener listener = new TcpListener(IPAddress.Any, port);

        listener.Start();
        Console.WriteLine($"Сервер працює на порті {port}");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine($"Новий клієнт {client.Client.RemoteEndPoint} підключено о {DateTime.Now:HH:mm}");
            _ = HandleClientAsync(client);
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        using (NetworkStream stream = client.GetStream())
        {
            int generatedQuotesCount = 0;
            const int maxQuotes = 2;
            const string ValidCredentials = "admin:1234";
            bool isAuthenticated = false;

            byte[] buffer = new byte[1024];

            try
            {
                byte[] welcomeMsg = Encoding.UTF8.GetBytes("Введіть логін і пароль (логін:пароль): ");
                await stream.WriteAsync(welcomeMsg, 0, welcomeMsg.Length);

                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; 

                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                    if (!isAuthenticated)
                    {
                        if (receivedData == ValidCredentials)
                        {
                            isAuthenticated = true;
                            byte[] successMsg = Encoding.UTF8.GetBytes("Доступ дозволено! Надішліть '1' для цитати.\n");
                            await stream.WriteAsync(successMsg, 0, successMsg.Length);
                            continue;
                        }
                        else
                        {
                            byte[] failMsg = Encoding.UTF8.GetBytes("Невірний логін або пароль. До побачення!\n");
                            await stream.WriteAsync(failMsg, 0, failMsg.Length);
                            break;
                        }
                    }

                    if (int.TryParse(receivedData, out int message))
                    {
                        if (message == 1)
                        {
                            if (generatedQuotesCount >= maxQuotes)
                            {
                                byte[] limitMsg = Encoding.UTF8.GetBytes("\nВи досягли ліміту цитат. З'єднання буде розірвано.\n");
                                await stream.WriteAsync(limitMsg, 0, limitMsg.Length);
                                break;
                            }

                            string randomQuote = GeneratedQuote();
                            byte[] quoteResponce = Encoding.UTF8.GetBytes($"\nЦитата: {randomQuote}\nВведіть '1' для ще однієї: ");
                            await stream.WriteAsync(quoteResponce, 0, quoteResponce.Length);
                            generatedQuotesCount++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        byte[] hintMsg = Encoding.UTF8.GetBytes("Команда не розпізнана. Введіть '1'.\n");
                        await stream.WriteAsync(hintMsg, 0, hintMsg.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка клієнта {client.Client.RemoteEndPoint}: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"Клієнт {client.Client.RemoteEndPoint} відключився.");
            }
        }
    }

    private static string GeneratedQuote()
    {
        List<string> quotes = new List<string>
        {
            "Життя — це те, що з тобою відбувається, поки ти будуєш плани.",
            "Логіка може привести вас від А до Б, а уява — куди завгодно.",
            "Не важливо, як повільно ви йдете, головне — не зупинятися.",
            "Майбутнє належить тим, хто вірить у красу своїх мрій.",
            "Найкращий спосіб передбачити майбутнє — створити його.",
            "Все, що ви можете уявити — реальне."
        };

        return quotes[rnd.Next(quotes.Count)];
    }
}
