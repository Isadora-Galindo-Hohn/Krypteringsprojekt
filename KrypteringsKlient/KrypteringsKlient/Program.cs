﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace KrypteringsKlient
{
    class Program
    {
        static void Main(string[] args)
        {
            // Skapa ett TcpListener-objekt, börja lyssna och vänta på anslutning
            IPAddress myIp = IPAddress.Parse("127.0.0.1");

            while (true)
            {
                try
                {
                    Console.WriteLine("Väntar på anslutning...");

                    string address = "127.0.0.1"; // är en local host
                    int port = 8001;
                    TcpClient tcpClient = new TcpClient();
                    tcpClient.Connect(address, port);

                    //meny som består av en switch som skickar använderen till olika metoder beroende på vilket case som anropas
                    string menyVal = "";
                    bool inLoggad = false;

                    while (menyVal != "8")
                    {
                        Console.WriteLine();
                        Console.WriteLine("----------------------------------------------------");
                        Console.WriteLine("                     MENY                           ");
                        Console.WriteLine("----------------------------------------------------");
                        Console.WriteLine();
                        Console.WriteLine("1. Skapa användare");//klar men titta upp hur många bytes som behövs i servern kolla även om användarnamnet är upptaget
                        Console.WriteLine("2. Logga in"); //Klar
                        Console.WriteLine("3. Skapa ett nytt meddelande");
                        Console.WriteLine("4. Visa alla meddelanden (dekrypterade)");
                        Console.WriteLine("5. Hämta sparade meddelade(från XML - fil)");
                        Console.WriteLine("6. Spara alla meddelande(i XML - fil)");
                        Console.WriteLine("7. Logga ut");//Klar
                        Console.WriteLine("8. Avsluta programmet");//Klar
                        Console.WriteLine("Skriv in siffra mellan 1-7 och tryck sedan Enter...");
                        Console.WriteLine();

                        menyVal = Console.ReadLine();

                        byte[] menyValByte = Encoding.Unicode.GetBytes(menyVal);

                        //Sickar iväg användaren till servern
                        NetworkStream tcpStream = tcpClient.GetStream();
                        tcpStream.Write(menyValByte, 0, menyValByte.Length);

                        Console.Clear();
                        switch (menyVal)
                        {
                            case "1":
                                inLoggad = SkapaAnvändare(inLoggad);
                                break;

                            case "2":
                                inLoggad = LoggaIn(inLoggad);
                                break;

                            case "3":
                                NyttMeddelande(inLoggad);
                                break;

                            case "4":
                                break;

                            case "5":
                                break;

                            case "6":
                                break;

                            case "7":
                                Console.WriteLine("Du är nu utloggad");
                                inLoggad = false;
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

        // I skapa användare metoden skapas användare som lagras i servern i ett xml document
        static bool SkapaAnvändare(bool inLoggad)
        {
            if (inLoggad == true)
            {
                Console.WriteLine("Du har redan loggat in.");
                Console.WriteLine("Om du vill logga ut måste du först logga ut.");
                Console.WriteLine("Tryck på enter för att gå vidare till menyn.");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Skapa användare:");
                Console.WriteLine("");
                Console.WriteLine("-----------------------------------------------------------------------------------");
                Console.WriteLine("");
                Console.WriteLine("Skapa ett användarnamn som består av 1 till 16 tecken.");
                Console.WriteLine("Skapa ett lösenord som består av 1 till 16 tecken.");
                Console.WriteLine("Om ditt användare namn är upptaget kommer du få skriva om igen.");
                Console.WriteLine("Ange sedan det lösenordet du vill ha.");
                Console.WriteLine();
                Console.WriteLine("-----------------------------------------------------------------------------------");

                try //Försöker ansluta till servern om det inte fungerar går det vidare till exception
                {
                    string address = "127.0.0.1"; // är en local host
                    int port = 8001;

                    TcpClient tcpClient = new TcpClient();
                    tcpClient.Connect(address, port);


                    // Anslut till servern:
                    Console.WriteLine("Ansluter...");

                    //För att skapa en användare behövs användarnamn och lösenord dessa är båda i string form
                    string användarnamn;
                    string lösenord;
                    bool användareSkapad = false;

                    while (användareSkapad == false)//Pågar så länge en användare har skapats
                    {
                        //Skapar ett användarnamn inom avgränsningarna
                        while (true)
                        {
                            Console.Write("Användarnamn: ");
                            användarnamn = Console.ReadLine();
                            if (användarnamn.Length > 0 && användarnamn.Length < 17)
                            {
                                break;
                            }
                            else //Fel text för att underlätta för användaren att förstå vad som gick fel
                            {
                                Console.WriteLine();
                                Console.WriteLine("Ditt användarnamn ska vara mellan 1 till 16 tecken långt");
                                Console.WriteLine($"Ditt användarnamn hade {användarnamn.Length} antal tecken");
                                Console.WriteLine();
                            }
                        }

                        //Skapar ett lösenord inom avgränsningarna
                        while (true)
                        {
                            Console.Write("Lösenord: ");
                            lösenord = Console.ReadLine();
                            if (lösenord.Length > 0 && lösenord.Length < 17)
                            {
                                break;
                            }
                            else //Fel text för att underlätta för användaren att förstå vad som gick fel
                            {
                                Console.WriteLine();
                                Console.WriteLine("Ditt lösenord ska vara mellan 1 till 16 tecken långt");
                                Console.WriteLine($"Ditt lösenord hade {lösenord.Length} antal tecken");
                                Console.WriteLine();
                            }
                        }

                        //Skapa användare som sickas till serven i servern kommer stringen att splittas för att ge 2 stings
                        string användare = $"{Kryptering.Inkryptering(användarnamn)},{Kryptering.Inkryptering(lösenord)}";

                        byte[] bNyAnvändare = Encoding.Unicode.GetBytes(användare);

                        //Sickar iväg användaren till servern

                        NetworkStream tcpStream = tcpClient.GetStream();
                        tcpStream.Write(bNyAnvändare, 0, bNyAnvändare.Length);

                        byte[] läsAnvändarskapningsStatusByte = new byte[256];
                        int läsAnvändarskapningsStatusLängd = tcpStream.Read(läsAnvändarskapningsStatusByte, 0, läsAnvändarskapningsStatusByte.Length);

                        // Konvertera meddelandet till ett string-objekt och skriv ut:
                        string läsAnvändarskapningsStatus = "";
                        for (int i = 0; i < läsAnvändarskapningsStatusLängd; i++)
                        {
                            if (i % 2 == 0)
                            {
                                läsAnvändarskapningsStatus += Convert.ToChar(läsAnvändarskapningsStatusByte[i]);
                            }
                        }

                        if (läsAnvändarskapningsStatus == "Användarnamnet är upptaget, försök igen.")
                        {
                            Console.WriteLine();
                            Console.WriteLine(läsAnvändarskapningsStatus);
                            Console.WriteLine("Tryck på enter för att gå vidare till menyn.");
                            Console.ReadLine();
                            användareSkapad = true;
                        }

                        else if (läsAnvändarskapningsStatus == "Användare skapad!")
                        {
                            Console.WriteLine();
                            Console.WriteLine(läsAnvändarskapningsStatus);
                            Console.WriteLine("Tryck på enter för att gå vidare till menyn.");
                            Console.ReadLine();
                            användareSkapad = true;
                            inLoggad = true;
                        }
                    }

                    // Stäng anslutningen:
                    tcpClient.Close();
                }
                catch (Exception e)//felmedelande ifall servern inte svarar
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }

            return inLoggad;
        }

        static bool LoggaIn(bool inLoggad)
        {
            if (inLoggad == true)
            {
                Console.WriteLine("Du har redan loggat in.");
                Console.WriteLine("Om du vill logga ut måste du först logga ut.");
                Console.WriteLine("Tryck på enter för att gå vidare till menyn.");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Logga In:");
                Console.WriteLine("");
                Console.WriteLine("-----------------------------------------------------------------------------------");
                Console.WriteLine("");
                Console.WriteLine("Ange ditt användarnamn och lösenord för att logga in.");
                Console.WriteLine("Om användarnamnet eller lösenordet är felaktig kommer ett felmedelande upp.");
                Console.WriteLine("Om det inte existerar ett konto med det användarnamnet kan det skapas under 2.");
                Console.WriteLine();
                Console.WriteLine("-----------------------------------------------------------------------------------");

                try //Försöker ansluta till servern om det inte fungerar går det vidare till exception
                {
                    string address = "127.0.0.1"; // är en local host
                    int port = 8001;

                    TcpClient tcpClient = new TcpClient();
                    tcpClient.Connect(address, port);

                    NetworkStream tcpStream = tcpClient.GetStream();

                    //För att skapa en användare behövs användarnamn och lösenord dessa är båda i string form
                    string användarnamn;
                    string lösenord;

                    // Anslut till servern:
                    Console.WriteLine("Ansluter...");

                        //Skapar ett användarnamn inom avgränsningarna
                        while (true)
                        {
                            Console.Write("Användarnamn: ");
                            användarnamn = Console.ReadLine();
                            if (användarnamn.Length > 0 && användarnamn.Length < 17)
                            {
                                break;
                            }
                            else //Fel text för att underlätta för användaren att förstå vad som gick fel
                            {
                                Console.WriteLine();
                                Console.WriteLine("Användarnamn kan enbart ha 16 tecken");
                                Console.WriteLine($"Ditt användarnamn hade {användarnamn.Length} tecken");
                                Console.WriteLine();
                            }
                        }

                        //Skapar ett lösenord inom avgränsningarna
                        while (true)
                        {
                            Console.Write("Lösenord: ");
                            lösenord = Console.ReadLine();
                            if (lösenord.Length > 0 && lösenord.Length < 17)
                            {
                                break;
                            }
                            else //Fel text för att underlätta för användaren att förstå vad som gick fel
                            {
                                Console.WriteLine();
                                Console.WriteLine("Lösenord kan enbart vara 16 tecken");
                                Console.WriteLine($"Ditt lösenord hade {lösenord.Length} tecken");
                                Console.WriteLine();
                            }
                        }

                        //Skapa användare som sickas till serven i servern kommer stringen att splittas för att ge 2 stings
                        string användare = $"{Kryptering.Inkryptering(användarnamn)},{Kryptering.Inkryptering(lösenord)}";

                        byte[] loggInByte = Encoding.Unicode.GetBytes(användare);

                        //Sickar iväg användaren till servern
                        tcpStream.Write(loggInByte, 0, loggInByte.Length);


                        byte[] läsInLoggningsStatusByte = new byte[256];
                        int läsInLoggningsStatusByteLängd = tcpStream.Read(läsInLoggningsStatusByte, 0, läsInLoggningsStatusByte.Length);

                        // Konvertera meddelandet till ett string-objekt och skriv ut:
                        string läsInLoggningsStatus = "";
                        for (int i = 0; i < läsInLoggningsStatusByteLängd; i++)
                        {
                            if (i % 2 == 0)
                            {
                                läsInLoggningsStatus += Convert.ToChar(läsInLoggningsStatusByte[i]);
                            }
                        }

                        if (läsInLoggningsStatus == "Det finns inga skapade konton.")
                        {
                            Console.WriteLine();
                            Console.WriteLine(läsInLoggningsStatus);
                            Console.WriteLine("Tryck på enter för att gå vidare till menyn.");
                            Console.ReadLine();
                        }
                        else if (läsInLoggningsStatus == "Ingen användare vid det namnet existerar.")
                        {
                            Console.WriteLine();
                            Console.WriteLine(läsInLoggningsStatus);
                            Console.WriteLine("Tryck på enter för att gå vidare till menyn.");
                            Console.ReadLine();
                        }
                        else if (läsInLoggningsStatus == "Du är nu inloggad!")
                        {
                            Console.WriteLine();
                            Console.WriteLine(läsInLoggningsStatus);
                            Console.WriteLine("Du kan nu titta på och skicka meddelanden");
                            Console.WriteLine("Tryck på enter för att gå vidare till menyn.");
                            Console.ReadLine();
                            inLoggad = true;
                        }
                        else if (läsInLoggningsStatus == "Felaktigt lösenord.")
                        {
                            Console.WriteLine();
                            Console.WriteLine(läsInLoggningsStatus);
                            Console.WriteLine("Tryck på enter för att gå vidare till menyn.");
                            Console.ReadLine();
                        }
                        // Stäng anslutningen:
                        tcpClient.Close();
                    
                }
                catch (Exception e)//felmedelande ifall servern inte svarar
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
            return inLoggad;
        }

        static void NyttMeddelande(bool inLoggad)
        {
            if (inLoggad == false)
            {
                Console.WriteLine("Du måste logga in för att se ");
                Console.WriteLine("Om du vill logga ut måste du först logga ut.");
                Console.WriteLine("Tryck på enter för att gå vidare till menyn.");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("             SKAPA ETT NYTT MEDDELANDE              ");
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine();
                Console.WriteLine("Skriv in ett meddelande.");
                Console.WriteLine("Meddelanden ska vara mellan 1 till 250 tecken långt");
                Console.WriteLine();
                Console.WriteLine("----------------------------------------------------");

                try //Försöker ansluta till servern om det inte fungerar går det vidare till exception
                {
                    string address = "127.0.0.1"; // är en local host
                    int port = 8001;

                    TcpClient tcpClient = new TcpClient();
                    tcpClient.Connect(address, port);

                    NetworkStream tcpStream = tcpClient.GetStream();

                    // Anslut till servern:
                    Console.WriteLine("Ansluter...");

                    string meddelande;
                    bool meddelandeSkapat = false;

                    while (meddelandeSkapat == false)//Pågar så länge en användare har skapats
                    {
                        //Skapar ett meddelande inom avgränsningarna
                        while (true)
                        {
                            Console.WriteLine("Meddelnde: ");
                            meddelande = Console.ReadLine();
                            if (meddelande.Length > 0 && meddelande.Length < 251)
                            {
                                break;
                            }
                            else //Feltext för att underlätta för användaren att förstå vad som gick fel
                            {
                                Console.WriteLine();
                                Console.WriteLine("Ditt meddelande ska vara mellan 1 till 250 tecken långt");
                                Console.WriteLine($"Ditt användarnamn hade {meddelande.Length} tecken");
                                Console.WriteLine();
                            }
                        }

                        byte[] nyttMeddelandeByte = Encoding.Unicode.GetBytes(Kryptering.Inkryptering(meddelande));

                        //Sickar iväg användaren till servern
                        tcpStream.Write(nyttMeddelandeByte, 0, nyttMeddelandeByte.Length);


                        byte[] läsMeddelandeStatusByte = new byte[256];
                        int läsMeddelandeStatusLängd = tcpStream.Read(läsMeddelandeStatusByte, 0, läsMeddelandeStatusByte.Length);

                        // Konvertera meddelandet till ett string-objekt och skriv ut:
                        string läsMeddelandeStatus = "";
                        for (int i = 0; i < läsMeddelandeStatusLängd; i++)
                        {
                            if (i % 2 == 0)
                            {
                                läsMeddelandeStatus += Convert.ToChar(läsMeddelandeStatusByte[i]);
                            }
                        }

                        if (läsMeddelandeStatus == "Meddelande skapat!")
                        {
                            Console.WriteLine();
                            Console.WriteLine(läsMeddelandeStatus);
                            Console.WriteLine("Tryck på enter för att gå vidare till menyn.");
                            Console.ReadLine();
                            meddelandeSkapat = true;
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("Tryck på enter för att gå vidare till menyn.");
                            Console.ReadLine();
                            meddelandeSkapat = true;
                        }

                        // Stäng anslutningen:
                        tcpClient.Close();
                    }

                }
                catch (Exception e)//felmedelande ifall servern inte svarar
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }

            Console.WriteLine("hej");
        }
    }
}

