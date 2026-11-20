using System;
using GestionParking.Enums;

namespace GestionParking.Models
{
    public class Reservation
    {
        public int Id { get; set; }                 
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public StatutReservation Statut { get; set; } = StatutReservation.EN_ATTENTE;

        
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        
        public int PlaceId { get; set; }
        public Place? Place { get; set; }

        
        public int VehiculeId { get; set; }
        public Vehicule? Vehicule { get; set; }

        
        public Paiement? Paiement { get; set; }

        public void Confirmer()
        {
            if (Statut != StatutReservation.EN_ATTENTE)
                throw new InvalidOperationException($"Impossible de confirmer une réservation {Statut}.");
            Statut = StatutReservation.CONFIRMEE;
            if (Place != null) Place.Statut = StatutPlace.RESERVEE;
        }

        public void Annuler()
        {
            if (Statut == StatutReservation.ANNULEE || Statut == StatutReservation.TERMINEE)
                throw new InvalidOperationException($"La réservation #{Id} est déjà {Statut}.");

            Statut = StatutReservation.ANNULEE;
            Place?.Liberer();
            if (Paiement != null && Paiement.Statut == "EFFECTUE")
                Paiement.Rembourser();
        }

        public void Terminer()
        {
            Statut = StatutReservation.TERMINEE;
            Place?.Liberer();
        }

        
        public double CalculerMontant()
        {
            if (Place?.Parking == null) return 0;
            double dureeHeures = (DateFin - DateDebut).TotalHours;
            Tarif tarif = Place.Parking.GetTarif(Place.Type);
            return tarif.CalculerPrix(dureeHeures);
        }

        
        public Paiement Payer(ModePaiement mode)
        {
            if (Paiement != null && Paiement.Statut == "EFFECTUE")
                throw new InvalidOperationException("Cette réservation est déjà payée.");
            if (Statut == StatutReservation.ANNULEE || Statut == StatutReservation.TERMINEE)
                throw new InvalidOperationException($"Impossible de payer une réservation {Statut}.");

            Paiement = new Paiement { Montant = CalculerMontant(), Mode = mode, Reservation = this };
            if (Paiement.Effectuer() && Statut == StatutReservation.EN_ATTENTE)
                Confirmer();
            return Paiement;
        }

        public override string ToString() =>
            $"Réservation #{Id} | {Place?.Numero} | {DateDebut:dd/MM HH:mm} → {DateFin:dd/MM HH:mm} | {Statut}";
    }
}
