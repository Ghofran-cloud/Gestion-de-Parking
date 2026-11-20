using System;

namespace GestionParking.Models
{
    public class Tarif
    {
        public int Id { get; set; }                
        public string TypePlace { get; set; } = "";
        public double PrixHeure { get; set; }
        public double PrixJour { get; set; }

        
        public int ParkingId { get; set; }
        public Parking? Parking { get; set; }

     
        public double CalculerPrix(double dureeHeures)
        {
            if (dureeHeures <= 0) return 0;

            int jours = (int)(dureeHeures / 24);
            double heuresRestantes = Math.Ceiling(dureeHeures - jours * 24);

            double prix = jours * PrixJour + Math.Min(heuresRestantes * PrixHeure, PrixJour);
            return Math.Round(prix, 2);
        }

        public override string ToString() => $"{TypePlace} : {PrixHeure} €/h - {PrixJour} €/jour";
    }
}
