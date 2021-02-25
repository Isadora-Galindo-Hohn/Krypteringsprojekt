using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrypteringsServer
{
    class Användare
    {
        string användarNamn;
        string användarLösenord;
        List<Meddelande> meddelanden = new List<Meddelande>();

        public Användare(string n, string l)
        {
            this.användarNamn = n;
            this.användarLösenord = l;
        }

        public string AnvändarNamn
        {
            get
            {
                return användarNamn;
            }
        }

        public string AnvändarLösenord
        {
            get
            {
                return användarLösenord;
            }
        }

        public List<Meddelande> Meddelanden
        {
            get
            {
                return meddelanden;
            }
        }
    }
}
