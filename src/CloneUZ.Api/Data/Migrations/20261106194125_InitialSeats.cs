using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CloneUZ.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSeats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Seats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    WagonNumber = table.Column<int>(type: "integer", nullable: false),
                    SeatNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReservedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReservedUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TripWagons",
                columns: table => new
                {
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripWagons", x => new { x.TripId, x.Number });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Seats_ReservedUntil",
                table: "Seats",
                column: "ReservedUntil");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_TripId_WagonNumber_SeatNumber",
                table: "Seats",
                columns: new[] { "TripId", "WagonNumber", "SeatNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Seats");

            migrationBuilder.DropTable(
                name: "TripWagons");
        }
    }
}
