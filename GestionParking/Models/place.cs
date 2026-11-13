using System;
using System.Collections.Generic;
using System.Linq;
using GestionParking.Enums;

namespace GestionParking.Models
{
    public class Place
    {
        private string numero;
        private int etage;
        private string type;
        private StatutPlace statut;

        // Place 1 --- 0..* Reservation (réserve) : utile pour vérifier les chevauchements
        private readonly List<Reservation> reservations = new List<Reservation>();

        public Place(string numero, int etage, string type)
        {
            this.numero = numero;
            this.etage = etage;
            this.type = type;
            statut = StatutPlace.LIBRE;
        }

        public string Numero => numero;
        public int Etage => etage;
        public string Type { get => type; set => type = value; }
        public StatutPlace Statut { get => statut; set => statut = value; }

        /// <summary>Parking auquel appartient la place (renseigné par Parking.AjouterPlace).</summary>
        public Parking Parking { get; internal set; }

        public IReadOnlyList<Reservation> Reservations => reservations;

        internal void AjouterReservation(Reservation r) => reservations.Add(r);

        /// <summary>
        /// Une place est disponible si elle n'est pas hors service et si aucune réservation
        /// active (en attente ou confirmée) ne chevauche le créneau demandé.
        /// </summary>
        public bool EstDisponible(DateTime debut, DateTime fin)
        {
            if (statut == StatutPlace.HORS_SERVICE) return false;

            return !reservations.Any(r =>
                (r.Statut == StatutReservation.EN_ATTENTE || r.Statut == StatutReservation.CONFIRMEE)
                && debut < r.DateFin && fin > r.DateDebut);
        }

        public void Occuper()
        {
            if (statut == StatutPlace.HORS_SERVICE)
                throw new InvalidOperationException($"La place {numero} est hors service.");
            statut = StatutPlace.OCCUPEE;
        }

        public void Liberer()
        {
            if (statut != StatutPlace.HORS_SERVICE)
                statut = StatutPlace.LIBRE;
        }

        public override string ToString() => $"Place {numero} (étage {etage}, {type}) - {statut}";
    }
}
