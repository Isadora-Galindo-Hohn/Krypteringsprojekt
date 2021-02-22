using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrypteringsServer
{
    class Meddelande
    {
        string meddelandeID;
        string meddelande;

        public Meddelande(string mID, string m)
        {
            this.meddelandeID = mID;
            this.meddelande = m;
        }

        public string MeddelandeID
        {
            get
            {
                return meddelandeID;
            }
        }
        public string MeddelandeText
        {
            get
            {
                return meddelande;
            }
        }
    }
}
