using System;
using GestionParking.Enums;

namespace GestionParking.Models
{
    public class Reservation
    {
        private static int compteur = 0;

        private int id;
        private DateTime dateDebut;
        private DateTime dateFin;
        private StatutReservation statut;

        public Reservation(DateTime dateDebut, DateTime dateFin, Client client, Place place, Vehicule vehicule)
        {
            id = ++compteur;
            this.dateDebut = dateDebut;
            this.dateFin = dateFin;
            statut = StatutReservation.EN_ATTENTE;

            Client = client;      
            Place = place;        
            Vehicule = vehicule;  

            place.AjouterReservation(this);
        }

        public int Id => id;
        public DateTime DateDebut => dateDebut;
        public DateTime DateFin => dateFin;
        public StatutReservation Statut => statut;

        public Client Client { get; }
        public Place Place { get; }
        public Vehicule Vehicule { get; }

        
        public Paiement Paiement { get; private set; }

        public void Confirmer()
        {
            if (statut != StatutReservation.EN_ATTENTE)
                throw new InvalidOperationException($"Impossible de confirmer une réservation {statut}.");
            statut = StatutReservation.CONFIRMEE;
            Place.Statut = StatutPlace.RESERVEE;
            Console.WriteLine($"Réservation #{id} confirmée.");
        }

        public void Annuler()
        {
            if (statut == StatutReservation.ANNULEE || statut == StatutReservation.TERMINEE)
                throw new InvalidOperationException($"La réservation #{id} est déjà {statut}.");

            statut = StatutReservation.ANNULEE;
            Place.Liberer();
            if (Paiement != null && Paiement.Statut == "EFFECTUE")
                Paiement.Rembourser();
            Console.WriteLine($"Réservation #{id} annulée.");
        }

        public void Terminer()
        {
            statut = StatutReservation.TERMINEE;
            Place.Liberer();
        }

        public double CalculerMontant()
        {
            double dureeHeures = (dateFin - dateDebut).TotalHours;
            Tarif tarif = Place.Parking.GetTarif(Place.Type);
            return tarif.CalculerPrix(dureeHeures);
        }

        
        public Paiement Payer(ModePaiement mode)
        {
            if (Paiement != null && Paiement.Statut == "EFFECTUE")
                throw new InvalidOperationException("Cette réservation est déjà payée.");

            Paiement = new Paiement(CalculerMontant(), mode, this);
            if (Paiement.Effectuer() && statut == StatutReservation.EN_ATTENTE)
                Confirmer();
            return Paiement;
        }

        public override string ToString() =>
            $"Réservation #{id} | {Place.Numero} | {dateDebut:dd/MM HH:mm} → {dateFin:dd/MM HH:mm} | {statut}";
    }
}