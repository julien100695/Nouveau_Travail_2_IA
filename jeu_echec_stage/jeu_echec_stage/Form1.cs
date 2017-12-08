using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace jeu_echec_stage
{
    public partial class Form1 : Form
    {
        private static Form1 _me;

        private const int NB_CASE = 64; //Nombre de case au total
        private const int NB_CASE_LARGE = 8; //Nombre de cases en largeur ou longueur
        private const int TAILLE_BTN = 60; //Taille d'une case

        //private const String IMG_PATH = "D:\\Logiciels développés\\[2017-06-30] - Échecs\\jeu_echec_stage\\images\\";
        private const String IMG_PATH = "C:\\Users\\Francis\\Desktop\\Devoirs\\IA\\Travail2_IA\\Travail2_IA\\jeu_echec_stage\\images\\";
        private Button[] plateau; //Tableau de bouton qui représente l'échiquier
        private Echiquier ech = Echiquier.Instance(); //Instance de la classe Echiquier pour utiliser les méthodes de la classe
        private Jeu jeu; //Instance qui gère le déroulement du jeu dans un autre thread
        private Thread thread;
        private int xMangeB, yMangeB, xMangeN, yMangeN; //Coordonnées des images pour les mettre dans le panel des pièces mangées
        private String joueur; //Equipe qui gagne ou perd

        private Form1()
        {
            InitializeComponent();
        }

        public static Form1 Instance()
        {
            if (_me == null)
            {
                _me = new Form1();
            }
            return _me;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            initialisation();
        }

        public void initialisation()
        {
            jeu = new Jeu(this);
            plateau = new Button[NB_CASE];
            thread = new Thread(jeu.DoWork);
            thread.Start();
            xMangeB = yMangeB = xMangeN = yMangeN = 0;
            creerPlateau();
            btnQuitter.Enabled = false;
            btnRejouer.Enabled = false;
            btnPat.Enabled = false;
        }

        public void rejouer()
        {
            ech.reinit();
            jeu.reinit();
            thread = new Thread(jeu.DoWork);
            thread.Start();
            for(int i = 0; i  <NB_CASE; i++)
            {
                chargerImage(i, ech.getTabVal(i));
            }
            pnlPieceB.Controls.Clear();
            pnlPieceN.Controls.Clear();
            xMangeB = yMangeB = xMangeN = yMangeN = 0;
            setBtnJouer(true);
            setBtnPat(true);
            btnQuitter.Enabled = false;
            btnRejouer.Enabled = false;
            btnPat.Enabled = false;
            setLblTrait("Trait aux blancs");
            setLblAICoordDep("");
            setLblAICoordArr("");
            setLblGagnant("");
            setLblResult("");
            setColor("black");
        }

        //Affichage de fin de partie 
        public void finPartie(int val)
        {
            jeu.setStop(true); //Permet de fermer les IA
            switch (val)
            {
                case 1:
                    setLblTrait("TEMPS ECOULÉ");
                    setLblGagnant("Les " + joueur + " ont été trop long à répondre");
                    break;
                case 2:
                    setLblTrait("PAT !");
                    setLblGagnant("La partie est abandonnée");
                    break;
                case 3:
                    setLblTrait("ECHEC ET MAT !");
                    setLblGagnant("Les " + joueur + " ont gagné !");
                    break;
            }
            setColor("red");
            setBtnPat(false);
            setBtnQuitter(true);
            setBtnRejouer(true);
        }

        /*****************************************************************/
        /**************************** BOUTONS ****************************/
        /*****************************************************************/
        private void btnJouer_Click(object sender, EventArgs e)
        {
            jeu.setStartGame(true);
            btnJouer.Enabled = false;
            btnPat.Enabled = true;
        }

        private void btnPat_Click(object sender, EventArgs e)
        {
            jeu.setFin(2);
        }

        private void btnRejouer_Click(object sender, EventArgs e)
        {
            rejouer();
        }

        private void btnQuitter_Click(object sender, EventArgs e)
        {
            jeu.setStop(true);
            this.Close();
        }

        /****************************************************************************/
        /**************************** SETTERS ET GETTERS ****************************/
        /****************************************************************************/
        public void setBtnJouer(bool val) { btnJouer.Invoke((MethodInvoker)delegate { btnJouer.Enabled = val; }); }
        public void setBtnPat(bool val) { btnPat.Invoke((MethodInvoker)delegate { btnPat.Enabled = val; }); }
        public void setBtnQuitter(bool val) { btnQuitter.Invoke((MethodInvoker)delegate { btnQuitter.Enabled = val; }); }
        public void setBtnRejouer(bool val) { btnRejouer.Invoke((MethodInvoker)delegate { btnRejouer.Enabled = val; }); }

        public void setLblAICoordDep(String text) { lblAICoordDep.Invoke((MethodInvoker)delegate { lblAICoordDep.Text = text; }); }
        public void setLblAICoordArr(String text) { lblAICoordArr.Invoke((MethodInvoker)delegate { lblAICoordArr.Text = text; }); }
        public void setLblAIPromotion(String text) { lblAIPromotion.Invoke((MethodInvoker)delegate { lblAIPromotion.Text = text; }); }
        public void setLblResult(String text) { lblResult.Invoke((MethodInvoker)delegate { lblResult.Text = text; }); }
        public void setLblTrait(String text) { lblTrait.Invoke((MethodInvoker)delegate { lblTrait.Text = text; }); }
        public void setLblGagnant(String text) { lblGagnant.Invoke((MethodInvoker)delegate { lblGagnant.Text = text; }); }

        public void setJoueur(String text) { joueur = text; }
        public void setColor(String color)
        {
            if (color == "red")
            {
                lblTrait.Invoke((MethodInvoker)delegate { lblTrait.ForeColor = System.Drawing.Color.Red; });
                lblGagnant.Invoke((MethodInvoker)delegate { lblGagnant.ForeColor = System.Drawing.Color.Red; });
            }
            else
            {
                lblTrait.Invoke((MethodInvoker)delegate { lblTrait.ForeColor = System.Drawing.Color.Black; });
                lblGagnant.Invoke((MethodInvoker)delegate { lblGagnant.ForeColor = System.Drawing.Color.Black; });
            }
        }

        public String getLblAICoordDep() { return lblAICoordDep.Text.ToLower(); }
        public String getLblAICoordArr() { return lblAICoordArr.Text.ToLower(); }

        /*********************************************************************/
        /************************ CREATION DU PLATEAU ************************/
        /*********************************************************************/
        public void creerPlateau()
        {
            int x, y;
            bool couleur = true;
            x = y = 0;

            //On initialise la taille du panel qui contient les boutons et les pièces mangées
            pnlPlateau.Width = (NB_CASE * TAILLE_BTN);
            pnlPlateau.Height = (NB_CASE * TAILLE_BTN);
            pnlPieceB.Width = pnlPieceN.Width = (6 * TAILLE_BTN);
            pnlPieceB.Height = pnlPieceN.Height = (NB_CASE_LARGE * TAILLE_BTN) / 2;
            pnlPieceB.Location = new Point(795, 95);
            pnlPieceN.Location = new Point(795, 335);

            //On crée un tableau de boutons
            for (int i = 0; i < NB_CASE; i++)
            {
                if (i % 8 == 0 && i != 0)
                {
                    x = 0;
                    y += TAILLE_BTN;
                    couleur = !couleur;
                }

                plateau[i] = new Button();
                plateau[i].Width = TAILLE_BTN;
                plateau[i].Height = TAILLE_BTN;
                plateau[i].Location = new Point(x, y);
                plateau[i].Enabled = false;

                //Couleurs des cases
                if (!couleur)
                {
                    plateau[i].BackColor = Color.DarkGray;
                    couleur = true;
                }
                else
                {
                    plateau[i].BackColor = Color.GhostWhite;
                    couleur = false;
                }

                pnlPlateau.Controls.Add(plateau[i]);
                x += TAILLE_BTN;

                chargerImage(i, ech.getTabVal(i));
            }
        }

        /**********************************************************************/
        /************************ CHARGEMENT DES IMAGES ***********************/
        /**********************************************************************/
        public void chargerImage(int ind, int val)
        {
            switch (val)
            {
                case -1:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH+"PN.png"); break;
                case 1:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "PB.png"); break;
                case -21:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "TN.png");
                    if (ind != ech.coord("a8")) ech.setGRN();
                    break;
                case -22:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "TN.png");
                    if (ind != ech.coord("h8")) ech.setPRN();
                    break;
                case 21:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "TB.png");
                    if (ind != ech.coord("a1")) ech.setGRB();
                    break;
                case 22:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "TB.png");
                    if (ind != ech.coord("h1")) ech.setPRB();
                    break;
                case -31:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "CGN.png"); break;
                case -32:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "CDN.png"); break;
                case 31:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "CGB.png"); break;
                case 32:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "CDB.png"); break;
                case -4:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "FN.png"); break;
                case 4:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "FB.png"); break;
                case -5:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "DN.png"); break;
                case 5:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "DB.png"); break;
                case -6:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH + "RN.png");
                    if (ind != ech.coord("e8"))
                    {
                        ech.setPRN();
                        ech.setGRN();
                    }
                    break;
                case 6:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH+"RB.png");
                    if (ind != ech.coord("e1"))
                    {
                        ech.setPRB();
                        ech.setGRB();
                    }
                    break;
                case 0:
                    plateau[ind].BackgroundImage = Image.FromFile(IMG_PATH+"vide.png");
                    break;
                default:
                    break;
            }
        }

        /****************************************************************/
        /************************ PIECES MANGÉES ************************/
        /****************************************************************/
        public void manger(int arr)
        {
            //Si une pièce est mangée lors du déplacement on charge son image dans le panel correspondant
            if (ech.getSaveArr() != 0)
            {
                PictureBox pb = new PictureBox();

                //Si c'est une pièce prise en passant
                if (ech.getSaveArr() == -10 || ech.getSaveArr() == 10)
                {
                    int ind = arr + (NB_CASE_LARGE * ech.getTrait()); //indice du pion
                    pb.BackgroundImage = plateau[ind].BackgroundImage;
                    chargerImage(ind, ech.getTabVal(ind)); //On charge l'image vide à la place du pion
                }

                //Sinon on prend l'image de la case d'arrivée
                else pb.BackgroundImage = plateau[arr].BackgroundImage;

                pb.Width = TAILLE_BTN;
                pb.Height = TAILLE_BTN;

                //Si c'est une pièce noire
                if (ech.getSaveArr() < 0)
                {
                    pb.Location = new Point(xMangeN, yMangeN);
                    xMangeN += TAILLE_BTN;
                    if(xMangeN == pnlPieceN.Width)
                    {
                        xMangeN = 0;
                        yMangeN += TAILLE_BTN;
                    }
                    pnlPieceN.Invoke((MethodInvoker)delegate { pnlPieceN.Controls.Add(pb); });
                }
                //Si c'est une pièce blanche
                else
                {
                    pb.Location = new Point(xMangeB, yMangeB);
                    xMangeB += TAILLE_BTN;
                    if (xMangeB == pnlPieceB.Width)
                    {
                        xMangeB = 0;
                        yMangeB += TAILLE_BTN;
                    }
                    
                    pnlPieceB.Invoke((MethodInvoker)delegate { pnlPieceB.Controls.Add(pb); });
                }
            }
        }
    }
}
