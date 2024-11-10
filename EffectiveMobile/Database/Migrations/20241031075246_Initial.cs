using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EffectiveMobile.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "BaseDeliverySequence");

            migrationBuilder.CreateTable(
                name: "FilteredDeliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"BaseDeliverySequence\"')"),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    District = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilteredDeliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InitialDeliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"BaseDeliverySequence\"')"),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    District = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InitialDeliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LevelAsText = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    RenderedMessage = table.Column<string>(type: "text", nullable: false),
                    Properties = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilteredDeliveries");

            migrationBuilder.DropTable(
                name: "InitialDeliveries");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropSequence(
                name: "BaseDeliverySequence");
        }
    }
}
