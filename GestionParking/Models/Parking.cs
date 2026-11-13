using System;
using System.Collections.Generic;
using System.Linq;
using GestionParking.Enums;

namespace GestionParking.Models
{
    public class Parking
    {
        private int id;
        private string nom;
        private string adresse;
        private int capacite;

        // Parking 1 ◆--- 1..* Place (composition : contient)
        private readonly List<Place> places = new List<Place>();

        // Parking 1 ◇--- 1..* Tarif (agrégation : applique)
        private readonly List<Tarif> tarifs = new List<Tarif>();

        public Parking(int id, string nom, string adresse, int capacite)
        {
            this.id = id;
            this.nom = nom;
            this.adresse = adresse;
            this.capacite = capacite;
        }

        public int Id => id;
        public string Nom { get => nom; set => nom = value; }
        public string Adresse { get => adresse; set => adresse = value; }
        public int Capacite => capacite;
        public IReadOnlyList<Place> Places => places;
        public IReadOnlyList<Tarif> Tarifs => tarifs;

        public void AjouterPlace(Place place)
        {
            if (places.Count >= capacite)
                throw new InvalidOperationException($"Capacité maximale ({capacite}) atteinte pour {nom}.");
            if (places.Any(p => p.Numero == place.Numero))
                throw new InvalidOperationException($"La place {place.Numero} existe déjà.");

            place.Parking = this;
            places.Add(place);
        }

        public void AjouterOuRemplacerTarif(Tarif tarif)
        {
            tarifs.RemoveAll(t => t.TypePlace == tarif.TypePlace);
            tarifs.Add(tarif);
        }

        public Tarif GetTarif(string typePlace)
        {
            return tarifs.FirstOrDefault(t => t.TypePlace == typePlace)
                ?? throw new InvalidOperationException($"Aucun tarif défini pour le type '{typePlace}'.");
        }

        public List<Place> GetPlacesDisponibles()
        {
            return places.Where(p => p.Statut == StatutPlace.LIBRE).ToList();
        }

        public int GetNombrePlacesLibres()
        {
            return places.Count(p => p.Statut == StatutPlace.LIBRE);
        }

        /// <summary>Taux d'occupation entre 0 et 1 (places réservées ou occupées / places en service).</summary>
        public double GetTauxOccupation()
        {
            var enService = places.Count(p => p.Statut != StatutPlace.HORS_SERVICE);
            if (enService == 0) return 0;
            var prises = places.Count(p => p.Statut == StatutPlace.OCCUPEE || p.Statut == StatutPlace.RESERVEE);
            return (double)prises / enService;
        }

        public override string ToString() => $"{nom} - {adresse} ({places.Count}/{capacite} places)";
    }
}

