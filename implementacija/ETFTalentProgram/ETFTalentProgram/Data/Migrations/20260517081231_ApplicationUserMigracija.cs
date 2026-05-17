using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETFTalentProgram.Data.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationUserMigracija : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Administratori_Korisnici_Id",
                table: "Administratori");

            migrationBuilder.DropForeignKey(
                name: "FK_Firme_Korisnici_Id",
                table: "Firme");

            migrationBuilder.DropForeignKey(
                name: "FK_Ponude_Korisnici_PosiljalacId",
                table: "Ponude");

            migrationBuilder.DropForeignKey(
                name: "FK_Ponude_Korisnici_PrimalacId",
                table: "Ponude");

            migrationBuilder.DropForeignKey(
                name: "FK_Referenti_Korisnici_Id",
                table: "Referenti");

            migrationBuilder.DropForeignKey(
                name: "FK_Studenti_Korisnici_Id",
                table: "Studenti");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Korisnici",
                table: "Korisnici");

            migrationBuilder.RenameTable(
                name: "Korisnici",
                newName: "Korisnik");

            migrationBuilder.AddColumn<DateTime>(
                name: "DatumRegistracije",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatumZadnjePrijave",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Uloga",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Korisnik",
                table: "Korisnik",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Administratori_Korisnik_Id",
                table: "Administratori",
                column: "Id",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Firme_Korisnik_Id",
                table: "Firme",
                column: "Id",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ponude_Korisnik_PosiljalacId",
                table: "Ponude",
                column: "PosiljalacId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ponude_Korisnik_PrimalacId",
                table: "Ponude",
                column: "PrimalacId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Referenti_Korisnik_Id",
                table: "Referenti",
                column: "Id",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Studenti_Korisnik_Id",
                table: "Studenti",
                column: "Id",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Administratori_Korisnik_Id",
                table: "Administratori");

            migrationBuilder.DropForeignKey(
                name: "FK_Firme_Korisnik_Id",
                table: "Firme");

            migrationBuilder.DropForeignKey(
                name: "FK_Ponude_Korisnik_PosiljalacId",
                table: "Ponude");

            migrationBuilder.DropForeignKey(
                name: "FK_Ponude_Korisnik_PrimalacId",
                table: "Ponude");

            migrationBuilder.DropForeignKey(
                name: "FK_Referenti_Korisnik_Id",
                table: "Referenti");

            migrationBuilder.DropForeignKey(
                name: "FK_Studenti_Korisnik_Id",
                table: "Studenti");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Korisnik",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "DatumRegistracije",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DatumZadnjePrijave",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Uloga",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "Korisnik",
                newName: "Korisnici");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Korisnici",
                table: "Korisnici",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Administratori_Korisnici_Id",
                table: "Administratori",
                column: "Id",
                principalTable: "Korisnici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Firme_Korisnici_Id",
                table: "Firme",
                column: "Id",
                principalTable: "Korisnici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ponude_Korisnici_PosiljalacId",
                table: "Ponude",
                column: "PosiljalacId",
                principalTable: "Korisnici",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ponude_Korisnici_PrimalacId",
                table: "Ponude",
                column: "PrimalacId",
                principalTable: "Korisnici",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Referenti_Korisnici_Id",
                table: "Referenti",
                column: "Id",
                principalTable: "Korisnici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Studenti_Korisnici_Id",
                table: "Studenti",
                column: "Id",
                principalTable: "Korisnici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
