﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace KrypteringsServer
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

            //Lägga alla användare i en lista som sedan sickas till 
            List<Användare> användarLista = new List<Användare>();

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
                    //meny som består av en switch som skickar använderen till olika metoder beroende på vilket case som anropas
                    string menyVal = "";

                    while (menyVal != "7")
                    {
                        // Tag emot metodval
                        byte[] menyValByte = new byte[1000];
                        int menyValByteStorlek = socket.Receive(menyValByte);
                        menyVal = "";

                        for (int i = 0; i < menyValByteStorlek; i++)
                        {
                            if (i % 2 == 0)
                            {
                                menyVal += Convert.ToChar(menyValByte[i]);
                            }
                        }

                        Console.Clear();
                        switch (menyVal)
                        {
                            case "1":
                                SkapaAnvändare(användarLista);
                                break;

                            case "2":
                                LoggIn(användarLista);
                                break;

                            case "3":
                                break;

                            case "4":
                                break;

                            case "5":
                                break;

                            case "6":
                                break;

                            default: //default gör att så läge stringen inte är lika med 1-7 går den hit och visar menyn igen
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }

        static List<Användare> SkapaAnvändare(List<Användare> användarLista)
        {

            while (true)
            {
                try
                {
                    Console.WriteLine("Väntar på anslutning...");

                    // Någon försöker ansluta. Acceptera anslutningen
                    Socket socket = tcpListener.AcceptSocket();
                    Console.WriteLine("Anslutning accepterad från " + socket.RemoteEndPoint);

                    bool existerandeAnvändare = false;

                    // Tag emot användare
                    byte[] bNyAnvändare = new byte[1000];
                    int användarStorlek = socket.Receive(bNyAnvändare);
                    Console.WriteLine();
                    Console.WriteLine("Skapande av ny användre mottogs...");


                    // Konvertera användare till string från byte
                    string nyAnvändare = "";
                    for (int i = 0; i < användarStorlek; i++)
                    {
                        if (i % 2 == 0)
                        {
                            nyAnvändare += Convert.ToChar(bNyAnvändare[i]);
                        }
                    }

                    //Splittar stringen vid komma tecknet
                    string[] användarSplittare = nyAnvändare.Split(',');
                    string användarNamn = användarSplittare[0];
                    string användarLösenord = användarSplittare[1];

                    //Skapar en ny fil för användare eller öppnar en redan exiseterande fil
                    if (!File.Exists("användare.xml"))
                    {
                        XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                        xmlWriterSettings.Indent = true;
                        xmlWriterSettings.NewLineOnAttributes = true;

                        using (XmlWriter xmlWriter = XmlWriter.Create("användare.xml", xmlWriterSettings))
                        {
                            //Skapar layouten och tillsätter den första användaren
                            xmlWriter.WriteStartDocument();
                            xmlWriter.WriteStartElement("allaAnvändare");

                            xmlWriter.WriteStartElement("användare");
                            xmlWriter.WriteElementString("användarNamn", användarNamn);
                            xmlWriter.WriteElementString("användarLösenord", användarLösenord);
                            xmlWriter.WriteEndElement();

                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteEndDocument();
                            xmlWriter.Flush();
                            xmlWriter.Close();
                        }
                    }
                    else if (File.Exists("användare.xml"))//Här laddas det redan existerande xml dokument
                    {
                        //Ladda in alla book-noder:
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load("användare.xml");
                        XmlNodeList användarna = xmlDoc.SelectNodes("allaAnvändare/användare");

                        byte[] läsAnvändarskapningsStatusByte;

                        //Skapa ett Book-element för varje nod och lägg i media listan:
                        foreach (XmlNode användare in användarna)
                        {
                            string användarnamn = användare.SelectSingleNode("användarNamn").InnerText;
                            string användarLöseord = användare.SelectSingleNode("användarLösenord").InnerText;
                            Användare temp = new Användare(användarnamn, användarLöseord);
                            användarLista.Add(temp);
                        }

                        existerandeAnvändare = false;

                        for (int i = 0; i < användarLista.Count; i++)
                        {
                            if (Kryptering.Avkryptera(användarLista[i].AnvändarNamn) == Kryptering.Avkryptera(användarNamn))
                            {
                                existerandeAnvändare = true;
                            }
                        }

                        if (existerandeAnvändare == true)
                        {
                            läsAnvändarskapningsStatusByte = Encoding.Unicode.GetBytes("Användarnamnet är upptaget, försök igen.");
                            socket.Send(läsAnvändarskapningsStatusByte);
                        }
                        else if (existerandeAnvändare == false)
                        {
                            XDocument xDocument = XDocument.Load("användare.xml");
                            XElement root = xDocument.Element("allaAnvändare");
                            IEnumerable<XElement> rows = root.Descendants("användare");
                            XElement firstRow = rows.First();

                            //Lägger in ett nytt element högst upp
                            firstRow.AddBeforeSelf(
                               new XElement("användare",
                               new XElement("användarNamn", användarNamn),
                               new XElement("användarLösenord", användarLösenord)));
                            xDocument.Save("användare.xml");

                            användarLista.Add(new Användare(användarNamn, användarLösenord));
                            läsAnvändarskapningsStatusByte = Encoding.Unicode.GetBytes("Användare skapad!");
                            socket.Send(läsAnvändarskapningsStatusByte);
                        }
                    }

                    Console.WriteLine("Svar skickat");

                    return användarLista;

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }

        static void LoggIn(List<Användare> användarLista)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Väntar på anslutning...");

                    // Någon försöker ansluta. Acceptera anslutningen
                    Socket socket = tcpListener.AcceptSocket();
                    Console.WriteLine("Anslutning accepterad från " + socket.RemoteEndPoint);

                    // =======================================================================
                    // Logga in
                    // =======================================================================

                    byte[] läsInLoggningsStatusByte;
                    string loggInNamn;
                    string loggInLösenord;
                    bool existerandeAnvändare = false;
                    bool korrektLösenord = false;
                    bool inLoggad;

                    byte[] loggInByte = new byte[1000];
                    int loggInByteStorlek = socket.Receive(loggInByte);
                    Console.WriteLine();
                    Console.WriteLine("Inloggningsförsök mottogs...");

                    // Konvertera användare till string från byte
                    string loggIn = "";
                    for (int i = 0; i < loggInByteStorlek; i++)
                    {
                        if (i % 2 == 0)
                        {
                            loggIn += Convert.ToChar(loggInByte[i]);
                        }
                    }

                    //Splittar stringen vid komma tecknet
                    string[] loggInSplittare = loggIn.Split(',');
                    loggInNamn = loggInSplittare[0];
                    loggInLösenord = loggInSplittare[1];

                    if (användarLista.Count > 0)
                    {
                        for (int i = 0; i < användarLista.Count; i++)
                        {
                            if (Kryptering.Avkryptera(användarLista[i].AnvändarNamn) == Kryptering.Avkryptera(loggInNamn))
                            {
                                existerandeAnvändare = true;
                            }
                        }

                        if (existerandeAnvändare == true)
                        {
                            for (int i = 0; i < användarLista.Count; i++)
                            {
                                if (Kryptering.Avkryptera(användarLista[i].AnvändarLösenord) == Kryptering.Avkryptera(loggInLösenord))
                                {
                                    korrektLösenord = true;
                                }
                            }

                            if (korrektLösenord == true)
                            {
                                inLoggad = true;
                                läsInLoggningsStatusByte = Encoding.Unicode.GetBytes("Du är nu inloggad!");
                                socket.Send(läsInLoggningsStatusByte);
                            }
                            else
                            {
                                inLoggad = false;
                                läsInLoggningsStatusByte = Encoding.Unicode.GetBytes("Felaktigt lösenord.");
                                socket.Send(läsInLoggningsStatusByte);
                            }
                        }
                        else
                        {
                            läsInLoggningsStatusByte = Encoding.Unicode.GetBytes("Ingen användare vid det namnet existerar.");
                            socket.Send(läsInLoggningsStatusByte);
                        }
                    }
                    else
                    {
                        läsInLoggningsStatusByte = Encoding.Unicode.GetBytes("Det finns inga skapade konton.");
                        socket.Send(läsInLoggningsStatusByte);
                    }
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




/*
if (!File.Exists("användar.xml"))
{
    byte[] användarInfoByte = Encoding.Unicode.GetBytes("Det finns inga användare");
    socket.Send(användarInfoByte);
    Console.WriteLine("Användarlista skickat"); 
} 
else if (File.Exists("användare.xml"))
{
    //Lägga alla användare i en lista som sedan sickas till 
    List<Användare> användarLista = new List<Användare>();
    string användareIString;

    //Ladda in alla book-noder:
    XmlDocument xmlDoc = new XmlDocument();
    xmlDoc.Load("användare.xml");
    XmlNodeList användarna = xmlDoc.SelectNodes("allaAnvändare/användare");

    //Skapa ett Book-element för varje nod och lägg i media listan:
    foreach (XmlNode användare in användarna)
    {
        string användarnamn = användare.SelectSingleNode("användarNamn").InnerText;
        string användarLöseord = användare.SelectSingleNode("användarLösenord").InnerText;
        Användare temp = new Användare(användarNamn, användarLöseord);
        användarLista.Add(temp);
    }

    användareIString = String.Join(",", användarLista);

    byte[] användarInfoByte = Encoding.Unicode.GetBytes("Det finns inga användare");
    socket.Send(användarInfoByte);
    Console.WriteLine("Användarlista skickat");

}
*/
/*Console.WriteLine("Meddelande: " + message);
                   byte[] bSend = Encoding.Unicode.GetBytes("Meddelande mottaget");
                   socket.Send(bSend);
                   Console.WriteLine("Svar skickat");*/

/* byte[] bMessage = new byte[256];
            int messageSize = socket.Receive(bMessage);
            Console.WriteLine("Meddelandet mottogs...");

            // Konvertera meddelandet till en string-variabel och skriv ut
            string message = "";
            for (int i = 0; i < messageSize; i++)
            {
                if (i % 2 == 0)
                {
                    message += Convert.ToChar(bMessage[i]);
                }
            }

            Console.WriteLine("Meddelande: " + message);
            byte[] bSend = Encoding.Unicode.GetBytes("Meddelande mottaget");
            socket.Send(bSend);*/