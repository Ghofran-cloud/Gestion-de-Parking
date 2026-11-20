using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GestionParking.Data;
using GestionParking.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionParking.Forms
{
    /// <summary>
    /// Membre 1 — Gestion des clients (CRUD) et de leurs véhicules.
    /// </summary>
    public class FormClients : Form
    {
        // Un seul DbContext pendant toute la vie de la fenêtre
        private readonly ParkingContext db = new ParkingContext();

        private readonly DataGridView grilleClients = new DataGridView();
        private readonly DataGridView grilleVehicules = new DataGridView();
        private readonly TextBox txtRecherche = new TextBox();

        // Champs du client
        private readonly TextBox txtNom = new TextBox();
        private readonly TextBox txtPrenom = new TextBox();
        private readonly TextBox txtEmail = new TextBox();
        private readonly TextBox txtTelephone = new TextBox();
        private readonly TextBox txtMotDePasse = new TextBox { UseSystemPasswordChar = true };

        // Champs du véhicule
        private readonly TextBox txtImmat = new TextBox { Width = 130 };
        private readonly TextBox txtMarque = new TextBox { Width = 120 };
        private readonly TextBox txtModele = new TextBox { Width = 120 };
        private readonly ComboBox cmbType = new ComboBox { Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label lblVehicules = new Label();

        public FormClients()
        {
            Theme.AppliquerFenetre(this, "Gestion des clients", 1200, 740);
            ConstruireInterface();
            ChargerClients();
            FormClosed += (s, e) => db.Dispose();
        }

        // =====================================================================
        //  Construction de l'interface
        // =====================================================================
        private void ConstruireInterface()
        {
            // ----- Zone centrale : recherche + grille des clients + véhicules -----
            var centre = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            Theme.StyliserGrille(grilleClients);
            grilleClients.SelectionChanged += (s, e) => AfficherClientSelectionne();

            var barreRecherche = new Panel { Dock = DockStyle.Top, Height = 46 };
            barreRecherche.Controls.Add(new Label { Text = "🔍 Rechercher :", AutoSize = true, Location = new Point(0, 10) });
            txtRecherche.Location = new Point(120, 6);
            txtRecherche.Width = 300;
            txtRecherche.PlaceholderText = "nom, prénom ou email…";
            txtRecherche.TextChanged += (s, e) => ChargerClients();
            barreRecherche.Controls.Add(txtRecherche);

            centre.Controls.Add(grilleClients);
            centre.Controls.Add(ConstruireZoneVehicules());
            centre.Controls.Add(barreRecherche);
            Controls.Add(centre);

            // ----- Panneau de droite : formulaire du client -----
            var panneau = Theme.CreerPanneauDroite("Fiche client");
            int y = 60;
            Theme.AjouterChamp(panneau, "Nom *", txtNom, ref y);
            Theme.AjouterChamp(panneau, "Prénom *", txtPrenom, ref y);
            Theme.AjouterChamp(panneau, "Email *", txtEmail, ref y);
            Theme.AjouterChamp(panneau, "Téléphone", txtTelephone, ref y);
            Theme.AjouterChamp(panneau, "Mot de passe * (vide = inchangé)", txtMotDePasse, ref y);

            var btnAjouter = Theme.CreerBouton("➕ Ajouter", Theme.Vert);
            var btnModifier = Theme.CreerBouton("✏ Modifier", Theme.Bleu);
            var btnSupprimer = Theme.CreerBouton("🗑 Supprimer", Theme.Rouge);
            var btnVider = Theme.CreerBouton("Nouveau", Theme.Gris);
            btnAjouter.Click += (s, e) => AjouterClient();
            btnModifier.Click += (s, e) => ModifierClient();
            btnSupprimer.Click += (s, e) => SupprimerClient();
            btnVider.Click += (s, e) => ViderFormulaire();
            Theme.CreerRangeeBoutons(panneau, y + 10, btnAjouter, btnModifier, btnSupprimer, btnVider);
            Controls.Add(panneau);

            // ----- Bandeau du haut -----
            Controls.Add(Theme.CreerEntete("👤  Clients", "Sélectionnez un client dans la liste pour le modifier ou le supprimer"));
        }

        private Panel ConstruireZoneVehicules()
        {
            var zone = new Panel { Dock = DockStyle.Bottom, Height = 250, Padding = new Padding(0, 12, 0, 0) };

            Theme.StyliserGrille(grilleVehicules);

            lblVehicules.Text = "Véhicules du client";
            lblVehicules.Font = new Font("Segoe UI Semibold", 12F);
            lblVehicules.Dock = DockStyle.Top;
            lblVehicules.Height = 30;

            cmbType.Items.AddRange(new object[] { "Voiture", "Moto", "Utilitaire" });
            cmbType.SelectedIndex = 0;

            var barre = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(0, 4, 0, 0) };
            barre.Controls.Add(PetitLabel("Immat."));
            barre.Controls.Add(txtImmat);
            barre.Controls.Add(PetitLabel("Marque"));
            barre.Controls.Add(txtMarque);
            barre.Controls.Add(PetitLabel("Modèle"));
            barre.Controls.Add(txtModele);
            barre.Controls.Add(cmbType);

            var btnAjouterV = Theme.CreerBouton("➕ Véhicule", Theme.Vert, 120);
            var btnSupprimerV = Theme.CreerBouton("🗑", Theme.Rouge, 44);
            btnAjouterV.Click += (s, e) => AjouterVehicule();
            btnSupprimerV.Click += (s, e) => SupprimerVehicule();
            barre.Controls.Add(btnAjouterV);
            barre.Controls.Add(btnSupprimerV);

            zone.Controls.Add(grilleVehicules);
            zone.Controls.Add(barre);
            zone.Controls.Add(lblVehicules);
            return zone;
        }

        private static Label PetitLabel(string texte) =>
            new Label { Text = texte, AutoSize = true, ForeColor = Theme.Gris, Margin = new Padding(6, 9, 2, 0) };

        // =====================================================================
        //  Lecture (R du CRUD)
        // =====================================================================
        private void ChargerClients()
        {
            string filtre = txtRecherche.Text.Trim().ToLower();

            grilleClients.DataSource = db.Clients
                .Include(c => c.Vehicules)
                .AsEnumerable()   // la suite du filtre se fait en C#
                .Where(c => filtre == "" ||
                            c.Nom.ToLower().Contains(filtre) ||
                            c.Prenom.ToLower().Contains(filtre) ||
                            c.Email.ToLower().Contains(filtre))
                .OrderBy(c => c.Nom)
                .Select(c => new
                {
                    c.Id,
                    c.Nom,
                    Prénom = c.Prenom,
                    c.Email,
                    Téléphone = c.Telephone,
                    Véhicules = c.Vehicules.Count
                })
                .ToList();
        }

        private Client? ClientSelectionne()
        {
            int? id = Theme.IdSelectionne(grilleClients);
            return id == null ? null : db.Clients.Include(c => c.Vehicules).FirstOrDefault(c => c.Id == id);
        }

        private void AfficherClientSelectionne()
        {
            var client = ClientSelectionne();
            if (client == null) return;

            txtNom.Text = client.Nom;
            txtPrenom.Text = client.Prenom;
            txtEmail.Text = client.Email;
            txtTelephone.Text = client.Telephone;
            txtMotDePasse.Text = "";
            ChargerVehicules(client);
        }

        private void ChargerVehicules(Client client)
        {
            lblVehicules.Text = $"Véhicules de {client.NomComplet}";
            grilleVehicules.DataSource = client.Vehicules
                .Select(v => new { v.Id, Immatriculation = v.Immatriculation, v.Marque, Modèle = v.Modele, v.Type })
                .ToList();
        }

        // =====================================================================
        //  Création / Modification / Suppression (C, U, D du CRUD)
        // =====================================================================
        private bool FormulaireValide(bool motDePasseObligatoire)
        {
            if (txtNom.Text.Trim() == "" || txtPrenom.Text.Trim() == "" || txtEmail.Text.Trim() == "")
            {
                Theme.Erreur("Le nom, le prénom et l'email sont obligatoires.");
                return false;
            }
            if (!txtEmail.Text.Contains("@"))
            {
                Theme.Erreur("L'adresse email n'est pas valide.");
                return false;
            }
            if (motDePasseObligatoire && txtMotDePasse.Text == "")
            {
                Theme.Erreur("Le mot de passe est obligatoire pour un nouveau client.");
                return false;
            }
            return true;
        }

        private void AjouterClient()
        {
            if (!FormulaireValide(motDePasseObligatoire: true)) return;

            var client = new Client
            {
                Nom = txtNom.Text.Trim(),
                Prenom = txtPrenom.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Telephone = txtTelephone.Text.Trim(),
                MotDePasse = txtMotDePasse.Text
            };
            db.Clients.Add(client);
            if (Enregistrer())
            {
                Theme.Info($"Le client {client.NomComplet} a été ajouté.");
                ViderFormulaire();
                ChargerClients();
            }
        }

        private void ModifierClient()
        {
            var client = ClientSelectionne();
            if (client == null) { Theme.Erreur("Sélectionnez d'abord un client."); return; }
            if (!FormulaireValide(motDePasseObligatoire: false)) return;

            client.Nom = txtNom.Text.Trim();
            client.Prenom = txtPrenom.Text.Trim();
            client.Email = txtEmail.Text.Trim();
            client.Telephone = txtTelephone.Text.Trim();
            if (txtMotDePasse.Text != "")
                client.MotDePasse = txtMotDePasse.Text;

            if (Enregistrer())
            {
                Theme.Info("Les modifications ont été enregistrées.");
                ChargerClients();
            }
        }

        private void SupprimerClient()
        {
            var client = ClientSelectionne();
            if (client == null) { Theme.Erreur("Sélectionnez d'abord un client."); return; }

            if (!Theme.Confirmer($"Supprimer {client.NomComplet} ?\n\nSes véhicules et ses réservations seront aussi supprimés."))
                return;

            db.Clients.Remove(client);
            if (Enregistrer())
            {
                ViderFormulaire();
                ChargerClients();
            }
        }

        private void AjouterVehicule()
        {
            var client = ClientSelectionne();
            if (client == null) { Theme.Erreur("Sélectionnez d'abord un client."); return; }
            if (txtImmat.Text.Trim() == "" || txtMarque.Text.Trim() == "")
            {
                Theme.Erreur("L'immatriculation et la marque sont obligatoires.");
                return;
            }

            try
            {
                // On utilise la méthode métier de la classe Client
                client.AjouterVehicule(new Vehicule
                {
                    Immatriculation = txtImmat.Text.Trim().ToUpper(),
                    Marque = txtMarque.Text.Trim(),
                    Modele = txtModele.Text.Trim(),
                    Type = cmbType.Text
                });
            }
            catch (InvalidOperationException ex)
            {
                Theme.Erreur(ex.Message);
                return;
            }

            if (Enregistrer())
            {
                txtImmat.Clear(); txtMarque.Clear(); txtModele.Clear();
                ChargerVehicules(client);
                ChargerClients();
            }
        }

        private void SupprimerVehicule()
        {
            var client = ClientSelectionne();
            int? id = Theme.IdSelectionne(grilleVehicules);
            if (client == null || id == null) { Theme.Erreur("Sélectionnez un véhicule."); return; }

            var vehicule = db.Vehicules.Find(id);
            if (vehicule == null) return;
            if (!Theme.Confirmer($"Supprimer le véhicule {vehicule} ?\nSes réservations seront aussi supprimées.")) return;

            db.Vehicules.Remove(vehicule);
            if (Enregistrer())
            {
                ChargerVehicules(client);
                ChargerClients();
            }
        }

        /// <summary>
        /// Enregistre les changements dans la base. En cas d'erreur (ex : email déjà utilisé),
        /// on affiche un message et on annule les changements en mémoire.
        /// </summary>
        private bool Enregistrer()
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (DbUpdateException)
            {
                Theme.Erreur("Enregistrement impossible : cet email ou cette immatriculation existe déjà.");
                db.ChangeTracker.Clear();
                return false;
            }
        }

        private void ViderFormulaire()
        {
            txtNom.Clear(); txtPrenom.Clear(); txtEmail.Clear(); txtTelephone.Clear(); txtMotDePasse.Clear();
            grilleClients.ClearSelection();
            grilleVehicules.DataSource = null;
            lblVehicules.Text = "Véhicules du client";
            txtNom.Focus();
        }
    }
}
