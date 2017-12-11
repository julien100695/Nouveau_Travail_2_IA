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
                                int dep=1000;
                                int arr=1000;

                                echiquier.setTrait();
                                for(int i = 0; i < tabVal.Length; i++)
                                {
                                    if(echiquier.getTabVal(i) != tabVal[i])
                                    {
                                        if (echiquier.getTabVal(i) == -10 || tabVal[i] == -10);
                                        else if (tabVal[i] == 0)
                                            dep = i;
                                        
                                        else
                                        {
                                            arr = i;
                                            if (dep != 1000 && arr != 1000);
                                                //Console.WriteLine("ennemi bouge de :" + tabCoord[dep] + " à " + tabCoord[arr] + "\n");
                                        }
                                    }
                                    //Console.WriteLine("tab1: " + echiquier.getTabVal(i) + " tab2: " + tabVal[i] + " " + tabCoord[i]);
                                }
                                if(dep!=1000 && arr!=1000)
                                echiquier.deplacement(dep, arr);
                                echiquier.setTrait();
                                //Console.WriteLine(tabVal[0] + " " + tabVal[1] + " " + tabVal[2]);



                                int trait = echiquier.getTrait();
                                int[] mouvementPion = new int[] { -11, -9 }; // Vecteurs de mouvement du pion.

                                // Création de la liste des pièces adverse que l'on peut manger
                                List<CasesAdversesManger> listeCasesAManger = new List<CasesAdversesManger>();

                                // On récupère la liste de nos pièces.
                                List<String> mesPieces = new List<String>();
                                List<Pieces> piecesAlli = new List<Pieces>(); // Creation de la liste de nos pièces.
                                List<Pieces> piecesEnnemies = new List<Pieces>(); // Creation de la liste des pièces ennemies.
                                var min = new List<Tuple<int, int, CasesAdversesManger>>(); // Liste comportant les tuples(casedépartallié,déplacement_ennemi)
                                int poids = new int();
                                CoupPossible[] coups;

                                List<CasesAdversesManger> deplacement_poss = new List<CasesAdversesManger>();

                                for (int i = 0; i < tabVal.Length; i++)
                                {
                                    if (tabVal[i] > 0) // Si ce sont des alliés
                                    {
                                        int position = PosForCoord(tabCoord[i]); // On récupère la position
                                        // mesPieces.Add(tabCoord[i]); Si on veut les coordonnées de nos pièces, on peut rajouter cette ligne.
                                        if (tabVal[i] == 1) //Pion
                                            poids = 10;
                                        else if (tabVal[i] == 21 || tabVal[i] == 22) //tour
                                            poids = 50;
                                        else if (tabVal[i] == 4) //fou
                                            poids = 33;
                                        else if (tabVal[i] == 31 || tabVal[i] == 32) //cavalier
                                            poids = 32;
                                        else if (tabVal[i] == 5) //dame
                                            poids = 90;
                                        else if (tabVal[i] == 6) //roi
                                            poids = 900;
                                        Pieces piece = new Pieces(tabCoord[i], tabVal[i], position, poids);
                                        piecesAlli.Add(piece); // On ajoute à la liste de pieces alliées.
                                        //Console.WriteLine(piece.Coordonnees + " " + piece.Poids);
                                    }
                                    else if (tabVal[i] <= -1) // Si c'est une piece ennemie.
                                    {
                                        int position = PosForCoord(tabCoord[i]); // On récupère la position.
                                        if (tabVal[i] == -1) //Pion
                                            poids = -10;
                                        else if (tabVal[i] == -21 || tabVal[i] == -22) //tour
                                            poids = -50;
                                        else if (tabVal[i] == -4) //fou
                                            poids = -33;
                                        else if (tabVal[i] == -31 || tabVal[i] == -32) //cavalier
                                            poids = -32;
                                        else if (tabVal[i] == -5) //dame
                                            poids = -90;
                                        else if (tabVal[i] == -6) //roi
                                            poids = -900;
                                        Pieces piece = new Pieces(tabCoord[i], tabVal[i], position, poids);
                                        piecesEnnemies.Add(piece); // On ajoute à la liste de pieces ennemies.
                                        //Console.WriteLine(piece.Coordonnees + " " + piece.Poids);
                                    }
                                }

                                /* On récupère la liste de tout ce qu'il reste.
                                //List<String> reste = new List<String>(); // Cases vides.
                                for (int i = 0; i < tabVal.Length; i++)
                                {
                                    //if (tabVal[i] <= 0) reste.Add(tabCoord[i]);
                                        Pieces piece = new Pieces(tabCoord[i], tabVal[i], position, poids);
                                        piecesEnnemies.Add(piece); // On ajoute à la liste de pieces ennemies.
                                    }
                                }*/

                                /* Maintenant on remplit la liste des cases contenant des pièces que l'on peut manger 
                                et celles où l'on peut aller sans rencontrer d'obstacle. */

                                /*foreach (Pieces piece in piecesAlli)
                                {
                                    //Console.WriteLine(piece.Position);
                                }
                                foreach (Pieces piece in piecesAlli)
                                {
                                    //Quelque soit la pièce, si le coup est valide sur une piece ennemie, alors on peut la manger
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

                                }*/
                                int tabtmparr = new int();
                                int somme = 0;
                                foreach(Pieces piece in piecesAlli)
                                {
                                    for (int i = tabVal.Length-1; i >= 0; i--)
                                    {
                                        if (echiquier.valide(giveIndexForPosition(piece.Position), giveIndexForPosition(PosForCoord(tabCoord[i]))))
                                        {
                                            somme = 0;
                                            tabtmparr = tabVal[i]; //on stocke temporairement la valeur de la case d'arrivée
                                            tabVal[i] = tabVal[Array.IndexOf(tabCoord, piece.Coordonnees)]; //arrivée prend valeur de piece déplacée
                                            tabVal[Array.IndexOf(tabCoord, piece.Coordonnees)] = 0; //départ prend valeur nulle

                                            for (int j = tabVal.Length-1; j>=0; j--)
                                            {
                                                if (tabVal[j] == 1) //Pion
                                                    somme += 10;
                                                else if (tabVal[j] == 21 || tabVal[j] == 22) //tour
                                                    somme += 50;
                                                else if (tabVal[j] == 4) //fou
                                                    somme += 33;
                                                else if (tabVal[j] == 31 || tabVal[j] == 32) //cavalier
                                                    somme += 32;
                                                else if (tabVal[j] == 5) //dame
                                                    somme += 90;
                                                else if (tabVal[j] == 6) //roi
                                                    somme += 900;
                                                else if (tabVal[j] == -1) //Pion
                                                    somme += -10;
                                                else if (tabVal[j] == -21 || tabVal[j] == -22) //tour
                                                     somme += -50;
                                                else if (tabVal[j] == -4) //fou
                                                    somme += -33;
                                                else if (tabVal[j] == -31 || tabVal[j] == -32) //cavalier
                                                    somme += -32;
                                                else if (tabVal[j] == -5) //dame
                                                    somme += -90;
                                                else if (tabVal[j] == -6) //roi
                                                    somme += -900;
                                            }
                                            Console.WriteLine("somme = " + somme + " dp: " + piece.Coordonnees + " arr: " +tabCoord[i]);
                                            CasesAdversesManger deplacement = new CasesAdversesManger(piece.Position, PosForCoord(tabCoord[i]), somme);
                                            deplacement_poss.Add(deplacement);
                                            tabVal[Array.IndexOf(tabCoord, piece.Coordonnees)] = tabVal[i]; //case de départ reprend sa valeur
                                            tabVal[i] = tabtmparr; //case d'arrivée reprend sa valeur
                                        }

                                    }     
                                 }
                                //Console.WriteLine("\n");

                                CasesAdversesManger Deplacement_ennemi;

                                int valeur_ar = new int();
                                //int valeur_dp = new int();
                                echiquier.setTrait();
                                foreach(CasesAdversesManger cas in deplacement_poss)
                                {
                                    tabtmparr = tabVal[Array.IndexOf(tabCoord, CoordForPos(cas.CoordEnnemies1))]; // on stocke la valeur de la case d'arrivée
                                    tabVal[Array.IndexOf(tabCoord, CoordForPos(cas.CoordEnnemies1))] = tabVal[Array.IndexOf(tabCoord, CoordForPos(cas.CoordAlliee1))]; // on considère que notre pièce à fait son mouvement
                                    tabVal[Array.IndexOf(tabCoord, CoordForPos(cas.CoordAlliee1))] = 0; // la case de départ est donc vide

                                    echiquier.enregistrerCoup(Array.IndexOf(tabCoord, CoordForPos(cas.CoordAlliee1)), Array.IndexOf(tabCoord, CoordForPos(cas.CoordEnnemies1)));
                                    echiquier.deplacement(Array.IndexOf(tabCoord, CoordForPos(cas.CoordAlliee1)), Array.IndexOf(tabCoord, CoordForPos(cas.CoordEnnemies1)));

                                    foreach (Pieces piece in piecesEnnemies)
                                    {
                                        //Console.WriteLine(piece.Coordonnees);
                                        for (int i = tabCoord.Length-1; i >= 0; i--)
                                        {
                                            somme = 0;
                                            //Console.WriteLine("départ: "+ piece.Position + " destination: " +PosForCoord(tabCoord[i]) + echiquier.valide(giveIndexForPosition(piece.Position), giveIndexForPosition(PosForCoord(tabCoord[i]))));
                                            if (echiquier.valide(giveIndexForPosition(piece.Position), giveIndexForPosition(PosForCoord(tabCoord[i]))))
                                            {
                                                valeur_ar = tabVal[i]; //stocke arrivée
                                                tabVal[i] = tabVal[Array.IndexOf(tabCoord, piece.Coordonnees)]; //arrivée = départ
                                                tabVal[Array.IndexOf(tabCoord, piece.Coordonnees)] = 0; // départ = vide


                                                for (int j = tabVal.Length - 1; j >= 0; j--)
                                                {
                                                    if (tabVal[j] == 1) //Pion
                                                        somme += 10;
                                                    else if (tabVal[j] == 21 || tabVal[j] == 22) //tour
                                                        somme += 50;
                                                    else if (tabVal[j] == 4) //fou
                                                        somme += 33;
                                                    else if (tabVal[j] == 31 || tabVal[j] == 32) //cavalier
                                                        somme += 32;
                                                    else if (tabVal[j] == 5) //dame
                                                        somme += 90;
                                                    else if (tabVal[j] == 6) //roi
                                                        somme += 900;
                                                    else if (tabVal[j] == -1) //Pion
                                                        somme += -10;
                                                    else if (tabVal[j] == -21 || tabVal[j] == -22) //tour
                                                        somme += -50;
                                                    else if (tabVal[j] == -4) //fou
                                                        somme += -33;
                                                    else if (tabVal[j] == -31 || tabVal[j] == -32) //cavalier
                                                        somme += -32;
                                                    else if (tabVal[j] == -5) //dame
                                                        somme += -90;
                                                    else if (tabVal[j] == -6) //roi
                                                        somme += -900;
                                                }

                                                Deplacement_ennemi = new CasesAdversesManger(piece.Position, PosForCoord(tabCoord[i]), somme);
                                                min.Add(cas.CoordAlliee1, cas.CoordEnnemies1, Deplacement_ennemi);
                                                if(somme!=0)
                                                Console.WriteLine("somme = " + somme + " dp1: " + CoordForPos(cas.CoordAlliee1) + " arr1: " + CoordForPos(cas.CoordEnnemies1) + "\n    dp2: " + piece.Coordonnees + " arr2: " + tabCoord[i] + "\n");

                                                tabVal[Array.IndexOf(tabCoord, piece.Coordonnees)] = tabVal[i]; //départ = départ
                                                tabVal[i] = valeur_ar; //arrivée = arrivée
                                                

                                               //Console.WriteLine ("Poids coup" + Deplacement_ennemi.PoidsAlliee);
                                            }
                                        }
                                    }
                                    tabVal[Array.IndexOf(tabCoord, CoordForPos(cas.CoordAlliee1))] = tabVal[Array.IndexOf(tabCoord, CoordForPos(cas.CoordEnnemies1))]; //redonne les valeurs de départ
                                    tabVal[Array.IndexOf(tabCoord, CoordForPos(cas.CoordEnnemies1))] = tabtmparr; //redonne les valeurs d'arrivée
                                    echiquier.annulerCoup(Array.IndexOf(tabCoord, CoordForPos(cas.CoordAlliee1)), Array.IndexOf(tabCoord, CoordForPos(cas.CoordEnnemies1)));
                                }
                             echiquier.setTrait();

                             coups = lien(min);


                             CoupPossible s = CompareMinMax(coups);
                             echiquier.deplacement(Array.IndexOf(tabCoord, s.Position), Array.IndexOf(tabCoord, s.Arrive));
                             Console.WriteLine("départ: " + s.Position + " arrivée: " + s.Arrive + "valeur: " + s.Valeur + "\n");













                                /////////////////////////////////////////// Notre joueur joue en Random /////////////////////////////////////////////////
                                //Random rnd = new Random();
                                coord[0] = s.Position;
                                //coord[0] = "b7";
                                coord[1] = s.Arrive;
                                //coord[1] = tabCoord[rnd.Next(reste.Count)];
                                coord[2] = "D";


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

        private static string CoordForPos(int position)
        {
            String[] tabCoord = new string[] {  "a8","b8","c8","d8","e8","f8","g8","h8",
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
             int indice = Array.IndexOf(tabPos, position);
            return tabCoord[indice];
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
        private static CoupPossible CompareMinMax(CoupPossible[] coup)
        {


           // Dictionary<string, int> MinValueDictionary = new Dictionary<string, int>();
           Dictionary<String, CoupPossible> dico = new Dictionary<string, CoupPossible>();
            Dictionary<String,CoupPossible> CoupPossibleDictionary = new Dictionary<String, CoupPossible>();
            Dictionary<Tuple<String, String>, CoupPossible> poss = new Dictionary<Tuple<String, String>, CoupPossible>();

            foreach (CoupPossible val in coup)
            {
                Tuple<String, String> tup = new Tuple<string, string>(val.Position, val.Arrive);
                if (!poss.ContainsKey(tup))
                poss.Add(new Tuple<String, String>(val.Position, val.Arrive), new CoupPossible("", "", 1000, ""));
            }
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

            foreach (CoupPossible valueCoupPossible in coup) {  
                /* if (valueCoupPossible.Valeur < CoupPossibleDictionary[valueCoupPossible.Position].Valeur) //cherche le min de toutes les propositions 
                 {
                     CoupPossibleDictionary[valueCoupPossible.Position] = valueCoupPossible;
                     //Console.WriteLine(valueCoupPossible.Position + " " + valueCoupPossible.Arrive + " " + valueCoupPossible.Valeur);
                 }*/
                Tuple<String, String> tup = new Tuple<string, string>(valueCoupPossible.Position,valueCoupPossible.Arrive);
                if (valueCoupPossible.Valeur < poss[tup].Valeur)
                {
                    poss[tup] = valueCoupPossible;
                    Console.WriteLine("dp: " + poss[tup].Position + " ar: " + poss[tup].Arrive + " val:" + poss[tup].Valeur);
                }
            }
            Console.WriteLine("\n");
            int max = -1000;
            string pos = "";
            string arr = "";
            /*for (int i = 0; i < tabCoord.Length; i++) //trouve le maximum de toutes les solutions (meme départ)  
            {
                if (CoupPossibleDictionary[tabCoord[i]].Valeur != 1000 && CoupPossibleDictionary[tabCoord[i]].Valeur > max)
                {
                    max = CoupPossibleDictionary[tabCoord[i]].Valeur;
                    pos = tabCoord[i]; //pos de départ on veut celle d'arrivé
                    //Console.Write("max: " + CoupPossibleDictionary[tabCoord[i]].Valeur + " coord: " + tabCoord[i]);
                }
            }*/

            foreach (KeyValuePair<Tuple<String, String>, CoupPossible> po in poss)
            {

                if(po.Value.Valeur != 1000 && po.Value.Valeur > max)
                {
                    max = po.Value.Valeur;
                    pos = po.Key.Item1;
                    arr = po.Key.Item2;
                }

            }

            //retourne une valeur parmis toute celle jouable (toujours le premier min) 
            //CoupPossible arr = CoupPossibleDictionary[pos];
            CoupPossible ret = new CoupPossible(pos, arr, max, "");


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
            return ret;
        }

        private static Boolean isPion (Pieces piece)

       

        {
            if(piece.Valeurs != 1 && piece.Valeurs != -1)
            {
                return false;
            }
            else { return true; }
        }


        private static CoupPossible[] lien (List<Tuple<int, int, CasesAdversesManger>>  ennemi)
        {
            CoupPossible[] coups = new CoupPossible[ennemi.Count];
            int i = 0;
            foreach(Tuple<int, int, CasesAdversesManger> entree in ennemi)
            {
                string dp = CoordForPos(entree.Item1);
                string ar = CoordForPos(entree.Item2);
                int val = entree.Item3.PoidsAlliee;
                CoupPossible coup = new CoupPossible(dp, ar,val,"");
                coups[i] = coup;
                //Console.WriteLine(dp + " " + ar + " " + val);
                i++;
            }
            return coups;
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
