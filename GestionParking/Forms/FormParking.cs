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
    
    public class FormParking : Form
    {
        private readonly ParkingContext db = new ParkingContext();

        private readonly ComboBox cmbParking = new ComboBox { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label lblInfos = new Label();
        private readonly DataGridView grillePlaces = new DataGridView();
        private readonly DataGridView grilleTarifs = new DataGridView();

        private readonly TextBox txtNumero = new TextBox();
        private readonly NumericUpDown numEtage = new NumericUpDown { Minimum = -5, Maximum = 20 };
        private readonly ComboBox cmbType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox cmbStatut = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

        private readonly TextBox txtTypeTarif = new TextBox { Width = 140 };
        private readonly NumericUpDown numPrixHeure = new NumericUpDown { Width = 90, DecimalPlaces = 2, Increment = 0.5M, Maximum = 1000 };
        private readonly NumericUpDown numPrixJour = new NumericUpDown { Width = 90, DecimalPlaces = 2, Increment = 1M, Maximum = 10000 };

        public FormParking()
        {
            Theme.AppliquerFenetre(this, "Gestion du parking", 1200, 760);
            ConstruireInterface();

            cmbParking.DataSource = db.Parkings.OrderBy(p => p.Nom).ToList();
            cmbParking.SelectedIndexChanged += (s, e) => ChargerTout();
            ChargerTout();

            FormClosed += (s, e) => db.Dispose();
        }

        
        private Parking? ParkingCourant()
        {
            if (cmbParking.SelectedItem is not Parking p) return null;
            return db.Parkings
                .Include(x => x.Places).ThenInclude(pl => pl.Reservations)
                .Include(x => x.Tarifs)
                .First(x => x.Id == p.Id);
        }

        private void ConstruireInterface()
        {
            var centre = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var barre = new Panel { Dock = DockStyle.Top, Height = 46 };
            barre.Controls.Add(new Label { Text = "Parking :", AutoSize = true, Location = new Point(0, 10) });
            cmbParking.Location = new Point(75, 6);
            barre.Controls.Add(cmbParking);
            lblInfos.AutoSize = true;
            lblInfos.ForeColor = Theme.Gris;
            lblInfos.Location = new Point(355, 10);
            barre.Controls.Add(lblInfos);

            Theme.StyliserGrille(grillePlaces);
            Theme.ColorerStatuts(grillePlaces);
            grillePlaces.SelectionChanged += (s, e) => AfficherPlaceSelectionnee();

            centre.Controls.Add(grillePlaces);
            centre.Controls.Add(ConstruireZoneTarifs());
            centre.Controls.Add(barre);
            Controls.Add(centre);

           
            var panneau = Theme.CreerPanneauDroite("Fiche place");
            cmbStatut.DataSource = Enum.GetValues(typeof(StatutPlace));
            int y = 60;
            Theme.AjouterChamp(panneau, "Numéro * (ex : A05)", txtNumero, ref y);
            Theme.AjouterChamp(panneau, "Étage", numEtage, ref y);
            Theme.AjouterChamp(panneau, "Type (selon les tarifs)", cmbType, ref y);
            Theme.AjouterChamp(panneau, "Statut", cmbStatut, ref y);

            var btnAjouter = Theme.CreerBouton("➕ Ajouter", Theme.Vert);
            var btnModifier = Theme.CreerBouton("✏ Modifier", Theme.Bleu);
            var btnSupprimer = Theme.CreerBouton("🗑 Supprimer", Theme.Rouge);
            var btnVider = Theme.CreerBouton("Nouveau", Theme.Gris);
            btnAjouter.Click += (s, e) => AjouterPlace();
            btnModifier.Click += (s, e) => ModifierPlace();
            btnSupprimer.Click += (s, e) => SupprimerPlace();
            btnVider.Click += (s, e) => { txtNumero.Clear(); numEtage.Value = 0; grillePlaces.ClearSelection(); txtNumero.Focus(); };
            Theme.CreerRangeeBoutons(panneau, y + 10, btnAjouter, btnModifier, btnSupprimer, btnVider);
            Controls.Add(panneau);

            Controls.Add(Theme.CreerEntete("🅿  Parking", "Places du parking (en haut) et tarifs par type de place (en bas)"));
        }

        private Panel ConstruireZoneTarifs()
        {
            var zone = new Panel { Dock = DockStyle.Bottom, Height = 240, Padding = new Padding(0, 12, 0, 0) };

            Theme.StyliserGrille(grilleTarifs);
            grilleTarifs.SelectionChanged += (s, e) => AfficherTarifSelectionne();

            var titre = new Label { Text = "Tarifs", Font = new Font("Segoe UI Semibold", 12F), Dock = DockStyle.Top, Height = 30 };

            var barre = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(0, 4, 0, 0) };
            barre.Controls.Add(PetitLabel("Type"));
            barre.Controls.Add(txtTypeTarif);
            barre.Controls.Add(PetitLabel("€ / heure"));
            barre.Controls.Add(numPrixHeure);
            barre.Controls.Add(PetitLabel("€ / jour"));
            barre.Controls.Add(numPrixJour);

            var btnAjouter = Theme.CreerBouton("➕", Theme.Vert, 44);
            var btnModifier = Theme.CreerBouton("✏", Theme.Bleu, 44);
            var btnSupprimer = Theme.CreerBouton("🗑", Theme.Rouge, 44);
            btnAjouter.Click += (s, e) => AjouterTarif();
            btnModifier.Click += (s, e) => ModifierTarif();
            btnSupprimer.Click += (s, e) => SupprimerTarif();
            barre.Controls.AddRange(new Control[] { btnAjouter, btnModifier, btnSupprimer });

            zone.Controls.Add(grilleTarifs);
            zone.Controls.Add(barre);
            zone.Controls.Add(titre);
            return zone;
        }

        private static Label PetitLabel(string texte) =>
            new Label { Text = texte, AutoSize = true, ForeColor = Theme.Gris, Margin = new Padding(6, 9, 2, 0) };

        private void ChargerTout()
        {
            var parking = ParkingCourant();
            if (parking == null) return;

            lblInfos.Text = $"{parking.Adresse}  ·  {parking.Places.Count}/{parking.Capacite} places  ·  " +
                            $"{parking.GetNombrePlacesLibres()} libres  ·  occupation {parking.GetTauxOccupation():P0}";

            grillePlaces.DataSource = parking.Places
                .OrderBy(p => p.Numero)
                .Select(p => new
                {
                    p.Id,
                    Numéro = p.Numero,
                    Étage = p.Etage,
                    p.Type,
                    Statut = p.Statut.ToString(),
                    Réservations = p.Reservations.Count(r => r.Statut == StatutReservation.EN_ATTENTE || r.Statut == StatutReservation.CONFIRMEE)
                })
                .ToList();

            grilleTarifs.DataSource = parking.Tarifs
                .OrderBy(t => t.TypePlace)
                .Select(t => new
                {
                    t.Id,
                    Type = t.TypePlace,
                    PrixHeure = t.PrixHeure.ToString("0.00 €"),
                    PrixJour = t.PrixJour.ToString("0.00 €"),
                    Places = parking.Places.Count(p => p.Type == t.TypePlace)
                })
                .ToList();

            cmbType.DataSource = parking.Tarifs.Select(t => t.TypePlace).OrderBy(t => t).ToList();
        }

        private Place? PlaceSelectionnee()
        {
            int? id = Theme.IdSelectionne(grillePlaces);
            return id == null ? null : db.Places.Find(id);
        }

        private Tarif? TarifSelectionne()
        {
            int? id = Theme.IdSelectionne(grilleTarifs);
            return id == null ? null : db.Tarifs.Find(id);
        }

        private void AfficherPlaceSelectionnee()
        {
            var place = PlaceSelectionnee();
            if (place == null) return;
            txtNumero.Text = place.Numero;
            numEtage.Value = place.Etage;
            cmbType.SelectedItem = place.Type;
            cmbStatut.SelectedItem = place.Statut;
        }

        private void AfficherTarifSelectionne()
        {
            var tarif = TarifSelectionne();
            if (tarif == null) return;
            txtTypeTarif.Text = tarif.TypePlace;
            numPrixHeure.Value = (decimal)tarif.PrixHeure;
            numPrixJour.Value = (decimal)tarif.PrixJour;
        }

       
        private void AjouterPlace()
        {
            var parking = ParkingCourant();
            if (parking == null) return;
            if (txtNumero.Text.Trim() == "" || cmbType.SelectedItem == null)
            {
                Theme.Erreur("Le numéro et le type sont obligatoires (ajoutez d'abord un tarif si la liste des types est vide).");
                return;
            }

            try
            {
                
                parking.AjouterPlace(new Place
                {
                    Numero = txtNumero.Text.Trim().ToUpper(),
                    Etage = (int)numEtage.Value,
                    Type = cmbType.SelectedItem.ToString()!,
                    Statut = (StatutPlace)cmbStatut.SelectedItem!
                });
                db.SaveChanges();
                ChargerTout();
            }
            catch (InvalidOperationException ex)
            {
                Theme.Erreur(ex.Message);
            }
        }

        private void ModifierPlace()
        {
            var parking = ParkingCourant();
            var place = PlaceSelectionnee();
            if (parking == null || place == null) { Theme.Erreur("Sélectionnez d'abord une place."); return; }

            string numero = txtNumero.Text.Trim().ToUpper();
            if (numero == "") { Theme.Erreur("Le numéro est obligatoire."); return; }
            if (parking.Places.Any(p => p.Numero == numero && p.Id != place.Id))
            {
                Theme.Erreur($"La place {numero} existe déjà.");
                return;
            }

            place.Numero = numero;
            place.Etage = (int)numEtage.Value;
            place.Type = cmbType.SelectedItem?.ToString() ?? place.Type;
            place.Statut = (StatutPlace)cmbStatut.SelectedItem!;
            db.SaveChanges();
            ChargerTout();
        }

        private void SupprimerPlace()
        {
            var place = PlaceSelectionnee();
            if (place == null) { Theme.Erreur("Sélectionnez d'abord une place."); return; }
            if (!Theme.Confirmer($"Supprimer la place {place.Numero} ?\nSes réservations seront aussi supprimées.")) return;

            db.Places.Remove(place);
            db.SaveChanges();
            ChargerTout();
        }

       
        private void AjouterTarif()
        {
            var parking = ParkingCourant();
            if (parking == null) return;

            string type = txtTypeTarif.Text.Trim();
            if (type == "") { Theme.Erreur("Le type de place est obligatoire."); return; }
            if (parking.Tarifs.Any(t => t.TypePlace.ToLower() == type.ToLower()))
            {
                Theme.Erreur($"Un tarif existe déjà pour le type '{type}'. Utilisez ✏ pour le modifier.");
                return;
            }

            parking.Tarifs.Add(new Tarif
            {
                TypePlace = type,
                PrixHeure = (double)numPrixHeure.Value,
                PrixJour = (double)numPrixJour.Value
            });
            db.SaveChanges();
            ChargerTout();
        }

        private void ModifierTarif()
        {
            var tarif = TarifSelectionne();
            if (tarif == null) { Theme.Erreur("Sélectionnez d'abord un tarif."); return; }

            tarif.PrixHeure = (double)numPrixHeure.Value;
            tarif.PrixJour = (double)numPrixJour.Value;
            db.SaveChanges();
            ChargerTout();
        }

        private void SupprimerTarif()
        {
            var parking = ParkingCourant();
            var tarif = TarifSelectionne();
            if (parking == null || tarif == null) { Theme.Erreur("Sélectionnez d'abord un tarif."); return; }

            if (parking.Places.Any(p => p.Type == tarif.TypePlace))
            {
                Theme.Erreur($"Impossible : des places de type '{tarif.TypePlace}' utilisent ce tarif.");
                return;
            }
            if (!Theme.Confirmer($"Supprimer le tarif '{tarif.TypePlace}' ?")) return;

            db.Tarifs.Remove(tarif);
            db.SaveChanges();
            ChargerTout();
        }
    }
}
