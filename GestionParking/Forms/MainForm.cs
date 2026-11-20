using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GestionParking.Data;
using GestionParking.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestionParking.Forms
{
   
    public class MainForm : Form
    {
        
        private readonly Label lblClients = new Label();
        private readonly Label lblPlaces = new Label();
        private readonly Label lblReservations = new Label();
        private readonly Label lblTaux = new Label();

        public MainForm()
        {
            Theme.AppliquerFenetre(this, "Gestion de Parking", 1100, 640);
            ConstruireInterface();
            ChargerStatistiques();
        }

        private void ConstruireInterface()
        {
            
            var contenu = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30, 20, 30, 20) };

            contenu.Controls.Add(new Label { Text = "Tableau de bord", Font = new Font("Segoe UI Semibold", 13F), AutoSize = true, Location = new Point(30, 20) });

            
            var cartes = new FlowLayoutPanel { Location = new Point(30, 55), Size = new Size(1020, 120) };
            cartes.Controls.Add(CreerCarteStat("Clients inscrits", lblClients, Theme.Bleu));
            cartes.Controls.Add(CreerCarteStat("Places libres", lblPlaces, Theme.Vert));
            cartes.Controls.Add(CreerCarteStat("Réservations actives", lblReservations, Theme.Orange));
            cartes.Controls.Add(CreerCarteStat("Taux d'occupation", lblTaux, Theme.Gris));
            contenu.Controls.Add(cartes);

            contenu.Controls.Add(new Label { Text = "Gestion", Font = new Font("Segoe UI Semibold", 13F), AutoSize = true, Location = new Point(30, 195) });

            
            var tuiles = new FlowLayoutPanel { Location = new Point(30, 230), Size = new Size(1020, 230) };
            tuiles.Controls.Add(CreerTuile("👤", "Clients", "Créer, modifier et supprimer les clients et leurs véhicules.", Theme.Bleu, () => new FormClients()));
            tuiles.Controls.Add(CreerTuile("🅿", "Parking", "Gérer les places du parking et les tarifs appliqués.", Theme.Vert, () => new FormParking()));
            tuiles.Controls.Add(CreerTuile("📅", "Réservations", "Réserver une place, payer, annuler une réservation.", Theme.Orange, () => new FormReservations()));
            contenu.Controls.Add(tuiles);

            Controls.Add(contenu);

            
            Controls.Add(Theme.CreerEntete("Gestion de Parking", "Mini-projet Programmation avancée · Entity Framework Core + SQLite"));
        }

        
        private Panel CreerCarteStat(string titre, Label valeur, Color couleur)
        {
            var carte = new Panel { Size = new Size(240, 105), BackColor = Theme.Carte, Margin = new Padding(0, 0, 14, 0) };
            carte.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 6, BackColor = couleur });   // liseré coloré
            carte.Controls.Add(new Label { Text = titre, ForeColor = Theme.Gris, AutoSize = true, Location = new Point(22, 16) });

            valeur.Font = Theme.Chiffre;
            valeur.ForeColor = couleur;
            valeur.AutoSize = true;
            valeur.Location = new Point(18, 44);
            carte.Controls.Add(valeur);
            return carte;
        }

        
        private Panel CreerTuile(string icone, string titre, string description, Color couleur, Func<Form> creerFenetre)
        {
            var tuile = new Panel { Size = new Size(320, 200), BackColor = Theme.Carte, Margin = new Padding(0, 0, 18, 0), Cursor = Cursors.Hand };

            var pastille = new Label
            {
                Text = icone, Font = new Font("Segoe UI Emoji", 20F), ForeColor = Color.White, BackColor = couleur,
                Size = new Size(56, 56), Location = new Point(24, 24), TextAlign = ContentAlignment.MiddleCenter
            };
            var lblTitre = new Label { Text = titre, Font = new Font("Segoe UI Semibold", 14F), AutoSize = true, Location = new Point(24, 94) };
            var lblDesc = new Label { Text = description, ForeColor = Theme.Gris, Size = new Size(275, 50), Location = new Point(24, 128) };
            tuile.Controls.AddRange(new Control[] { pastille, lblTitre, lblDesc });

            
            EventHandler ouvrir = (s, e) =>
            {
                using (var f = creerFenetre())
                    f.ShowDialog(this);
                ChargerStatistiques();
            };
            tuile.Click += ouvrir;

            
            EventHandler survol = (s, e) => tuile.BackColor = Color.FromArgb(239, 246, 255);
            EventHandler sortie = (s, e) => tuile.BackColor = Theme.Carte;
            tuile.MouseEnter += survol;
            tuile.MouseLeave += sortie;
            foreach (Control c in tuile.Controls)
            {
                c.Click += ouvrir;
                c.Cursor = Cursors.Hand;
                c.MouseEnter += survol;
            }
            return tuile;
        }

        
        private void ChargerStatistiques()
        {
            using var db = new ParkingContext();

            lblClients.Text = db.Clients.Count().ToString();

            var parkings = db.Parkings.Include(p => p.Places).ToList();
            int libres = parkings.Sum(p => p.GetNombrePlacesLibres());
            int total = parkings.Sum(p => p.Places.Count);
            lblPlaces.Text = $"{libres} / {total}";

            lblReservations.Text = db.Reservations
                .Count(r => r.Statut == StatutReservation.EN_ATTENTE || r.Statut == StatutReservation.CONFIRMEE)
                .ToString();

            double taux = parkings.Count == 0 ? 0 : parkings.Average(p => p.GetTauxOccupation());
            lblTaux.Text = taux.ToString("P0");
        }
    }
}
