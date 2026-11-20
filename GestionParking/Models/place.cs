using System;
using System.Collections.Generic;
using System.Linq;
using GestionParking.Enums;

namespace GestionParking.Models
{
    public class Place
    {
        public int Id { get; set; }                 
        public string Numero { get; set; } = "";
        public int Etage { get; set; }
        public string Type { get; set; } = "";
        public StatutPlace Statut { get; set; } = StatutPlace.LIBRE;

       
        public int ParkingId { get; set; }
        public Parking? Parking { get; set; }

        
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();

       
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
