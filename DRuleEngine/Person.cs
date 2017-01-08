using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRuleEngine
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public int Children { get; set; }
        public bool Married { get; set; }
        public Adresse Adresse_ { get; set; }

        private List<Adresse> adresses = new List<Adresse>();
        public List<Adresse> Adresses_
        {
            get { return adresses; }
            set { adresses = value; }
        }
    }

    public class Adresse
    {
        public string Street { get; set; }
        public int Plz { get; set; }
        public string City { get; set; }
        public bool ActiveState { get; set; }
    }
}
