using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Threading;

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

            if (File.Exists("användare.xml"))//Här laddas det redan existerande xml dokument
            {
                //Ladda in alla book-noder:
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("användare.xml");
                XmlNodeList användarna = xmlDoc.SelectNodes("allaAnvändare/användare");
                XmlNodeList meddelanden = xmlDoc.SelectNodes("allaAnvändare/användare/meddelanden/meddelande");

                //Skapa ett Book-element för varje nod och lägg i media listan:
                foreach (XmlNode användare in användarna)
                {
                    string användarnamn = användare.SelectSingleNode("användarNamn").InnerText;
                    string användarLöseord = användare.SelectSingleNode("användarLösenord").InnerText;
                    Användare tempA = new Användare(användarnamn, användarLöseord);

                    foreach (XmlNode meddelande in meddelanden)
                    {
                        string meddelandeID = meddelande.SelectSingleNode("meddelandeID").InnerText;
                        string meddelandeText = meddelande.SelectSingleNode("meddelandeText").InnerText;
                        Meddelande tempM = new Meddelande(meddelandeID, meddelandeText);

                        tempA.Meddelanden.Add(tempM);
                    }
                    användarLista.Add(tempA);
                }
            }

            bool inLoggad = false;

            // Skapa ett TcpListener-objekt, börja lyssna och vänta på anslutning
            IPAddress myIp = IPAddress.Parse("127.0.0.1");
            tcpListener = new TcpListener(myIp, 8001);
            tcpListener.Start();

            while (true)
            {
                try
                {
                    //meny som består av en switch som skickar använderen till olika metoder beroende på vilket case som anropas
                    string menyVal = "";

                    while (menyVal != "7")
                    {
                        Console.Clear();
                        Console.WriteLine("Väntar på anslutning...");
                        Console.WriteLine("I meny");

                        // Någon försöker ansluta. Acceptera anslutningen
                        Socket socket = tcpListener.AcceptSocket();
                        Console.WriteLine("Anslutning accepterad från " + socket.RemoteEndPoint);

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

                        switch (menyVal)
                        {
                            case "1":

                                SkapaAnvändare(användarLista);
                                break;

                            case "2":
                                LoggIn(användarLista, inLoggad);

                                break;

                            case "3":
                                NyttMeddelande(användarLista);
                                break;

                            case "4":
                                VisaMeddelanden(användarLista);
                                break;

                            case "5":
                                LaddaNerMeddelanden(användarLista);
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
                    Socket socket = tcpListener.AcceptSocket();
                    Console.WriteLine("Anslutning accepterad från " + socket.RemoteEndPoint);

                    Console.WriteLine("Skapa användare");

                    // Någon försöker ansluta. Acceptera anslutningen

                    bool existerandeAnvändare = false;
                    byte[] läsAnvändarskapningsStatusByte;

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
                            xmlWriter.WriteStartElement("meddelande");
                            xmlWriter.WriteEndElement();

                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteEndDocument();
                            xmlWriter.Flush();
                            xmlWriter.Close();
                        }

                        användarLista.Add(new Användare(användarNamn, användarLösenord));
                        läsAnvändarskapningsStatusByte = Encoding.Unicode.GetBytes("Användare skapad!");
                        Console.WriteLine("Användare skapad!");
                        socket.Send(läsAnvändarskapningsStatusByte);
                    }
                    else if (File.Exists("användare.xml"))//Här laddas det redan existerande xml dokument
                    {
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
                            Console.WriteLine("Användarnamnet är upptaget, försök igen.");
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
                            Console.WriteLine("Användare skapad!");
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

        static bool LoggIn(List<Användare> användarLista, bool inLoggad)
        {
            while (true)
            {
                try
                {
                    // =======================================================================
                    // Logga in
                    // =======================================================================

                    Socket socket = tcpListener.AcceptSocket();
                    Console.WriteLine("Anslutning accepterad från " + socket.RemoteEndPoint);

                    Console.WriteLine("Logga in");

                    byte[] läsInLoggningsStatusByte;
                    string loggInNamn;
                    string loggInLösenord;
                    bool existerandeAnvändare = false;
                    bool korrektLösenord = false;

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
                                Console.WriteLine("Du är nu inloggad!");
                                socket.Send(läsInLoggningsStatusByte);
                            }
                            else
                            {
                                inLoggad = false;
                                läsInLoggningsStatusByte = Encoding.Unicode.GetBytes("Felaktigt lösenord.");
                                Console.WriteLine("Felaktigt lösenord.");
                                socket.Send(läsInLoggningsStatusByte);
                            }
                        }
                        else
                        {
                            läsInLoggningsStatusByte = Encoding.Unicode.GetBytes("Ingen användare vid det namnet existerar.");
                            Console.WriteLine("Ingen användare vid det namnet existerar.");
                            socket.Send(läsInLoggningsStatusByte);
                            inLoggad = false;
                        }
                    }
                    else
                    {
                        läsInLoggningsStatusByte = Encoding.Unicode.GetBytes("Det finns inga skapade konton.");
                        Console.WriteLine("Det finns inga skapade konton.");
                        socket.Send(läsInLoggningsStatusByte);
                        inLoggad = false;
                    }
                    // Stäng anslutningen mot just den här klienten:
                    socket.Close();
                    Console.WriteLine("socket stängd");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }

                return inLoggad;
            }
        }

        static void NyttMeddelande(List<Användare> användarLista)
        {
            while (true)
            {
                try
                {
                    Socket socket = tcpListener.AcceptSocket();
                    Console.WriteLine("Anslutning accepterad från " + socket.RemoteEndPoint);

                    Console.WriteLine("Meddeladen");

                    byte[] läsMeddelandeStatus;

                    byte[] sändareByte = new byte[1000];
                    string sändare = "";
                    int sändareByteStorlek;

                    sändareByteStorlek = socket.Receive(sändareByte);
                    Console.WriteLine();
                    Console.WriteLine("Sändare mottogs...");

                    for (int i = 0; i < sändareByteStorlek; i++)
                    {
                        if (i % 2 == 0)
                        {
                            sändare += Convert.ToChar(sändareByte[i]);
                        }
                    }

                    byte[] nyttMeddelandeByte = new byte[10000];
                    string nyttMeddelande = "";
                    int nyttMeddelandeByteStorlek;

                    // Tag emot meddelande
                    nyttMeddelandeByteStorlek = socket.Receive(nyttMeddelandeByte);
                    Console.WriteLine();
                    Console.WriteLine("Skapande av nytt meddelande mottogs...");

                    // Konvertera meddelande till string från byte
                    for (int i = 0; i < nyttMeddelandeByteStorlek; i++)
                    {
                        if (i % 2 == 0)
                        {
                            nyttMeddelande += Convert.ToChar(nyttMeddelandeByte[i]);
                        }
                    }

                    DateTime meddelandeID = DateTime.Now;

                    foreach (Användare a in användarLista)
                    {
                        if (Kryptering.Avkryptera(a.AnvändarNamn) == Kryptering.Avkryptera(sändare))
                        {
                            a.Meddelanden.Add(new Meddelande(meddelandeID.ToString(), nyttMeddelande));
                            break;
                        }
                    }

                    File.Delete("användare.xml");

                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                    xmlWriterSettings.Indent = true;
                    xmlWriterSettings.NewLineOnAttributes = true;

                    using (XmlWriter xmlWriter = XmlWriter.Create("användare.xml", xmlWriterSettings))
                    {
                        //Skapar layouten och tillsätter den första användaren
                        xmlWriter.WriteStartDocument();
                        xmlWriter.WriteStartElement("allaAnvändare");

                        for (int i = 0; i < användarLista.Count; i++)
                        {
                            if (användarLista[i].Meddelanden.Count > 0)
                            {
                                xmlWriter.WriteStartElement("användare");
                                xmlWriter.WriteElementString("användarNamn", användarLista[i].AnvändarNamn);
                                xmlWriter.WriteElementString("användarLösenord", användarLista[i].AnvändarLösenord);
                                xmlWriter.WriteStartElement("meddelanden");

                                foreach (Meddelande m in användarLista[i].Meddelanden)
                                {
                                    xmlWriter.WriteStartElement("meddelande");
                                    xmlWriter.WriteElementString("meddelandeID", m.MeddelandeID);
                                    xmlWriter.WriteElementString("meddelandeText", m.MeddelandeText);
                                    xmlWriter.WriteEndElement();
                                }

                                xmlWriter.WriteEndElement();
                                xmlWriter.WriteEndElement();
                            }
                            else
                            {
                                xmlWriter.WriteStartElement("användare");
                                xmlWriter.WriteElementString("användarNamn", användarLista[i].AnvändarNamn);
                                xmlWriter.WriteElementString("användarLösenord", användarLista[i].AnvändarLösenord);
                                xmlWriter.WriteEndElement();
                            }
                        }

                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndDocument();
                        xmlWriter.Flush();
                        xmlWriter.Close();
                    }

                    läsMeddelandeStatus = Encoding.Unicode.GetBytes("Meddelande skapat!");
                    Console.WriteLine("Meddelande skapat!");
                    socket.Send(läsMeddelandeStatus);

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }

                Console.WriteLine("Svar skickat");
                break;
            }
        }

        static void VisaMeddelanden(List<Användare> användarLista)
        {
            while (true)
            {
                try
                {
                    Socket socket = tcpListener.AcceptSocket();
                    Console.WriteLine("Anslutning accepterad från " + socket.RemoteEndPoint);

                    Console.WriteLine("Visa meddeladen");

                    byte[] läsMeddelande;
                    byte[] läsAntalMeddelanden;
                    List<Meddelande> allaMeddelanden = new List<Meddelande>();

                    byte[] sändareByte = new byte[1000];
                    string sändare = "";
                    int sändareByteStorlek;

                    sändareByteStorlek = socket.Receive(sändareByte);
                    Console.WriteLine();

                    for (int i = 0; i < sändareByteStorlek; i++)
                    {
                        if (i % 2 == 0)
                        {
                            sändare += Convert.ToChar(sändareByte[i]);
                        }
                    }

                    Console.WriteLine("Sändare mottogs...");

                    foreach (Användare a in användarLista)
                    {
                        if (Kryptering.Avkryptera(a.AnvändarNamn) == Kryptering.Avkryptera(sändare))
                        {
                            foreach (Meddelande m in a.Meddelanden)
                            {
                                allaMeddelanden.Add(m);
                            }
                            Console.WriteLine("meddelanden till laggda");

                            läsAntalMeddelanden = Encoding.Unicode.GetBytes($"{allaMeddelanden.Count}");
                            Console.WriteLine($"Antal meddelanden: {allaMeddelanden.Count}");
                            socket.Send(läsAntalMeddelanden);

                            for (int i = 0; i < allaMeddelanden.Count; i++)
                            {
                                läsMeddelande = Encoding.Unicode.GetBytes($"{allaMeddelanden[i].MeddelandeText}");
                                Console.WriteLine($"{i + 1} av {allaMeddelanden.Count} skickade");
                                socket.Send(läsMeddelande);
                            }
                            break;
                        }
                    }

                    Console.WriteLine("Meddelanden skickat");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }

                break;
            }
        }

        static void LaddaNerMeddelanden(List<Användare> användarLista)
        {
            while (true)
            {
                try
                {
                    Socket socket = tcpListener.AcceptSocket();
                    Console.WriteLine("Anslutning accepterad från " + socket.RemoteEndPoint);

                    Console.WriteLine("Visa meddeladen");

                    byte[] läsMeddelande;
                    byte[] läsMeddelandeID;
                    byte[] läsAntalMeddelanden;
                    List<Meddelande> allaMeddelanden = new List<Meddelande>();

                    byte[] sändareByte = new byte[1000];
                    string sändare = "";
                    int sändareByteStorlek;

                    sändareByteStorlek = socket.Receive(sändareByte);
                    Console.WriteLine();

                    for (int i = 0; i < sändareByteStorlek; i++)
                    {
                        if (i % 2 == 0)
                        {
                            sändare += Convert.ToChar(sändareByte[i]);
                        }
                    }

                    Console.WriteLine("Sändare mottogs...");

                    foreach (Användare a in användarLista)
                    {
                        if (Kryptering.Avkryptera(a.AnvändarNamn) == Kryptering.Avkryptera(sändare))
                        {
                            foreach (Meddelande m in a.Meddelanden)
                            {
                                allaMeddelanden.Add(m);
                            }
                            Console.WriteLine("meddelanden till laggda");

                            läsAntalMeddelanden = Encoding.Unicode.GetBytes($"{allaMeddelanden.Count}");
                            Console.WriteLine($"Antal meddelanden: {allaMeddelanden.Count}");
                            socket.Send(läsAntalMeddelanden);

                            for (int i = 0; i < allaMeddelanden.Count; i++)
                            {
                                läsMeddelande = Encoding.Unicode.GetBytes($"{allaMeddelanden[i].MeddelandeText}");
                                Console.WriteLine($"{i + 1} av {allaMeddelanden.Count} skickade");
                                socket.Send(läsMeddelande);
                            }

                            for (int i = 0; i < allaMeddelanden.Count; i++)
                            {
                                läsMeddelandeID = Encoding.Unicode.GetBytes($"{allaMeddelanden[i].MeddelandeID}");
                                Console.WriteLine($"{i + 1} av {allaMeddelanden.Count} skickade");
                                socket.Send(läsMeddelandeID);
                            }
                            break;
                        }
                    }

                    Console.WriteLine("Meddelanden skickat");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }

                break;
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