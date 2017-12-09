using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace jeu_echec_stage
{
    class Jeu
    {
        Form1 form;
        private Echiquier ech;

        private const int NB_CASE = 64;
        private const int NB_CASE_LARGE = 8;

        private Mutex mutexStartAI1, mutexAI1, mutexStartAI2, mutexAI2;
        private String msg; //Contient le message envoyé aux IA
        private String[] jouer; //Contient les réponses des IA avec les coordonnées du déplacement
        private int fin, rejouer;
        private volatile bool stop, startThread, startGame; //Gère les boucles du thread

        public Jeu(Form1 form)
        {
            this.form = form;
            ech = Echiquier.Instance();
            msg = "";
            jouer = new String[3];
            fin = rejouer = 0;
            stop = startGame = false;
            startThread = true;
        }

        //setters
        public void setFin(int val) { fin = val; }
        public void setStartGame(bool val) { startGame = val; }
        public void setStartThread(bool val) { startThread = val; }
        public void setStop(bool val) { stop = val; }

        public void reinit()
        {
            rejouer++; //permet la gestion des mutex quand on rejoue
            msg = "";
            fin = 0;
            stop = startGame = false;
            startThread = true;
        }

        public void DoWork()
        {
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen("plateau", 100000))
            {
                using (MemoryMappedFile mmf2 = MemoryMappedFile.CreateOrOpen("repAI1", 100000))
                {
                    using (MemoryMappedFile mmf3 = MemoryMappedFile.CreateOrOpen("repAI2", 100000))
                    {
                        mutexStartAI1 = new Mutex(true, "mutexStartAI1");
                        mutexAI1 = new Mutex(true, "mutexAI1");
                        mutexStartAI2 = new Mutex(true, "mutexStartAI2");
                        mutexAI2 = new Mutex(true, "mutexAI2");

                        //Reprend les mutex nécessaires pour relancer une partie
                        if(rejouer > 0)
                        {
                            mutexAI1.WaitOne();
                            mutexStartAI1.WaitOne();
                            mutexAI2.WaitOne();
                            mutexStartAI2.WaitOne();
                        }

                        mutexAI1.ReleaseMutex();
                        mutexAI2.ReleaseMutex();
                       
                        while (startThread)
                        {
                            while (startGame)
                            {
                                if (!stop)
                                {
                                    //Gestion de la communication
                                    if (ech.getTrait() > 0) //Si on s'adresse à l'IA1 (les blancs)
                                    {
                                        //Ecriture du tableau de jeu
                                        using (var accessor = mmf.CreateViewAccessor())
                                        {
                                            msg = ech.getTabVal(0).ToString();
                                            for (int i = 1; i < NB_CASE; i++)
                                            {
                                                msg += "," + ech.getTabVal(i).ToString();
                                            }
                                            byte[] Buffer = ASCIIEncoding.ASCII.GetBytes(msg);
                                            accessor.Write(0, (ushort)Buffer.Length);
                                            accessor.WriteArray(0 + 2, Buffer, 0, Buffer.Length);
                                        }
                                        msg = "";
                                        mutexStartAI1.ReleaseMutex();
                                        DateTime start = DateTime.Now; //Pour calculer le temps de réponse de l'IA
                                        mutexAI1.WaitOne();
                                        TimeSpan dur = DateTime.Now - start;

                                        //Elle perd si elle répond en plus de 250ms
                                        if (dur.TotalMilliseconds > 250)
                                        {
                                            form.setJoueur("BLANCS");
                                            fin = 1;
                                        }
                                        //Lecture de la réponse
                                        using (var accessor = mmf2.CreateViewAccessor())
                                        {
                                            ushort Size = accessor.ReadUInt16(0);
                                            byte[] Buffer = new byte[Size];
                                            accessor.ReadArray(0 + 2, Buffer, 0, Buffer.Length);
                                            String value = ASCIIEncoding.ASCII.GetString(Buffer);
                                            jouer = value.Split(',');
                                            mutexStartAI1.WaitOne();
                                            mutexAI1.ReleaseMutex();
                                        }
                                    }
                                    //Même chose si on s'adresse à l'IA2
                                    else
                                    {
                                        using (var accessor = mmf.CreateViewAccessor())
                                        {
                                            msg = ech.getTabVal(0).ToString();
                                            for (int i = 1; i < NB_CASE; i++)
                                            {
                                                msg += "," + ech.getTabVal(i).ToString();
                                            }
                                            byte[] Buffer = ASCIIEncoding.ASCII.GetBytes(msg);
                                            accessor.Write(0, (ushort)Buffer.Length);
                                            accessor.WriteArray(0 + 2, Buffer, 0, Buffer.Length);
                                        }
                                        msg = "";
                                        mutexStartAI2.ReleaseMutex();
                                        DateTime start = DateTime.Now;
                                        mutexAI2.WaitOne();
                                        TimeSpan dur = DateTime.Now - start;

                                        if (dur.TotalMilliseconds > 1000)
                                        {
                                            form.setJoueur("NOIRS");
                                            form.finPartie(1);
                                        }

                                        using (var accessor = mmf3.CreateViewAccessor())
                                        {
                                            ushort Size = accessor.ReadUInt16(0);
                                            byte[] Buffer = new byte[Size];
                                            accessor.ReadArray(0 + 2, Buffer, 0, Buffer.Length);
                                            String value = ASCIIEncoding.ASCII.GetString(Buffer);
                                            jouer = value.Split(',');
                                            mutexStartAI2.WaitOne();
                                            mutexAI2.ReleaseMutex();
                                        }
                                    }
                                    //On joue
                                    tour();
                                    
                                    //On affiche le tour
                                    if (ech.getTrait() == 1) form.setLblTrait("Trait aux blancs");
                                    else form.setLblTrait("Trait aux noirs");

                                    //Règle le temps d'affichage d'un déplacement avant le suivant
                                    Thread.Sleep(50);

                                    //On test la fin de partie
                                    if(fin != 0)
                                    {
                                        form.finPartie(fin);
                                    }
                                }
                                //On termine la communication avec les IA
                                else
                                {
                                    using (var accessor = mmf.CreateViewAccessor())
                                    {
                                        msg = "stop";
                                        byte[] Buffer = ASCIIEncoding.ASCII.GetBytes(msg);
                                        accessor.Write(0, (ushort)Buffer.Length);
                                        accessor.WriteArray(0 + 2, Buffer, 0, Buffer.Length);
                                    }
                                    setStartGame(false);
                                    setStartThread(false);
                                    mutexStartAI1.ReleaseMutex();
                                    mutexStartAI2.ReleaseMutex();
                                }
                            }
                        }
                    }
                }
            }
        }

        public void tour()
        {
            int dep, arr;
            bool valide = false;

            form.setLblAICoordDep(jouer[0].ToUpper());
            form.setLblAICoordArr(jouer[1].ToUpper());
            form.setLblAIPromotion(jouer[2].ToUpper());

            switch (jouer[0])
            {
                case "grand roque":
                    if (ech.grandRoque())
                    {
                        if (ech.getTrait() < 0) for (int i = 0; i< 5; i++) form.chargerImage(i, ech.getTabVal(i));
                        else for (int i = 56; i<NB_CASE; i++) form.chargerImage(i, ech.getTabVal(i));

                        ech.setTrait();
                    }
                    else form.setLblResult("Grand roque interdit");
                    break;

                case "petit roque":
                    if (ech.petitRoque())
                    {
                        if (ech.getTrait() < 0) for (int i = 4; i<NB_CASE_LARGE; i++) form.chargerImage(i, ech.getTabVal(i));
                        else for (int i = 60; i<NB_CASE; i++) form.chargerImage(i, ech.getTabVal(i));

                        ech.setTrait();
                    }
                    else form.setLblResult("Petit roque interdit");
                    break;

                default:
                    //On trouve l'indice de la coordonnée dans le tableau
                    dep = ech.coord(form.getLblAICoordDep());
                    arr = ech.coord(form.getLblAICoordArr());

                    //Si les coordonnées sont valides on regarde si le déplacement est valide
                    if (dep >= 0 && arr >= 0)
                    {
                        valide = ech.valide(dep, arr);
                        if (!valide || ech.getTabVal(arr) == 6 * (-ech.getTrait())) form.setLblResult("Déplacement interdit");
                    }
                    else form.setLblResult("Coordonées invalides");

                    //Si le deplacement est valide on le fait
                    if (valide && ech.getTabVal(arr) != 6 * (-ech.getTrait()))
                    {
                        ech.enregistrerCoup(dep, arr);
                        ech.deplacement(dep, arr);

                        //On test si le déplacement nous met en echec
                        ech.setTrait();
                        if (ech.miseEnEchec())
                        {
                            ech.annulerCoup(dep, arr);
                            form.setLblResult("Déplacement interdit : vous êtes en echec");
                        }
                        ech.setTrait();

                        //Si non
                        if (!ech.getEchec())
                        {
                            form.manger(arr);
                            form.chargerImage(dep, ech.getTabVal(dep));
                            form.chargerImage(arr, ech.getTabVal(arr));
                            if (ech.promotion(arr) && (jouer[2].ToUpper() == "TG" || jouer[2].ToUpper() == "CG" || jouer[2].ToUpper() == "F" || jouer[2].ToUpper() == "D"))
                            {
                                form.setLblAIPromotion(ech.setTabVal(arr, jouer[2].ToUpper()));
                                form.chargerImage(arr, ech.getTabVal(arr));
                            }

                            if (ech.promotion(arr) && (jouer[2].ToUpper() != "TG" || jouer[2].ToUpper() != "CG" || jouer[2].ToUpper() != "F" || jouer[2].ToUpper() != "D"))
                            {
                                ech.annulerCoup(dep, arr);
                                form.chargerImage(dep, ech.getTabVal(dep));
                                form.chargerImage(arr, ech.getTabVal(arr));
                                form.setLblAIPromotion("Promotion incorrect");
                                ech.setTrait();
                             }
                             else if (ech.mat())
                             {
                                if (ech.getTrait() < 0) form.setJoueur("NOIRS");
                                else form.setJoueur("BLANCS");
                                fin = 3;
                             }
                             else if (ech.miseEnEchec()) form.setLblResult("ECHEC !");
                             else form.setLblResult("");
                             ech.setTrait(); //on change le tour
                        }
                    }
                    break;
                }
            }
    }
}
