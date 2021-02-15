using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static TcpListener tcpListener;
        // =======================================================================
        // Main(), lyssnar efter trafik. Loopar till dess att ctrl-C trycks. I
        // varje varv i loopen väntar servern på en ny anslutning.
        // =======================================================================
        public static void Main()
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPress);
            // Skapa ett TcpListener-objekt, börja lyssna och vänta på anslutning
            IPAddress myIp = IPAddress.Parse("127.0.0.1");
            tcpListener = new TcpListener(myIp, 8001);
            tcpListener.Start();
            while (true)
            {
                try
                {
                    Console.WriteLine("Väntar på anslutning...");

                    // Någon försöker ansluta. Acceptera anslutningen
                    Socket socket = tcpListener.AcceptSocket();
                    Console.WriteLine("Anslutning accepterad från " + socket.RemoteEndPoint);

                    // Tag emot meddelandet
                    byte[] bMessage = new byte[256];
                    int messageSize = socket.Receive(bMessage);
                    Console.WriteLine("Meddelandet mottogs...");

                    // Konvertera meddelandet till en string-variabel och skriv ut
                    string message = "";
                    for (int i = 0; i < messageSize; i++) {
                        if (i % 2 == 0)
                        {
                            message += Convert.ToChar(bMessage[i]);
                        }
                    }

                    Console.WriteLine("Meddelande: " + message);
                    byte[] bSend = Encoding.Unicode.GetBytes("Meddelande mottaget");
                    socket.Send(bSend);
                    Console.WriteLine("Svar skickat");
                    
                    // Stäng anslutningen mot just den här klienten:
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }
        // =======================================================================
        // CancelKeyPress(), anropas då användaren trycker på Ctrl-C.
        // =======================================================================
        static void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            // Sluta lyssna efter trafik:
            tcpListener.Stop();
            Console.WriteLine("Servern stängdes av!");
        }
    }
}
