using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace Client    
{
    class Program
    {
        public static void Main()
        {
            try
            {

                string address = "127.0.0.1";
                int port = 8001;

                string name = "";

                while (true)
                {
                    Console.Write("Namn: ");
                    name = Console.ReadLine();
                    if (name.Length > 0)
                    {
                        break;
                    }
                }

                // Anslut till servern:
                Console.WriteLine("Ansluter...");

                while (true)
                {

                    // Skriv in meddelandet att skicka:
                    string message = $"[Från användare: {name}] ";
                    while (true)
                    {
                        Console.Write("Meddelande: ");
                        message += Console.ReadLine();
                        if (message.Length > 0)
                        {
                            break;
                        }
                    }

                    TcpClient tcpClient = new TcpClient();
                    tcpClient.Connect(address, port);
                    Console.WriteLine("Ansluten!");
                    byte[] bMessage = Encoding.Unicode.GetBytes(message);


                    // Skicka iväg meddelandet:
                    Console.WriteLine("Skickar...");
                    NetworkStream tcpStream = tcpClient.GetStream();
                    tcpStream.Write(bMessage, 0, bMessage.Length);

                    // Tag emot meddelande från servern:
                    byte[] bRead = new byte[256];
                    int bReadSize = tcpStream.Read(bRead, 0, bRead.Length);

                    // Konvertera meddelandet till ett string-objekt och skriv ut:
                    string read = "";
                    for (int i = 0; i < bReadSize; i++) {
                        if (i % 2 == 0)
                        {
                        read += Convert.ToChar(bRead[i]);
                        }
                    } 

                    Console.WriteLine("Servern säger: " + read);

                    // Stäng anslutningen:
                    tcpClient.Close();
                }

            }

            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }
    }
}
