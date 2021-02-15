using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLOchKlasser
{
    class Movie : Media
    {
        string director; 

        public Movie(string title, string language, string description, string director) : base(title, language, description)
        {
            this.director = director;
        }
        public string Director
        {
            get { return director; }
        }
        public override void Print()
        {
            Console.WriteLine("=============================================================");
            Console.WriteLine("Filmen " + title + " är regisserad av " + director + " på språket " + language);
            Console.WriteLine("Den handlar om: " + description);
        }
    }
}
