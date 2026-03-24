using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoultryFarmManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FeedingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyFeedingTimes",
                table: "Batches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FeedingTableId",
                table: "Batches",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FeedingTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedingTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedingTableDayEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedingTableId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    FoodType = table.Column<byte>(type: "smallint", nullable: false),
                    AmountPerBird = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitOfMeasure = table.Column<byte>(type: "smallint", nullable: false),
                    ExpectedBirdWeight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ExpectedBirdWeightUnitOfMeasure = table.Column<byte>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedingTableDayEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedingTableDayEntries_FeedingTables_FeedingTableId",
                        column: x => x.FeedingTableId,
                        principalTable: "FeedingTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Batches_FeedingTableId",
                table: "Batches",
                column: "FeedingTableId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedingTableDayEntries_FeedingTableId",
                table: "FeedingTableDayEntries",
                column: "FeedingTableId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedingTableDayEntries_FeedingTableId_DayNumber",
                table: "FeedingTableDayEntries",
                columns: new[] { "FeedingTableId", "DayNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedingTables_Name",
                table: "FeedingTables",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Batches_FeedingTables_FeedingTableId",
                table: "Batches",
                column: "FeedingTableId",
                principalTable: "FeedingTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batches_FeedingTables_FeedingTableId",
                table: "Batches");

            migrationBuilder.DropTable(
                name: "FeedingTableDayEntries");

            migrationBuilder.DropTable(
                name: "FeedingTables");

            migrationBuilder.DropIndex(
                name: "IX_Batches_FeedingTableId",
                table: "Batches");

            migrationBuilder.DropColumn(
                name: "DailyFeedingTimes",
                table: "Batches");

            migrationBuilder.DropColumn(
                name: "FeedingTableId",
                table: "Batches");
        }
    }
}
