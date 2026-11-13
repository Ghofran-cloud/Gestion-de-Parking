using System;

namespace GestionParking.Models
{
    public class Tarif
    {
        private string typePlace;
        private double prixHeure;
        private double prixJour;

        public Tarif(string typePlace, double prixHeure, double prixJour)
        {
            this.typePlace = typePlace;
            this.prixHeure = prixHeure;
            this.prixJour = prixJour;
        }

        public string TypePlace => typePlace;
        public double PrixHeure { get => prixHeure; set => prixHeure = value; }
        public double PrixJour { get => prixJour; set => prixJour = value; }

        /// <summary>
        /// Calcule le prix : jours complets au prix journalier, puis heures restantes
        /// (arrondies à l'heure supérieure) plafonnées au prix d'une journée.
        /// </summary>
        public double CalculerPrix(double dureeHeures)
        {
            if (dureeHeures <= 0) return 0;

            int jours = (int)(dureeHeures / 24);
            double heuresRestantes = Math.Ceiling(dureeHeures - jours * 24);

            double prix = jours * prixJour + Math.Min(heuresRestantes * prixHeure, prixJour);
            return Math.Round(prix, 2);
        }

        public override string ToString() => $"{typePlace} : {prixHeure} €/h - {prixJour} €/jour";
    }
}
