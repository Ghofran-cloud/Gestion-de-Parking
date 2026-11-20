using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GestionParking.Data;
using GestionParking.Enums;
using GestionParking.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionParking.Forms
{

   

    /// <summary>
    /// Membre 3 — Gestion des réservations : réserver, modifier, payer, annuler, supprimer.
    /// </summary>

    public class FormReservations : Form
    {
        private readonly ParkingContext db = new ParkingContext();

        private readonly DataGridView grille = new DataGridView();
        private readonly ComboBox cmbFiltre = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };

        private readonly ComboBox cmbClient = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox cmbVehicule = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox cmbPlace = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker dtDebut = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy   HH:mm" };
        private readonly DateTimePicker dtFin = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy   HH:mm" };
        private readonly ComboBox cmbMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label lblMontant = new Label();

        public FormReservations()
        {
            Theme.AppliquerFenetre(this, "Gestion des réservations", 1250, 780);
            ConstruireInterface();
            ChargerListes();
            ChargerReservations();
            FormClosed += (s, e) => db.Dispose();
        }


        

        // =====================================================================
        //  Construction de l'interface
        // =====================================================================

        private void ConstruireInterface()
        {
            var centre = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var barre = new Panel { Dock = DockStyle.Top, Height = 46 };
            barre.Controls.Add(new Label { Text = "Afficher :", AutoSize = true, Location = new Point(0, 10) });
            cmbFiltre.Location = new Point(75, 6);
            cmbFiltre.Items.Add("Toutes");
            foreach (var statut in Enum.GetNames(typeof(StatutReservation)))
                cmbFiltre.Items.Add(statut);
            cmbFiltre.SelectedIndex = 0;
            cmbFiltre.SelectedIndexChanged += (s, e) => ChargerReservations();
            barre.Controls.Add(cmbFiltre);

            Theme.StyliserGrille(grille);
            Theme.ColorerStatuts(grille, "Statut");
            Theme.ColorerStatuts(grille, "Paiement");
            grille.SelectionChanged += (s, e) => AfficherReservationSelectionnee();

            centre.Controls.Add(grille);
            centre.Controls.Add(barre);
            Controls.Add(centre);


            

            // ----- Panneau de droite -----

            var panneau = Theme.CreerPanneauDroite("Réservation", 360);
            int y = 60;
            Theme.AjouterChamp(panneau, "Client *", cmbClient, ref y);
            Theme.AjouterChamp(panneau, "Véhicule *", cmbVehicule, ref y);
            Theme.AjouterChamp(panneau, "Place *", cmbPlace, ref y);
            Theme.AjouterChamp(panneau, "Début", dtDebut, ref y);
            Theme.AjouterChamp(panneau, "Fin", dtFin, ref y);

            lblMontant.Font = new Font("Segoe UI Semibold", 15F);
            lblMontant.ForeColor = Theme.Bleu;
            lblMontant.AutoSize = true;
            lblMontant.Location = new Point(20, y);
            panneau.Controls.Add(lblMontant);
            y += 45;

            cmbMode.DataSource = Enum.GetValues(typeof(ModePaiement));
            Theme.AjouterChamp(panneau, "Mode de paiement", cmbMode, ref y);

            var btnReserver = Theme.CreerBouton("➕ Réserver", Theme.Vert, 150);
            var btnModifier = Theme.CreerBouton("✏ Modifier", Theme.Bleu, 150);
            var btnPayer = Theme.CreerBouton("💳 Payer", Theme.Orange, 150);
            var btnAnnuler = Theme.CreerBouton("✖ Annuler", Theme.Gris, 150);
            var btnSupprimer = Theme.CreerBouton("🗑 Supprimer", Theme.Rouge, 150);
            var btnNouveau = Theme.CreerBouton("Nouveau", Theme.Entete, 150);
            btnReserver.Click += (s, e) => Reserver();
            btnModifier.Click += (s, e) => ModifierReservation();
            btnPayer.Click += (s, e) => Payer();
            btnAnnuler.Click += (s, e) => AnnulerReservation();
            btnSupprimer.Click += (s, e) => SupprimerReservation();
            btnNouveau.Click += (s, e) => Nouveau();
            Theme.CreerRangeeBoutons(panneau, y + 10, btnReserver, btnModifier, btnPayer, btnAnnuler, btnSupprimer, btnNouveau);
            Controls.Add(panneau);

            Controls.Add(Theme.CreerEntete("📅  Réservations", "Créez une réservation, payez-la ou annulez-la (remboursement automatique)"));


            // Le montant estimé se met à jour quand on change la place ou les dates

            cmbClient.SelectedIndexChanged += (s, e) => ChargerVehiculesDuClient();
            cmbPlace.SelectedIndexChanged += (s, e) => MettreAJourMontant();
            dtDebut.ValueChanged += (s, e) => MettreAJourMontant();
            dtFin.ValueChanged += (s, e) => MettreAJourMontant();
        }


        
        private void ChargerListes()
        {
            

        // =====================================================================
        //  Chargement des données
        // =====================================================================
        private void ChargerListes()
        {
            // Les clients avec leurs véhicules et réservations

            cmbClient.DataSource = db.Clients
                .Include(c => c.Vehicules)
                .Include(c => c.Reservations)
                .OrderBy(c => c.Nom)
                .ToList();


            

            // Les places en service, avec leur parking (pour les tarifs) et leurs réservations (pour les chevauchements)

            cmbPlace.DataSource = db.Places
                .Include(p => p.Parking).ThenInclude(pk => pk!.Tarifs)
                .Include(p => p.Reservations)
                .Where(p => p.Statut != StatutPlace.HORS_SERVICE)
                .OrderBy(p => p.Numero)
                .ToList();

            Nouveau();
        }

        private void ChargerVehiculesDuClient()
        {
            cmbVehicule.DataSource = (cmbClient.SelectedItem as Client)?.Vehicules.ToList();
        }

        private void ChargerReservations()
        {
            var requete = db.Reservations
                .Include(r => r.Client)
                .Include(r => r.Vehicule)
                .Include(r => r.Paiement)
                .Include(r => r.Place).ThenInclude(p => p!.Parking).ThenInclude(pk => pk!.Tarifs)
                .AsQueryable();

            if (cmbFiltre.SelectedIndex > 0)
            {
                var statut = Enum.Parse<StatutReservation>(cmbFiltre.SelectedItem!.ToString()!);
                requete = requete.Where(r => r.Statut == statut);
            }

            grille.DataSource = requete
                .OrderByDescending(r => r.DateDebut)
                .AsEnumerable()
                .Select(r => new
                {
                    r.Id,
                    Client = r.Client!.NomComplet,
                    Véhicule = r.Vehicule!.Immatriculation,
                    Place = r.Place!.Numero,
                    Début = r.DateDebut.ToString("dd/MM/yyyy HH:mm"),
                    Fin = r.DateFin.ToString("dd/MM/yyyy HH:mm"),
                    Montant = MontantTexte(r),
                    Statut = r.Statut.ToString(),
                    Paiement = r.Paiement?.Statut ?? "—"
                })
                .ToList();
        }

        private static string MontantTexte(Reservation r)
        {
            try { return r.CalculerMontant().ToString("0.00 €"); }
            catch (InvalidOperationException) { return "pas de tarif"; }
        }

        private Reservation? ReservationSelectionnee()
        {
            int? id = Theme.IdSelectionne(grille);
            return id == null ? null : db.Reservations.Find(id);
        }

        private void AfficherReservationSelectionnee()
        {
            var r = ReservationSelectionnee();
            if (r == null) return;

            cmbClient.SelectedItem = r.Client;
            cmbVehicule.SelectedItem = r.Vehicule;
            cmbPlace.SelectedItem = r.Place;
            dtDebut.Value = r.DateDebut;
            dtFin.Value = r.DateFin;
            if (r.Paiement != null) cmbMode.SelectedItem = r.Paiement.Mode;
        }

        private void MettreAJourMontant()
        {
            if (cmbPlace.SelectedItem is not Place place || place.Parking == null)
            {
                lblMontant.Text = "";
                return;
            }
            try
            {
                double heures = (dtFin.Value - dtDebut.Value).TotalHours;
                double prix = place.Parking.GetTarif(place.Type).CalculerPrix(heures);
                lblMontant.Text = $"Montant estimé : {prix:0.00} €";
            }
            catch (InvalidOperationException ex)
            {
                lblMontant.Text = ex.Message;
            }
        }


        

        // =====================================================================
        //  Actions
        // =====================================================================

        private void Reserver()
        {
            if (cmbClient.SelectedItem is not Client client || cmbPlace.SelectedItem is not Place place)
            {
                Theme.Erreur("Choisissez un client et une place.");
                return;
            }

            try
            {

                // Méthode métier : vérifie les dates, le véhicule et la disponibilité de la place

                var r = client.Reserver(place, dtDebut.Value, dtFin.Value, (cmbVehicule.SelectedItem as Vehicule)!);
                db.SaveChanges();
                Theme.Info($"Réservation #{r.Id} créée pour la place {place.Numero}.\nMontant : {r.CalculerMontant():0.00} €");
                ChargerReservations();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                Theme.Erreur(ex.Message);
            }
        }

        private void ModifierReservation()
        {
            var r = ReservationSelectionnee();
            if (r == null) { Theme.Erreur("Sélectionnez d'abord une réservation."); return; }
            if (r.Statut != StatutReservation.EN_ATTENTE)
            {
                Theme.Erreur("Seule une réservation EN_ATTENTE (non payée) peut être modifiée.");
                return;
            }
            if (cmbPlace.SelectedItem is not Place place || cmbVehicule.SelectedItem is not Vehicule vehicule)
            {
                Theme.Erreur("Choisissez une place et un véhicule.");
                return;
            }

            DateTime debut = dtDebut.Value, fin = dtFin.Value;
            if (fin <= debut) { Theme.Erreur("La date de fin doit être après la date de début."); return; }


            bool chevauche = place.Reservations.Any(x =>
                x.Id != r.Id
                && (x.Statut == StatutReservation.EN_ATTENTE || x.Statut == StatutReservation.CONFIRMEE)
                && debut < x.DateFin && fin > x.DateDebut);
            if (chevauche) { Theme.Erreur($"La place {place.Numero} n'est pas disponible sur ce créneau."); return; }

            r.Place = place;
            r.Vehicule = vehicule;
            r.DateDebut = debut;
            r.DateFin = fin;
            db.SaveChanges();
            ChargerReservations();
        }

        private void Payer()
        {
            var r = ReservationSelectionnee();
            if (r == null) { Theme.Erreur("Sélectionnez d'abord une réservation."); return; }

            try
            {
                Paiement p = r.Payer((ModePaiement)cmbMode.SelectedItem!);
                db.SaveChanges();
                Theme.Info(p.GenererRecu());
                ChargerReservations();
            }
            catch (InvalidOperationException ex)
            {
                Theme.Erreur(ex.Message);
            }
        }

        private void AnnulerReservation()
        {
            var r = ReservationSelectionnee();
            if (r == null) { Theme.Erreur("Sélectionnez d'abord une réservation."); return; }
            if (!Theme.Confirmer($"Annuler la réservation #{r.Id} ?")) return;

            try
            {


                r.Annuler();   // libère la place et rembourse si déjà payée

                db.SaveChanges();
                string rembourse = r.Paiement?.Statut == "REMBOURSE" ? $"\n{r.Paiement.Montant:0.00} € remboursés." : "";
                Theme.Info($"Réservation #{r.Id} annulée.{rembourse}");
                ChargerReservations();
            }
            catch (InvalidOperationException ex)
            {
                Theme.Erreur(ex.Message);
            }
        }

        private void SupprimerReservation()
        {
            var r = ReservationSelectionnee();
            if (r == null) { Theme.Erreur("Sélectionnez d'abord une réservation."); return; }
            if (!Theme.Confirmer($"Supprimer définitivement la réservation #{r.Id} (et son paiement) ?")) return;

            if (r.Statut == StatutReservation.CONFIRMEE)
                r.Place?.Liberer();
            db.Reservations.Remove(r);
            db.SaveChanges();
            ChargerReservations();
        }

        private void Nouveau()
        {
            grille.ClearSelection();
            dtDebut.Value = DateTime.Today.AddDays(1).AddHours(9);
            dtFin.Value = dtDebut.Value.AddHours(3);
            MettreAJourMontant();
        }
    }

