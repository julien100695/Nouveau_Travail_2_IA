namespace processAI2

{



    public class CoupPossible

    {

        string position;

        string arrive;

        int valeur;

        string pièce;



        public CoupPossible(string position, string arrive, int valeur, string pièce)

        {

            this.position = position;

            this.Arrive = arrive;

            this.Valeur = valeur;

            this.Pièce = pièce;

        }



        public string Position { get => position; set => position = value; }

        public string Arrive { get => arrive; set => arrive = value; }

        public int Valeur { get => valeur; set => valeur = value; }

        public string Pièce { get => pièce; set => pièce = value; }

    }

}