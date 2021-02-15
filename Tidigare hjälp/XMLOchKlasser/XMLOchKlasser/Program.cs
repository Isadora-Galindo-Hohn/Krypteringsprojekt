using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XMLOchKlasser
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Media> media = new List<Media>();

            //Ladda in XML-dokumentet:

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("media.xml");

            //Ladda in alla book-noder:
            XmlNodeList books = xmlDoc.SelectNodes("media/books/book");

            //Skapa ett Book-element för varje nod och lägg i media listan:
            foreach(XmlNode book in books)
            {
                string author = book.SelectSingleNode("author").InnerText;
                string title = book.SelectSingleNode("title").InnerText;
                string language = book.SelectSingleNode("language").InnerText;
                string description = book.SelectSingleNode("description").InnerText;
                Book temp = new Book(title, language, description, author);
                media.Add(temp);
            }

            //Ladda in alla movie-noder:
            XmlNodeList movies = xmlDoc.SelectNodes("media/movies/movie");

            //Skapa ett Movie-element för varje nod och lägg i media-lista:
            foreach(XmlNode movie in movies)
            {
                string director = movie.SelectSingleNode("director").InnerText;
                string title = movie.SelectSingleNode("title").InnerText;
                string language = movie.SelectSingleNode("language").InnerText;
                string description = movie.SelectSingleNode("description").InnerText;
                Movie temp = new Movie(title, language, description, director);
                media.Add(temp);
            }

            //Skriv ut alla element i media-listan:
            foreach(Media m in media)
            {
                m.Print();
            }

            XmlDocument newXmlDoc = new XmlDocument();

            XmlDeclaration xmlDeclaration = newXmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            newXmlDoc.AppendChild(xmlDeclaration);

            XmlElement newMedia = newXmlDoc.CreateElement("media");
            newXmlDoc.AppendChild(newMedia);

            XmlElement newBooks = newXmlDoc.CreateElement("books");
            newMedia.AppendChild(newBooks);

            XmlElement newMovies = newXmlDoc.CreateElement("movies");
            newMedia.AppendChild(newMovies);
            foreach (Media item in media)
            {
                if (item is Book)
                {
                    Book book = (Book)item;
                    XmlElement newBook = newXmlDoc.CreateElement("book");
                    newBooks.AppendChild(newBook);

                    XmlElement author = newXmlDoc.CreateElement("author");
                    author.InnerText = book.Author;
                    newBook.AppendChild(author);

                    XmlElement title = newXmlDoc.CreateElement("title");
                    title.InnerText = book.Title;
                    newBook.AppendChild(title);

                    XmlElement language = newXmlDoc.CreateElement("language");
                    language.InnerText = book.Language;
                    newBook.AppendChild(language);

                    XmlElement description = newXmlDoc.CreateElement("description");
                    description.InnerText = book.Description;
                    newBook.AppendChild(description);
                }
            }

            foreach (Media item in media)
            {
                if (item is Movie)
                {
                    Movie movie = (Movie)item;
                    XmlElement newMovie = newXmlDoc.CreateElement("movie");
                    newMovies.AppendChild(newMovie);

                    XmlElement director = newXmlDoc.CreateElement("director");
                    director.InnerText = movie.Director;
                    newMovie.AppendChild(director);

                    XmlElement title = newXmlDoc.CreateElement("title");
                    title.InnerText = movie.Title;
                    newMovie.AppendChild(title);

                    XmlElement language = newXmlDoc.CreateElement("language");
                    language.InnerText = movie.Language;
                    newMovie.AppendChild(language);

                    XmlElement description = newXmlDoc.CreateElement("description");
                    description.InnerText = movie.Description;
                    newMovie.AppendChild(description);
                }
            }

            newXmlDoc.Save("nymedia.xml");

            Console.ReadLine();
        }
    }
}
