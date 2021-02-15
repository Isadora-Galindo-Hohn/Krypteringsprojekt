using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLOchKlasser
{
    class Book : Media
    {
        string author;

        public Book(string title, string language, string description, string author) : base(title, language, description)
        {
            this.author = author;
        }

        public string Author
        {
            get { return author; }
        }

        public override void Print()
        {
            Console.WriteLine("=============================================================");
            Console.WriteLine("Boken " + title + " är skriven av " + author + " på språket " + language);
            Console.WriteLine("Den handlar om: " + description);
        }
    }
}
