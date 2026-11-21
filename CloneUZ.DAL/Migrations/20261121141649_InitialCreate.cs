using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloneUZ.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    City = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Trains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AvgSpeed = table.Column<double>(type: "double precision", nullable: false),
                    MoneyPerMeter = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RouteStations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StationCode = table.Column<int>(type: "integer", nullable: false),
                    StationOrder = table.Column<int>(type: "integer", nullable: false),
                    ArrivalTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DepartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    RoutesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteStations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteStations_Routes_RoutesId",
                        column: x => x.RoutesId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteStations_Stations_StationCode",
                        column: x => x.StationCode,
                        principalTable: "Stations",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainId = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    DelayMinuts = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainRuns_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainRuns_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RouteStations_RoutesId",
                table: "RouteStations",
                column: "RoutesId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStations_StationCode",
                table: "RouteStations",
                column: "StationCode");

            migrationBuilder.CreateIndex(
                name: "IX_TrainRuns_RouteId",
                table: "TrainRuns",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainRuns_TrainId",
                table: "TrainRuns",
                column: "TrainId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteStations");

            migrationBuilder.DropTable(
                name: "TrainRuns");

            migrationBuilder.DropTable(
                name: "Stations");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Trains");
        }
    }
}
