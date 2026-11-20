using System;
using System.Collections.Generic;
using System.Linq;
using GestionParking.Enums;

namespace GestionParking.Models
{
    public class Place
    {
        public int Id { get; set; }                 // NOUVEAU : clé primaire
        public string Numero { get; set; } = "";
        public int Etage { get; set; }
        public string Type { get; set; } = "";
        public StatutPlace Statut { get; set; } = StatutPlace.LIBRE;

        // Parking 1 ◆--- 1..* Place : clé étrangère + navigation
        public int ParkingId { get; set; }
        public Parking? Parking { get; set; }

        // Place 1 --- 0..* Reservation (réserve)
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();

        /// <summary>
        /// Disponible si la place n'est pas hors service et si aucune réservation active
        /// (en attente ou confirmée) ne chevauche le créneau demandé.
        /// </summary>
        public bool EstDisponible(DateTime debut, DateTime fin)
        {
            if (Statut == StatutPlace.HORS_SERVICE) return false;

            return !Reservations.Any(r =>
                (r.Statut == StatutReservation.EN_ATTENTE || r.Statut == StatutReservation.CONFIRMEE)
                && debut < r.DateFin && fin > r.DateDebut);
        }

        public void Occuper()
        {
            if (Statut == StatutPlace.HORS_SERVICE)
                throw new InvalidOperationException($"La place {Numero} est hors service.");
            Statut = StatutPlace.OCCUPEE;
        }

        public void Liberer()
        {
            if (Statut != StatutPlace.HORS_SERVICE)
                Statut = StatutPlace.LIBRE;
        }

        public override string ToString() => $"{Numero} (étage {Etage}, {Type})";
    }
}
