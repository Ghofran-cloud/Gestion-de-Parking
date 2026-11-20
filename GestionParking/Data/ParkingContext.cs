using GestionParking.Enums;
using GestionParking.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionParking.Data
{
    /// <summary>
    /// Le DbContext fait le lien entre nos classes C# et la base SQLite (approche Code First).
    /// Chaque DbSet correspond à une table.
    /// </summary>
    public class ParkingContext : DbContext
    {
        public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Administrateur> Administrateurs => Set<Administrateur>();
        public DbSet<Vehicule> Vehicules => Set<Vehicule>();
        public DbSet<Parking> Parkings => Set<Parking>();
        public DbSet<Place> Places => Set<Place>();
        public DbSet<Tarif> Tarifs => Set<Tarif>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<Paiement> Paiements => Set<Paiement>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // Le fichier parking.db est créé dans le dossier du projet par "Update-Database"
            options.UseSqlite("Data Source=parking.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ----- Utilisateurs (Client + Administrateur dans la même table) -----
            modelBuilder.Entity<Utilisateur>().Ignore(u => u.EstConnecte);
            modelBuilder.Entity<Utilisateur>().HasIndex(u => u.Email).IsUnique();

            // ----- Véhicule : immatriculation unique -----
            modelBuilder.Entity<Vehicule>().HasIndex(v => v.Immatriculation).IsUnique();

            // ----- Place : numéro unique dans un même parking -----
            modelBuilder.Entity<Place>().HasIndex(p => new { p.ParkingId, p.Numero }).IsUnique();

            // ----- Parking géré par un administrateur (facultatif) -----
            modelBuilder.Entity<Parking>()
                .HasOne(p => p.Administrateur)
                .WithMany(a => a.Parkings)
                .HasForeignKey(p => p.AdministrateurId)
                .OnDelete(DeleteBehavior.SetNull);

            // ----- Reservation 1 --- 0..1 Paiement -----
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Paiement)
                .WithOne(p => p.Reservation)
                .HasForeignKey<Paiement>(p => p.ReservationId);

            // ----- Les enums sont enregistrés en texte (plus lisible dans la base) -----
            modelBuilder.Entity<Place>().Property(p => p.Statut).HasConversion<string>();
            modelBuilder.Entity<Reservation>().Property(r => r.Statut).HasConversion<string>();
            modelBuilder.Entity<Paiement>().Property(p => p.Mode).HasConversion<string>();

            // ----- Données de départ (insérées par la migration) -----
            modelBuilder.Entity<Administrateur>().HasData(
                new Administrateur { Id = 1, Nom = "Martin", Prenom = "Sophie", Email = "admin@parking.fr", MotDePasse = "admin123", Role = "SuperAdmin" });

            modelBuilder.Entity<Client>().HasData(
                new Client { Id = 2, Nom = "Ben Ali", Prenom = "Ghofran", Email = "ghofran@mail.com", MotDePasse = "1234", Telephone = "0600000000" },
                new Client { Id = 3, Nom = "Trabelsi", Prenom = "Mariem", Email = "mariem@mail.com", MotDePasse = "1234", Telephone = "0611111111" },
                new Client { Id = 4, Nom = "Gharbi", Prenom = "Yasmine", Email = "yasmine@mail.com", MotDePasse = "1234", Telephone = "0622222222" });

            modelBuilder.Entity<Vehicule>().HasData(
                new Vehicule { Id = 1, Immatriculation = "AB-123-CD", Marque = "Peugeot", Modele = "208", Type = "Voiture", ClientId = 2 },
                new Vehicule { Id = 2, Immatriculation = "EF-456-GH", Marque = "Renault", Modele = "Zoe", Type = "Voiture", ClientId = 3 },
                new Vehicule { Id = 3, Immatriculation = "IJ-789-KL", Marque = "Yamaha", Modele = "MT-07", Type = "Moto", ClientId = 4 });

            modelBuilder.Entity<Parking>().HasData(
                new Parking { Id = 1, Nom = "Parking Centre", Adresse = "12 rue de la Gare", Capacite = 20, AdministrateurId = 1 });

            modelBuilder.Entity<Tarif>().HasData(
                new Tarif { Id = 1, TypePlace = "Standard", PrixHeure = 2.5, PrixJour = 20, ParkingId = 1 },
                new Tarif { Id = 2, TypePlace = "Electrique", PrixHeure = 3.0, PrixJour = 25, ParkingId = 1 },
                new Tarif { Id = 3, TypePlace = "Moto", PrixHeure = 1.0, PrixJour = 8, ParkingId = 1 });

            modelBuilder.Entity<Place>().HasData(
                new Place { Id = 1, Numero = "A01", Etage = 0, Type = "Standard", ParkingId = 1 },
                new Place { Id = 2, Numero = "A02", Etage = 0, Type = "Standard", ParkingId = 1 },
                new Place { Id = 3, Numero = "A03", Etage = 0, Type = "Standard", ParkingId = 1 },
                new Place { Id = 4, Numero = "A04", Etage = 0, Type = "Moto", ParkingId = 1 },
                new Place { Id = 5, Numero = "B01", Etage = 1, Type = "Electrique", ParkingId = 1 },
                new Place { Id = 6, Numero = "B02", Etage = 1, Type = "Electrique", ParkingId = 1 },
                new Place { Id = 7, Numero = "B03", Etage = 1, Type = "Standard", ParkingId = 1 },
                new Place { Id = 8, Numero = "B04", Etage = 1, Type = "Standard", ParkingId = 1, Statut = StatutPlace.HORS_SERVICE });
        }
    }
}
