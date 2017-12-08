using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace processAI1
{
    class Pieces
    {
        private String coordonnees;
        private double valeurs;
        private int position;

        public Pieces(string coordonnees, double valeurs, int position)
        {
            this.coordonnees = coordonnees;
            this.valeurs = valeurs;
            this.position = position;
        }

        public string Coordonnees { get => coordonnees; set => coordonnees = value; }
        public double Valeurs { get => valeurs; set => valeurs = value; }
        public int Position { get => position; set => position = value; }
    }
}
