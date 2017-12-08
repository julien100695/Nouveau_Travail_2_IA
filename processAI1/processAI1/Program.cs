using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using jeu_echec_stage;


namespace processAI1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                
                bool stop = false;
                int[] tabVal = new int[64];
                String value;
                String[] coord = new String[] { "", "", "" };
                String[] tabCoord = new string[] { "a8","b8","c8","d8","e8","f8","g8","h8",
                                                   "a7","b7","c7","d7","e7","f7","g7","h7",
                                                   "a6","b6","c6","d6","e6","f6","g6","h6",
                                                   "a5","b5","c5","d5","e5","f5","g5","h5",
                                                   "a4","b4","c4","d4","e4","f4","g4","h4",
                                                   "a3","b3","c3","d3","e3","f3","g3","h3",
                                                   "a2","b2","c2","d2","e2","f2","g2","h2",
                                                   "a1","b1","c1","d1","e1","f1","g1","h1" };

                while (!stop)
                {

                    using (var mmf = MemoryMappedFile.OpenExisting("plateau"))
                    {
                        using (var mmf2 = MemoryMappedFile.OpenExisting("repAI1"))
                        {
                            Mutex mutexStartAI1 = Mutex.OpenExisting("mutexStartAI1");
                            Mutex mutexAI1 = Mutex.OpenExisting("mutexAI1");
                            mutexAI1.WaitOne();

                            mutexStartAI1.WaitOne();

                            using (var accessor = mmf.CreateViewAccessor())
                            {
                                ushort Size = accessor.ReadUInt16(0);
                                byte[] Buffer = new byte[Size];
                                accessor.ReadArray(0 + 2, Buffer, 0, Buffer.Length);

                                value = ASCIIEncoding.ASCII.GetString(Buffer);
                                if (value == "stop") stop = true;
                                else
                                {
                                    String[] substrings = value.Split(',');
                                    for (int i = 0; i < substrings.Length; i++)
                                    {
                                        tabVal[i] = Convert.ToInt32(substrings[i]);
                                    }
                                }
                            }
                            if (!stop)
                            {
                                /******************************************************************************************************/
                                /***************************************** ECRIRE LE CODE DE L'IA *************************************/
                                /******************************************************************************************************/


                                Echiquier echiquier = Echiquier.Instance();

                                
                                
                                int trait = echiquier.getTrait();
                                int[] mouvementPion = new int[] { -11, -9 }; // Vecteurs de mouvement du pion.

                                // Création de la liste des pièces adverse que l'on peut manger
                                List<CasesAdversesManger> listeCasesAManger = new List<CasesAdversesManger>();

                                // On récupère la liste de nos pièces.
                                List<String> mesPieces = new List<String>();
                                List<Pieces> piecesAlli = new List<Pieces>(); // Creation de la liste de nos pièces.
                                List<Pieces> piecesEnnemies = new List<Pieces>(); // Creation de la liste des pièces ennemies.
                                var min = new List<Tuple<int, CasesAdversesManger>>(); // Liste comportant les tuples(casedépartallié,déplacement_ennemi)

                                List<CasesAdversesManger> deplacement_poss = new List<CasesAdversesManger>();

                                for (int i = 0; i < tabVal.Length; i++)
                                {
                                    if (tabVal[i] > 0) // Si ce sont des alliés
                                    {
                                        int position = PosForCoord(tabCoord[i]); // On récupère la position.
                                        // mesPieces.Add(tabCoord[i]); Si on veut les coordonnées de nos pièces, on peut rajouter cette ligne.
                                        Pieces piece = new Pieces(tabCoord[i], tabVal[i], position);
                                        piecesAlli.Add(piece); // On ajoute à la liste de pieces alliées.
                                    }
                                }

                                // On récupère la liste de tout ce qu'il reste.
                                List<String> reste = new List<String>(); // Cases vides.
                                for (int i = 0; i < tabVal.Length; i++)
                                {
                                    if (tabVal[i] <= 0) reste.Add(tabCoord[i]);
                                    if(tabVal[i]<= -1) // Si c'est une piece ennemie.
                                    {
                                        int position = PosForCoord(tabCoord[i]); // On récupère la position.
                                        Pieces piece = new Pieces(tabCoord[i], tabVal[i], position);
                                        piecesEnnemies.Add(piece); // On ajoute à la liste de pieces ennemies.
                                    }
                                }

                                /* Maintenant on remplit la liste des cases contenant des pièces que l'on peut manger 
                                et celles où l'on peut aller sans rencontrer d'obstacle. */

                                foreach (Pieces piece in piecesAlli)
                                {
                                    //Console.WriteLine(piece.Position);
                                }
                                foreach (Pieces piece in piecesAlli)
                                {
                                    /* Quelque soit la pièce, si le coup est valide sur une piece ennemie, alors on peut la manger          */ 
                                        foreach (Pieces pieceEnnemies in piecesEnnemies)
                                        {
                                        //Console.WriteLine("La position de la pièce est "  + piece.Position + "et ca valeur est" + piece.Valeurs);

                                            if (echiquier.valide(giveIndexForPosition(piece.Position), giveIndexForPosition(pieceEnnemies.Position)))
                                            {
                                            CasesAdversesManger newPiece = new CasesAdversesManger(piece.Position, pieceEnnemies.Position);
                                            listeCasesAManger.Add(newPiece);
                                            }
                                    }

                                        if(isPion(piece)) // Mais si la pièce est un pion on peut aussi prendre les cases en passant. C'est donc des cases où il n'y a pas d'ennemie dessus.
                                    {

                                        for (int i = 0; i < mouvementPion.Length; i++)
                                        {
                                            int positionCaseEvalué = piece.Position + mouvementPion[i] * trait; // Pour Chaque endroit ou le pion peut aller en diagonale.
                                            if (inTheTerrain(positionCaseEvalué))
                                            {
                                                //Console.WriteLine("la position de la case évaluée est " + positionCaseEvalué + " et son index est " + giveIndexForPosition(positionCaseEvalué));
                                                //Console.WriteLine(piece.Position + " " + positionCaseEvalué);
                                                if (echiquier.valide(giveIndexForPosition(piece.Position), giveIndexForPosition(positionCaseEvalué)))
                                                {
                                                    CasesAdversesManger newCase = new CasesAdversesManger(piece.Position, positionCaseEvalué);
                                                    listeCasesAManger.Add(newCase);
                                                }

                                            }
                                        }
                                    }

                                }

                                foreach(Pieces piece in piecesAlli)
                                {
                                    for (int i = 0; i < tabCoord.Length; i++)
                                    {
                                        if (echiquier.valide(giveIndexForPosition(piece.Position), giveIndexForPosition(PosForCoord(tabCoord[i]))))
                                        {
                                            CasesAdversesManger deplacement = new CasesAdversesManger(piece.Position, PosForCoord(tabCoord[i]));
                                            deplacement_poss.Add(deplacement);
                                            //Console.WriteLine(piece.Position + " " + PosForCoord(tabCoord[i]));
                                        }

                                    }
                                 }

                                CasesAdversesManger Deplacement_ennemi;

                                echiquier.setTrait();
                                foreach(CasesAdversesManger cas in deplacement_poss)
                                {
                                    //Console.WriteLine(cas.CoordAlliee1 + " " + cas.CoordEnnemies1);
                                    foreach (Pieces piece in piecesEnnemies)
                                    {
                                        //Console.WriteLine(piece.Coordonnees);
                                        for (int i = 0; i < tabCoord.Length; i++)
                                        { 
                                            //Console.WriteLine("départ: "+ piece.Position + " destination: " +PosForCoord(tabCoord[i]) + echiquier.valide(giveIndexForPosition(piece.Position), giveIndexForPosition(PosForCoord(tabCoord[i]))));
                                            if (echiquier.valide(giveIndexForPosition(piece.Position), giveIndexForPosition(PosForCoord(tabCoord[i]))))
                                            {
                                                Deplacement_ennemi = new CasesAdversesManger(piece.Position, PosForCoord(tabCoord[i]));
                                                min.Add(cas.CoordAlliee1, Deplacement_ennemi);
                                               Console.WriteLine("Blanc: " + cas.CoordAlliee1 + " DP Blanc: " + cas.CoordEnnemies1 + " Noir: " + Deplacement_ennemi.CoordAlliee1 + " DP Noir: " +Deplacement_ennemi.CoordEnnemies1);
                                            }
                                        }
                                    }
                                }
                                echiquier.setTrait();








                                /////////////////////////////////////////// Notre joueur joue en Random /////////////////////////////////////////////////
                                //Random rnd = new Random();
                                //coord[0] = mesPieces[rnd.Next(mesPieces.Count)];
                                //coord[0] = "b7";
                                //coord[1] = "b8";
                                //coord[1] = tabCoord[rnd.Next(reste.Count)];
                                //coord[2] = "P";
                                foreach (CasesAdversesManger newCase in listeCasesAManger)
                                {
                                    Console.WriteLine(newCase.ToString());
                                }

                                /********************************************************************************************************/
                                /********************************************************************************************************/
                                /********************************************************************************************************/

                                using (var accessor = mmf2.CreateViewAccessor())
                                {
                                    value = coord[0];
                                    for (int i = 1; i < coord.Length; i++)
                                    {
                                        value += "," + coord[i];
                                    }
                                    byte[] Buffer = ASCIIEncoding.ASCII.GetBytes(value);
                                    accessor.Write(0, (ushort)Buffer.Length);
                                    accessor.WriteArray(0 + 2, Buffer, 0, Buffer.Length);
                                }
                            }
                            mutexAI1.ReleaseMutex();
                            mutexStartAI1.ReleaseMutex();
                        }
                    }
                }

                
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Memory-mapped file does not exist. Run Process A first.");
                Console.ReadLine();
               
            }


            


        }

        // Création d'une fonction permettant de trouver la position pour une coordonnée donnée. 
        // Cela nous permet de calculer ensuite les distances avec les fonctions de l'echiquier.

        private static int PosForCoord(String coordonnée)
        {
           String[]  tabCoord = new string[] {  "a8","b8","c8","d8","e8","f8","g8","h8",
                                       "a7","b7","c7","d7","e7","f7","g7","h7",
                                       "a6","b6","c6","d6","e6","f6","g6","h6",
                                       "a5","b5","c5","d5","e5","f5","g5","h5",
                                       "a4","b4","c4","d4","e4","f4","g4","h4",
                                       "a3","b3","c3","d3","e3","f3","g3","h3",
                                       "a2","b2","c2","d2","e2","f2","g2","h2",
                                       "a1","b1","c1","d1","e1","f1","g1","h1" };


            int[] tabPos = new int[] {  21, 22, 23, 24, 25, 26, 27, 28,
                                  31, 32, 33, 34, 35, 36, 37, 38,
                                  41, 42, 43, 44, 45, 46, 47, 48,
                                  51, 52, 53, 54, 55, 56, 57, 58,
                                  61, 62, 63, 64, 65, 66, 67, 68,
                                  71, 72, 73, 74, 75, 76, 77, 78,
                                  81, 82, 83, 84, 85, 86, 87, 88,
                                  91, 92, 93, 94, 95, 96, 97, 98 };

            int indice = -100; 

            for(int i=0; i<tabCoord.Length; i++)
            {
                if( tabCoord[i] == coordonnée)
                {
                    indice = i;                 // On récupère l'indice pour récupérer la position avec ce même indice car il y a le même nombre d'élément. 64 éléments.
                }
            }

            
            return tabPos[indice]; // Retourne -100 si erreur et la position sinon. 


        }


        private static Boolean inTheTerrain(int position)   // Fonction permettant de savoir si la case se situe dans le terrain ou non.
        {
            Echiquier echiquier = Echiquier.Instance(); // On récupère une instance de l'échiquier. 
            Boolean retour = false; // Dans le doute, on met faux au départ.

            //Permet de calculer les déplacement en évitant les sorties de tableau
           int[] tab120 = new int[] {  - 1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                                  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                                  -1,  0,  1,  2,  3,  4,  5,  6,  7, -1,
                                  -1,  8,  9, 10, 11, 12, 13, 14, 15, -1,
                                  -1, 16, 17, 18, 19, 20, 21, 22, 23, -1,
                                  -1, 24, 25, 26, 27, 28, 29, 30, 31, -1,
                                  -1, 32, 33, 34, 35, 36, 37, 38, 39, -1,
                                  -1, 40, 41, 42, 43, 44, 45, 46, 47, -1,
                                  -1, 48, 49, 50, 51, 52, 53, 54, 55, -1,
                                  -1, 56, 57, 58, 59, 60, 61, 62, 63, -1,
                                  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                                  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

            if(tab120[position] != -1)
            {
                retour = true;
            }

            return retour; 
        }
        // int position, int arrivé, int valeur, string pièce 
        public String CompareMinMax(CoupPossible[] coup)
        {


            private static Boolean isPion(Pieces piece)
           // Dictionary<string, int> MinValueDictionary = new Dictionary<string, int>(); 
            Dictionary<string, CoupPossible> CoupPossibleDictionary = new Dictionary<string, CoupPossible>();

            String[] tabCoord = new string[] {  "a8","b8","c8","d8","e8","f8","g8","h8",
                                       "a7","b7","c7","d7","e7","f7","g7","h7",
                                       "a6","b6","c6","d6","e6","f6","g6","h6",
                                       "a5","b5","c5","d5","e5","f5","g5","h5",
                                       "a4","b4","c4","d4","e4","f4","g4","h4",
                                       "a3","b3","c3","d3","e3","f3","g3","h3",
                                       "a2","b2","c2","d2","e2","f2","g2","h2",
                                       "a1","b1","c1","d1","e1","f1","g1","h1" };

            //initialisation du dictionary on prend une valeur impossible a atteindre pour ignorer les case "vide" 
            for (int i = 0; i < tabCoord.Length; i++)
            {
                CoupPossibleDictionary.Add(tabCoord[i], new CoupPossible("", "", 1000, ""));
            }

            foreach (CoupPossible valueCoupPossible in coup)
            {
                if (valueCoupPossible.Valeur < CoupPossibleDictionary[valueCoupPossible.Position].Valeur) //cherche le min de toutes les propositions 
                {
                    CoupPossibleDictionary[valueCoupPossible.Position] = valueCoupPossible;
                }
            }

            int max = -1000;
            string pos = "";
            for (int i = 0; i < tabCoord.Length; i++) //trouve le maximum de toutes les solutions (meme départ)  
            {
                if (CoupPossibleDictionary[tabCoord[i]].Valeur != 1000 && CoupPossibleDictionary[tabCoord[i]].Valeur > max)
                {
                    max = CoupPossibleDictionary[tabCoord[i]].Valeur;
                    pos = tabCoord[i]; //pos de départ on veut celle d'arrivé 
                }
            }

            //retourne une valeur parmis toute celle jouable (toujours le premier min) 
            string arr = CoupPossibleDictionary[pos].Arrive;


            /***********recupere toutes les arrivés **************** si on a besoin des coups ennemis 
            List<CoupPossible> ArrList = new List<CoupPossible>(); 
            foreach (CoupPossible coupPossible in coup) 
            { 
                if (coupPossible.Position == pos) //cherche le min de toutes les propositions 
                { 
                    ArrList.Add(coupPossible); 
                } 
            } 
         
            */
            return arr;
        }

        private static Boolean isPion (Pieces piece)

       

        {
            if(piece.Valeurs != 1 && piece.Valeurs != -1)
            {
                return false;
            }
            else { return true; }
        }


        private static int giveIndexForPosition(int position)
        {
            int index = -1100;

            //Tableau permettant de calculer les déplacements avec les vecteurs de déplacement des pièces
           int[] tabPos = new int[] {  21, 22, 23, 24, 25, 26, 27, 28,
                                  31, 32, 33, 34, 35, 36, 37, 38,
                                  41, 42, 43, 44, 45, 46, 47, 48,
                                  51, 52, 53, 54, 55, 56, 57, 58,
                                  61, 62, 63, 64, 65, 66, 67, 68,
                                  71, 72, 73, 74, 75, 76, 77, 78,
                                  81, 82, 83, 84, 85, 86, 87, 88,
                                  91, 92, 93, 94, 95, 96, 97, 98 };

            for (int i =0; i<tabPos.Length; i++)
            {
                if(tabPos[i] == position)
                {
                    index = i;
                }
            }



            return index;
        }

}


 
}
