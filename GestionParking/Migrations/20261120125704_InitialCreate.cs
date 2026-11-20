using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestionParking.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    Prenom = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    MotDePasse = table.Column<string>(type: "TEXT", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: true),
                    Telephone = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parkings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    Adresse = table.Column<string>(type: "TEXT", nullable: false),
                    Capacite = table.Column<int>(type: "INTEGER", nullable: false),
                    AdministrateurId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parkings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parkings_Utilisateurs_AdministrateurId",
                        column: x => x.AdministrateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Vehicules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Immatriculation = table.Column<string>(type: "TEXT", nullable: false),
                    Marque = table.Column<string>(type: "TEXT", nullable: false),
                    Modele = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicules_Utilisateurs_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Places",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    Etage = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Statut = table.Column<string>(type: "TEXT", nullable: false),
                    ParkingId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Places", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Places_Parkings_ParkingId",
                        column: x => x.ParkingId,
                        principalTable: "Parkings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tarifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TypePlace = table.Column<string>(type: "TEXT", nullable: false),
                    PrixHeure = table.Column<double>(type: "REAL", nullable: false),
                    PrixJour = table.Column<double>(type: "REAL", nullable: false),
                    ParkingId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarifs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tarifs_Parkings_ParkingId",
                        column: x => x.ParkingId,
                        principalTable: "Parkings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateDebut = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateFin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Statut = table.Column<string>(type: "TEXT", nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlaceId = table.Column<int>(type: "INTEGER", nullable: false),
                    VehiculeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Places_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_Utilisateurs_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_Vehicules_VehiculeId",
                        column: x => x.VehiculeId,
                        principalTable: "Vehicules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paiements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Montant = table.Column<double>(type: "REAL", nullable: false),
                    DatePaiement = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Mode = table.Column<string>(type: "TEXT", nullable: false),
                    Statut = table.Column<string>(type: "TEXT", nullable: false),
                    ReservationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paiements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paiements_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Utilisateurs",
                columns: new[] { "Id", "Discriminator", "Email", "MotDePasse", "Nom", "Prenom", "Role" },
                values: new object[] { 1, "Administrateur", "admin@parking.fr", "admin123", "Martin", "Sophie", "SuperAdmin" });

            migrationBuilder.InsertData(
                table: "Utilisateurs",
                columns: new[] { "Id", "Discriminator", "Email", "MotDePasse", "Nom", "Prenom", "Telephone" },
                values: new object[,]
                {
                    { 2, "Client", "ghofran@mail.com", "1234", "Ben Ali", "Ghofran", "0600000000" },
                    { 3, "Client", "mariem@mail.com", "1234", "Trabelsi", "Mariem", "0611111111" },
                    { 4, "Client", "yasmine@mail.com", "1234", "Gharbi", "Yasmine", "0622222222" }
                });

            migrationBuilder.InsertData(
                table: "Parkings",
                columns: new[] { "Id", "AdministrateurId", "Adresse", "Capacite", "Nom" },
                values: new object[] { 1, 1, "12 rue de la Gare", 20, "Parking Centre" });

            migrationBuilder.InsertData(
                table: "Vehicules",
                columns: new[] { "Id", "ClientId", "Immatriculation", "Marque", "Modele", "Type" },
                values: new object[,]
                {
                    { 1, 2, "AB-123-CD", "Peugeot", "208", "Voiture" },
                    { 2, 3, "EF-456-GH", "Renault", "Zoe", "Voiture" },
                    { 3, 4, "IJ-789-KL", "Yamaha", "MT-07", "Moto" }
                });

            migrationBuilder.InsertData(
                table: "Places",
                columns: new[] { "Id", "Etage", "Numero", "ParkingId", "Statut", "Type" },
                values: new object[,]
                {
                    { 1, 0, "A01", 1, "LIBRE", "Standard" },
                    { 2, 0, "A02", 1, "LIBRE", "Standard" },
                    { 3, 0, "A03", 1, "LIBRE", "Standard" },
                    { 4, 0, "A04", 1, "LIBRE", "Moto" },
                    { 5, 1, "B01", 1, "LIBRE", "Electrique" },
                    { 6, 1, "B02", 1, "LIBRE", "Electrique" },
                    { 7, 1, "B03", 1, "LIBRE", "Standard" },
                    { 8, 1, "B04", 1, "HORS_SERVICE", "Standard" }
                });

            migrationBuilder.InsertData(
                table: "Tarifs",
                columns: new[] { "Id", "ParkingId", "PrixHeure", "PrixJour", "TypePlace" },
                values: new object[,]
                {
                    { 1, 1, 2.5, 20.0, "Standard" },
                    { 2, 1, 3.0, 25.0, "Electrique" },
                    { 3, 1, 1.0, 8.0, "Moto" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_ReservationId",
                table: "Paiements",
                column: "ReservationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parkings_AdministrateurId",
                table: "Parkings",
                column: "AdministrateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Places_ParkingId_Numero",
                table: "Places",
                columns: new[] { "ParkingId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ClientId",
                table: "Reservations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PlaceId",
                table: "Reservations",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_VehiculeId",
                table: "Reservations",
                column: "VehiculeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarifs_ParkingId",
                table: "Tarifs",
                column: "ParkingId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Email",
                table: "Utilisateurs",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicules_ClientId",
                table: "Vehicules",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicules_Immatriculation",
                table: "Vehicules",
                column: "Immatriculation",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Paiements");

            migrationBuilder.DropTable(
                name: "Tarifs");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Places");

            migrationBuilder.DropTable(
                name: "Vehicules");

            migrationBuilder.DropTable(
                name: "Parkings");

            migrationBuilder.DropTable(
                name: "Utilisateurs");
        }
    }
}
