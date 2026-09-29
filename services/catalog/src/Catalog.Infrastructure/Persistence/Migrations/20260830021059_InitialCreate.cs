using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "aircraft_types",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    model = table.Column<string>(type: "text", nullable: false),
                    seat_map_json = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "airlines",
                schema: "catalog",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airlines", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "airports",
                schema: "catalog",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    country = table.Column<string>(type: "text", nullable: false),
                    timezone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airports", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "flights",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    airline_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    flight_number = table.Column<int>(type: "integer", nullable: false),
                    origin_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    destination_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    aircraft_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    departure_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    arrival_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flights", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fare_classes",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cabin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    base_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    available_seats = table.Column<int>(type: "integer", nullable: false),
                    FlightId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fare_classes", x => x.id);
                    table.ForeignKey(
                        name: "FK_fare_classes_flights_FlightId",
                        column: x => x.FlightId,
                        principalSchema: "catalog",
                        principalTable: "flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fare_classes_FlightId",
                schema: "catalog",
                table: "fare_classes",
                column: "FlightId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aircraft_types",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "airlines",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "airports",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "fare_classes",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "flights",
                schema: "catalog");
        }
    }
}
