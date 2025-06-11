using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TourPlanner.RestServer.Migrations
{
    /// <inheritdoc />
    public partial class AddTourCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tours",
                columns: table => new
                {
                    TourId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TourName = table.Column<string>(type: "text", nullable: false),
                    TourDescription = table.Column<string>(type: "text", nullable: false),
                    StartLocation = table.Column<string>(type: "text", nullable: false),
                    EndLocation = table.Column<string>(type: "text", nullable: false),
                    TransportationType = table.Column<int>(type: "integer", nullable: false),
                    Distance = table.Column<double>(type: "double precision", nullable: false),
                    EstimatedTime = table.Column<float>(type: "real", nullable: false),
                    StartLat = table.Column<double>(type: "double precision", nullable: false),
                    StartLon = table.Column<double>(type: "double precision", nullable: false),
                    EndLat = table.Column<double>(type: "double precision", nullable: false),
                    EndLon = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tours", x => x.TourId);
                });

            migrationBuilder.CreateTable(
                name: "TourLogs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    DistanceTraveled = table.Column<float>(type: "real", nullable: false),
                    TimeTaken = table.Column<float>(type: "real", nullable: false),
                    Rating = table.Column<float>(type: "real", nullable: false),
                    TourId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_TourLogs_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "TourId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TourLogs_TourId",
                table: "TourLogs",
                column: "TourId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourLogs");

            migrationBuilder.DropTable(
                name: "Tours");
        }
    }
}
