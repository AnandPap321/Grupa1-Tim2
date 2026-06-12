using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETFTalentProgram.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFirmaProfilStatusVerifikacije : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusVerifikacije",
                table: "FirmaProfili",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusVerifikacije",
                table: "FirmaProfili");
        }
    }
}
