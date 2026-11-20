using System;
using System.Collections.Generic;
using System.Linq;
using GestionParking.Enums;

namespace GestionParking.Models
{
    public class Parking
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public string Adresse { get; set; } = "";
        public int Capacite { get; set; }

        
        public int? AdministrateurId { get; set; }
        public Administrateur? Administrateur { get; set; }

        public List<Place> Places { get; set; } = new List<Place>();

        public List<Tarif> Tarifs { get; set; } = new List<Tarif>();

        public void AjouterPlace(Place place)
        {
            if (Places.Count >= Capacite)
                throw new InvalidOperationException($"Capacité maximale ({Capacite}) atteinte pour {Nom}.");
            if (Places.Any(p => p.Numero == place.Numero))
                throw new InvalidOperationException($"La place {place.Numero} existe déjà.");

            place.Parking = this;
            Places.Add(place);
        }

        public void AjouterOuRemplacerTarif(Tarif tarif)
        {
            Tarifs.RemoveAll(t => t.TypePlace == tarif.TypePlace);
            Tarifs.Add(tarif);
        }

        public Tarif GetTarif(string typePlace)
        {
            return Tarifs.FirstOrDefault(t => t.TypePlace == typePlace)
                ?? throw new InvalidOperationException($"Aucun tarif défini pour le type '{typePlace}'.");
        }

        public List<Place> GetPlacesDisponibles() =>
            Places.Where(p => p.Statut == StatutPlace.LIBRE).ToList();

        public int GetNombrePlacesLibres() =>
            Places.Count(p => p.Statut == StatutPlace.LIBRE);

        
        public double GetTauxOccupation()
        {
            int enService = Places.Count(p => p.Statut != StatutPlace.HORS_SERVICE);
            if (enService == 0) return 0;
            int prises = Places.Count(p => p.Statut == StatutPlace.OCCUPEE || p.Statut == StatutPlace.RESERVEE);
            return (double)prises / enService;
        }

        public override string ToString() => Nom;
    }
}
