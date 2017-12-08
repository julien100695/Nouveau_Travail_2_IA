using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jeu_echec_stage
{
    public class Echiquier
    {
        private static Echiquier _me;

        private const int NB_CASE_LARGE = 8;
        private const int MAX_DEPLACEMENT = 7; //Les pièces se déplacent au maximum de 7 cases par coup

        private const int PP = 10; //pion passant
        private const int P = 1; //pion
        private const int TG = 21; //tour gauche (different pour le roque)
        private const int TD = 22; //tour droite
        private const int CG = 31; //cavalier gauche (différents pour l'image)
        private const int CD = 32; //cavalier droit
        private const int F = 4; //fou
        private const int D = 5; //dame
        private const int R = 6; //roi

        //Vecteur de déplacement
        private int[] MP;
        private int[] MF;
        private int[] MT;
        private int[] MC;
        private int[] MR;
        private int[] TOTAL;

        //Tableaux représentant l'echiquier
        private String[] tabCoord;
        private int[] tabVal;
        private int[] tabPos;
        private int[] tab120;

        private int trait, cpt, casePassant, saveDep, saveArr;
        private bool passant, echec;
        private bool grandRoqueN, petitRoqueN, grandRoqueB, petitRoqueB;

        public static Echiquier Instance()
        {
            if(_me == null)
            {
                _me = new Echiquier();
            }

            return _me;
        }

        //Constructeur
        private Echiquier()
        {
            trait = 1;
            cpt = casePassant = saveDep = saveArr = 0;
            passant = echec = false;
            grandRoqueN = petitRoqueN = grandRoqueB = petitRoqueB = true;

            //Vecteurs
            MP = new int[] { -10, -20, -11, -9 };
            MF = new int[] { 11, -11, -9, 9 };
            MT = new int[] { -1, 1, -10, 10 };
            MC = new int[] { 12, 21, 19, 8, -12, -21, -19, -8 };
            MR = new int[] { -1, 1, -10, 10, -11, 11, 9, -9 };
            TOTAL = new int[] { -1, 1, -10, 10, -11, 11, 9, -9, 12, 21, 19, 8, -12, -21, -19, -8 };

            //Coordonnées des cases
            tabCoord = new string[] {  "a8","b8","c8","d8","e8","f8","g8","h8",
                                       "a7","b7","c7","d7","e7","f7","g7","h7",
                                       "a6","b6","c6","d6","e6","f6","g6","h6",
                                       "a5","b5","c5","d5","e5","f5","g5","h5",
                                       "a4","b4","c4","d4","e4","f4","g4","h4",
                                       "a3","b3","c3","d3","e3","f3","g3","h3",
                                       "a2","b2","c2","d2","e2","f2","g2","h2",
                                       "a1","b1","c1","d1","e1","f1","g1","h1" };

            //Valeur de chaque cases en fonction de la pièce qui est dessus
            tabVal = new int[] {  -TG, -CG, -F, -D, -R, -F, -CD, -TD,
                                  -P, -P, -P, -P, -P, -P, -P, -P,
                                  0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0,
                                  P, P, P, P, P, P, P, P,
                                  TG, CG, F, D, R, F, CD, TD };

            /*tabVal = new int[] {  0, 0, 0, 0, -R, 0, 0, -TD,
                                  0, P, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0,
                                  D, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0,
                                  TD, 0, 0, 0, 0, 0, 0, 0,
                                  TG, 0, 0, 0, R, 0, 0, 0};*/

            //Tableau permettant de calculer les déplacements avec les vecteurs de déplacement des pièces
            tabPos = new int[] {  21, 22, 23, 24, 25, 26, 27, 28,
                                  31, 32, 33, 34, 35, 36, 37, 38,
                                  41, 42, 43, 44, 45, 46, 47, 48,
                                  51, 52, 53, 54, 55, 56, 57, 58,
                                  61, 62, 63, 64, 65, 66, 67, 68,
                                  71, 72, 73, 74, 75, 76, 77, 78,
                                  81, 82, 83, 84, 85, 86, 87, 88,
                                  91, 92, 93, 94, 95, 96, 97, 98 };

            //Permet de calculer les déplacement en évitant les sorties de tableau
            tab120 = new int[] {  - 1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
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
        }

        //Getters et setters
        public int getTrait() { return trait; }
        public bool getEchec() { return echec; }
        public int getSaveArr() { return saveArr; }
        public int getTabVal(int ind) { return tabVal[ind]; }

        public void setTrait() { trait = trait * (-1); }
        public void setPRN() { petitRoqueN = false; } 
        public void setGRN() { grandRoqueN = false; }
        public void setPRB() { petitRoqueB = false; } 
        public void setGRB() { grandRoqueB = false; }

        //Réinitialisation des valeurs pour rejouer
        public void reinit()
        {
            trait = 1;
            cpt = casePassant = saveDep = saveArr = 0;
            passant = echec = false;
            grandRoqueN = petitRoqueN = grandRoqueB = petitRoqueB = true;

            tabVal = new int[] {  -TG, -CG, -F, -D, -R, -F, -CD, -TD,
                                  -P, -P, -P, -P, -P, -P, -P, -P,
                                  0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0,
                                  P, P, P, P, P, P, P, P,
                                  TG, CG, F, D, R, F, CD, TD };
        }

        //Savoir si le coup est permis
        public bool valide(int dep, int arr)
        {
            if (tabVal[dep] == 0) return false; //si on demarre si une case vide
            if (tabVal[dep] * trait < 0) return false; //si la piece avec lequel on tente de jouer n'est pas le notre
            if (tabVal[arr] * trait > 0) return false; //si la case est occupé par une de nos pieces

            switch (tabVal[dep])
            {
                /*********************************************************************/
                /************************ DEPLACEMENT DU PION ************************/
                /*********************************************************************/
                case -P:
                case P:
                    //Si la case d'arrivée n'est pas occupé
                    if (tabVal[arr] == 0)
                    {
                        //Si le pion est sur sa case d'origine donc il ne s'est pas encore déplacé il peut avancer d'une ou deux cases
                        if ((tabVal[dep] == -P && (tabCoord[dep] == "a7" || tabCoord[dep] == "b7" || tabCoord[dep] == "c7" || tabCoord[dep] == "d7" ||
                            tabCoord[dep] == "e7" || tabCoord[dep] == "f7" || tabCoord[dep] == "g7" || tabCoord[dep] == "h7")) ||
                            tabVal[dep] == P && (tabCoord[dep] == "a2" || tabCoord[dep] == "b2" || tabCoord[dep] == "c2" || tabCoord[dep] == "d2" ||
                            tabCoord[dep] == "e2" || tabCoord[dep] == "f2" || tabCoord[dep] == "g2" || tabCoord[dep] == "h2"))
                        {
                            for (int i = 0; i < 2; i++) //Pour les déplacement d'une ou deux cases
                            {
                                int vecteur = tabPos[dep] + (MP[i] * trait); //On calcule les postions possibles
                                if (vecteur == tabPos[arr]) //Si on a trouvé la case demandé
                                {
                                    if (i == 0) //Pour un déplacement d'une case
                                    {
                                        if (passant) enPassant(casePassant); //on arrête la prise en passant puisqu'elle n'est pas utilisée
                                        return true;
                                    }
                                    else //S'il avance de deux cases on vérifie qu'il n'y a pas d'obstacle
                                    {
                                        vecteur = tabPos[dep] + (MP[0] * trait);
                                        if (tabVal[tab120[vecteur]] == 0)
                                        {
                                            passant = false; //On met le booléen à faux pour être sûr d'entrer dans la bonne boucle
                                            enPassant(tab120[vecteur]); //On active la prise en passant pour le tour suivant
                                            return true;
                                        }
                                        else return false;
                                    }
                                }
                            }
                        }
                        //Sinon il ne peut avancer que d'une case
                        else
                        {
                            if (passant) enPassant(casePassant);
                            int vecteur = tabPos[dep] + (MP[0] * trait);
                            if (vecteur == tabPos[arr]) return true;
                        }
                    }
                    //Si la case d'arrivée est occupé le déplacement doit être en diagonale
                    else
                    {
                        for (int i = 2; i < MP.Length; i++)
                        {
                            int vecteur = tabPos[dep] + (MP[i] * trait);
                            if (vecteur == tabPos[arr])
                            {
                                if (passant && (tabVal[arr] == PP || tabVal[arr] == -PP)) //Si la position d'arrivée correspond à une prise en passant
                                {
                                    passant = false;
                                    tabVal[arr + (NB_CASE_LARGE * trait)] = 0; //On met la valeur à 0 dans le tableau de valeur pour la position du pion car il va être mangé
                                }

                                if (passant) enPassant(casePassant);
                                return true;
                            }
                        }
                    }
                    return false; //Si aucun vecteur ne corespond on retourne faux


                /********************************************************************/
                /************************ DEPLACEMENT DU FOU ************************/
                /********************************************************************/
                case -F:
                case F:
                    for (int i = 0; i < MF.Length; i++) //Pour chaque vecteur de déplacement
                    {
                        int vecteur = tabPos[dep]; //On prend la valeur de la position de depart
                        int j = 1; //Compteur
                        while (tab120[vecteur] != -1 && vecteur != tabPos[arr] && j <= MAX_DEPLACEMENT) //Tant qu'on ne déborde pas du tableau et que l'on n'est pas à la position d'arrivée
                        {
                            vecteur = tabPos[dep] + (MF[i] * j); //Position de départ + vecteur * j pour avancer de case
                            if (vecteur == tabPos[arr]) //Si on a trouvé la case d'arrivée
                            {
                                //On refait le chemin pour vérifier qu'il n'y a pas d'obstacle
                                j = 1;
                                int vecteur2 = tabPos[dep] + (MF[i] * j);
                                while (vecteur2 != tabPos[arr] && (tabVal[tab120[vecteur2]] == 0 || tabVal[tab120[vecteur2]] == PP || tabVal[tab120[vecteur2]] == -PP))
                                {
                                    j++;
                                    vecteur2 = tabPos[dep] + (MF[i] * j);
                                }
                                //Si on a un obstacle à part la valeur du pion passant puisqu'elle sera effacée si le coup est valide
                                if (tabVal[tab120[vecteur2]] != 0 && tabVal[tab120[vecteur2]] != PP && tabVal[tab120[vecteur2]] != -PP && vecteur2 != tabPos[arr]) return false;
                                else
                                {
                                    if (passant) enPassant(casePassant);
                                    return true;
                                }
                            }
                            j++;
                        }
                    }
                    return false;


                /************************************************************************/
                /************************ DEPLACEMENT DE LA TOUR ************************/
                /************************************************************************/
                case -TG:
                case -TD:
                case TG:
                case TD:
                    for (int i = 0; i < MT.Length; i++)
                    {
                        int vecteur = tabPos[dep];
                        int j = 1;
                        while (tab120[vecteur] != -1 && vecteur != tabPos[arr] && j <= MAX_DEPLACEMENT)
                        {
                            vecteur = tabPos[dep] + (MT[i] * j);
                            if (vecteur == tabPos[arr])
                            {
                                j = 1;
                                int vecteur2 = tabPos[dep] + (MT[i] * j);
                                while (vecteur2 != tabPos[arr] && (tabVal[tab120[vecteur2]] == 0 || tabVal[tab120[vecteur2]] == PP || tabVal[tab120[vecteur2]] == -PP))
                                {
                                    j++;
                                    vecteur2 = tabPos[dep] + (MT[i] * j);
                                }
                                if (tabVal[tab120[vecteur2]] != 0 && tabVal[tab120[vecteur2]] != PP && tabVal[tab120[vecteur2]] != -PP && vecteur2 != tabPos[arr]) return false;
                                else
                                {
                                    if (passant) enPassant(casePassant);
                                    return true;
                                }
                            }
                            j++;
                        }
                    }
                    return false;


                /*************************************************************************/
                /************************ DEPLACEMENT DU CAVALIER ************************/
                /*************************************************************************/

                case -CG:
                case -CD:
                case CG:
                case CD:
                    for (int i = 0; i < MC.Length; i++)
                    {
                        int vecteur = tabPos[dep] + MC[i];
                        if (vecteur == tabPos[arr])
                        {
                            if (passant) enPassant(casePassant);
                            return true; //Il n'y a pas d'obstacle pour le cavalier
                        }
                    }
                    return false;


                /************************************************************************/
                /************************ DEPLACEMENT DE LA DAME ************************/
                /************************************************************************/
                case -D:
                case D:
                    for (int i = 0; i < MR.Length; i++) //On utilise le tableau de déplacement du roi car ils ont les mêmes vecteurs
                    {
                        int vecteur = tabPos[dep];
                        int j = 1;
                        while (tab120[vecteur] != -1 && vecteur != tabPos[arr] && j <= MAX_DEPLACEMENT)
                        {
                            vecteur = tabPos[dep] + (MR[i] * j);
                            if (vecteur == tabPos[arr])
                            {
                                j = 1;
                                int vecteur2 = tabPos[dep] + (MR[i] * j);
                                while (vecteur2 != tabPos[arr] && (tabVal[tab120[vecteur2]] == 0 || tabVal[tab120[vecteur2]] == PP || tabVal[tab120[vecteur2]] == -PP))
                                {
                                    j++;
                                    vecteur2 = tabPos[dep] + (MR[i] * j);
                                }
                                if (tabVal[tab120[vecteur2]] != 0 && tabVal[tab120[vecteur2]] != PP && tabVal[tab120[vecteur2]] != -PP && vecteur2 != tabPos[arr]) return false;
                                else
                                {
                                    if (passant) enPassant(casePassant);
                                    return true;
                                }
                            }
                            j++;
                        }
                    }
                    return false;


                /********************************************************************/
                /************************ DEPLACEMENT DU ROI ************************/
                /********************************************************************/
                case -R:
                case R:
                    for (int i = 0; i < MR.Length; i++)
                    {
                        int vecteur = tabPos[dep] + MR[i];
                        if (vecteur == tabPos[arr])
                        {
                            if (passant) enPassant(casePassant);
                            return true;
                        }
                    }
                    return false;

            }
            return false;
        }

        //Retourne l'indice de la case dans le tableau
        public int coord(String coord)
        {
            for (int i = 0; i < tabCoord.Length; i++)
            {
                if (tabCoord[i] == coord) return i;
            }
            return -1;
        }

        //Enregistre les valeur des cases avant un déplacement
        public void enregistrerCoup(int dep, int arr)
        {
            saveDep = tabVal[dep];
            saveArr = tabVal[arr];
        }

        //Annule le dernier déplacement
        public void annulerCoup(int dep, int arr)
        {
            tabVal[dep] = saveDep;
            tabVal[arr] = saveArr;
        }

        //Met à jour le tableau de valeur lors d'un déplacement
        public void deplacement(int dep, int arr)
        {
            tabVal[arr] = tabVal[dep];
            tabVal[dep] = 0;
        }

        //Modifier la valeur d'une case pour une promotion
        public String setTabVal(int ind, String piece)
        {
            int val = 0;
            String prom;
            switch (piece)
            {
                case "T": val = TG * trait; prom = "Le pion est promu en TOUR";  break;
                case "C": val = CG * trait; prom = "Le pion est promu en CAVALIER";  break;
                case "F": val = F * trait; prom = "Le pion est promu en FOU"; break;
                case "D": val = D * trait; prom = "Le pion est promu en DAME";  break;
                default: val = P * trait; prom = "Promotion incorrect"; break;

            }
            tabVal[ind] = val;
            return prom;
        }

        //Test de prise en passant
        public void enPassant(int caseP)
        {
            if (passant) //Si le booléen était à vrai au tour précédent on le met à faux pour ce tour
            {
                passant = false;
                tabVal[caseP] = 0; //On remet 0 dans la case de la prise
            }
            else
            { 
                passant = true;
                if (tabVal[casePassant] == PP || tabVal[casePassant] == -PP) tabVal[casePassant] = 0; //On remet l'ancienne case passante à 0
                casePassant = caseP;
                tabVal[casePassant] = PP * trait; //On met 10 ou -10 dans la case passante de ce tour
            }
        }

        //Test si le roi adverse est en echec
        public bool miseEnEchec()
        {
            int i = 0;
            cpt = 0;
            echec = false;
            while (tabVal[i] != R * (-trait) && i<tabVal.Length) i++; //On cherche la case du roi adverse

            //Si une pièce peut se déplacer jusqu'au roi adverse il y a echec
            for (int j = 0; j < tabVal.Length; j++)
            {
                if (tabVal[j] * trait > 0 && (tabVal[j] != PP || tabVal[j] != -PP))
                {
                    if (valide(j, i))
                    {
                        echec = true;
                        cpt++;
                    }
                }
            }
            return echec;
        }
        
        //Test de l'echec et mat
        public bool mat()
        {
            int ind = 0;
            while (tabVal[ind] != R * (-trait) && ind < tabVal.Length) ind++; //On cherche la case du roi adverse

            if (miseEnEchec())
            {
                //On regarde si le roi peut sortir de l'echec en se déplacant
                for (int i = 0; i < MR.Length; i++)
                {
                    int vecteur = tabPos[ind] + MR[i];
                    if (tab120[vecteur] != -1)
                    {
                        enregistrerCoup(ind, tab120[vecteur]);
                        deplacement(ind, tab120[vecteur]);
                        miseEnEchec();
                        annulerCoup(ind, tab120[vecteur]);
                        if (!echec) return false;
                    }
                }
                
                miseEnEchec();
                if (cpt == 1)
                {
                    int j = -1;
                    bool trouve = false;
                    while(j < tabVal.Length && !trouve)
                    {
                        j++;
                        if (tabVal[j] * trait > 0 && (tabVal[j] != PP || tabVal[j] != -PP))
                            if (valide(j, ind)) trouve = true;
                        
                    }

                    //On met le trait à l'equipe en echec et on regarde si une pièce peut manger la pièce menancante
                    setTrait();
                    for (int k = 0; k < tabVal.Length; k++)
                    {
                        if (tabVal[k] * trait > 0 && (tabVal[k] != PP || tabVal[k] != -PP))
                        {
                            if (valide(k, j))
                            {
                                setTrait();
                                return false; //Si oui, au moins un coup est possible, il n'y a pas mat
                            }
                        }
                    }
                    setTrait();

                    //Sinon on regarde si une piece peut couvrir le roi
                    int i = -1;
                    int mult = -1;
                    trouve = false;
                    while (i < TOTAL.Length && !trouve)
                    {
                        i++;
                        mult = 0;
                        int vecteur = tabPos[j];
                        while (mult < MAX_DEPLACEMENT && tab120[vecteur] != -1 && !trouve)
                        {
                            mult++;
                            vecteur = tabPos[j] + TOTAL[i] * mult;
                            if (tab120[vecteur] != -1)
                                if(valide(j, tab120[vecteur]) && tab120[vecteur] == ind) trouve = true;
                        }
                    }
                    for(int a=0; a<tabVal.Length; a++)
                    {
                        if(tabVal[a] * trait < 0 && tabVal[a] != R*(-trait))
                        {
                            for(int b = 1; b <= mult; b++)
                            {
                                int vecteur = tabPos[a] + TOTAL[i] * b;
                                setTrait();
                                if (tab120[vecteur] != -1 && valide(a, tab120[vecteur]))
                                {
                                    setTrait();
                                    return false;
                                }
                                setTrait();
                            }
                            
                        }
                    }
                }
                return true;
            }
            return false;
        }

        //Test pour savoir si on peu faire un petit roque
        public bool petitRoque()
        {
            if((trait < 0 && petitRoqueN) || (trait > 0 && petitRoqueB))
            {
                int i, j;
                i = j = 0;
                while (tabVal[i] != R * trait && i < tabVal.Length) i++;

                for (j = i + 1; j < (i+3); j++)
                {
                    if (tabVal[j] == 0)
                    {                        
                        tabVal[i] = 0;
                        tabVal[j] = R * trait;

                        setTrait();
                        miseEnEchec();
                        setTrait();

                        tabVal[i] = tabVal[j];
                        tabVal[j] = 0;

                        if (echec) return false;                      
                    }
                    else return false;
                }
                tabVal[j - 1] = tabVal[i];
                tabVal[i] = tabVal[j] = 0;
                tabVal[j - 2] = TD * trait; 
                return true;
            }
            return false;
        }

        //Idem pour le grand roque
        public bool grandRoque()
        {
            if ((trait < 0 && grandRoqueN) || (trait > 0 && grandRoqueB))
            {
                int i, j;
                i = j = 0;
                while (tabVal[i] != R * trait && i < tabVal.Length) i++;

                for (j = i-1; j > (i - 4); j--)
                {
                    if (tabVal[j] == 0)
                    {
                        tabVal[i] = 0;
                        tabVal[j] = R * trait;

                        setTrait();
                        miseEnEchec();
                        setTrait();

                        tabVal[i] = tabVal[j];
                        tabVal[j] = 0;

                        if (echec) return false;
                    }
                    else return false;
                }
                tabVal[j + 2] = tabVal[i];
                tabVal[i] = tabVal[j] = 0;
                tabVal[j + 3] = TD * trait;
                return true;
            }
            return false;
        }

        //Test pour savoir s'il y a une promotion
        public bool promotion(int arr)
        {
            if ((tabVal[arr] == P && (tabCoord[arr] == "a8" || tabCoord[arr] == "b8" || tabCoord[arr] == "c8" || tabCoord[arr] == "d8" ||
                tabCoord[arr] == "e8" || tabCoord[arr] == "f8" || tabCoord[arr] == "g8" || tabCoord[arr] == "h8")) ||
                tabVal[arr] == -P && (tabCoord[arr] == "a1" || tabCoord[arr] == "b1" || tabCoord[arr] == "c1" || tabCoord[arr] == "d1" ||
                tabCoord[arr] == "e1" || tabCoord[arr] == "f1" || tabCoord[arr] == "g1" || tabCoord[arr] == "h1"))
            {
                return true;
            }
            else return false;
        }
    }

}
        
