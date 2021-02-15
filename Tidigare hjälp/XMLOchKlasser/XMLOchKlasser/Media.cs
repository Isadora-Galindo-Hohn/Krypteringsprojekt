using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLOchKlasser
{
    abstract class Media
    {
        protected string title; 
        protected string language; 
        protected string description;

        public Media(string title, string language, string description)
        {
            this.title = title;
            this.language = language;
            this.description = description;
        }

        public string Title
        {
            get { return title; }
        }

        public string Language
        {
            get { return language; }
        }

        public string Description
        {
            get { return description; }
        }

        public abstract void Print();
    }
}
