using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CardiacMonitor.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_UserId",
                table: "Patients");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Patients",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d", null, "Admin", "ADMIN" },
                    { "b2c3d4e5-f67a-8b9c-0d1e-2f3a4b5c6d7e", null, "Doctor", "DOCTOR" },
                    { "c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f", null, "Patient", "PATIENT" }
                });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "ContactNumber", "DateOfBirth", "FirstName", "Gender", "LastName", "UserId" },
                values: new object[,]
                {
                    { 1, "+9759835279", new DateTime(1990, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ahmad", "Male", "Amr", null },
                    { 2, "+970988271", new DateTime(1985, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sara", "Female", "Ali", null }
                });

            migrationBuilder.InsertData(
                table: "VitalSigns",
                columns: new[] { "Id", "DiastolicBP", "HeartRate", "OxygenSaturation", "PatientId", "RecordedAt", "SystolicBP" },
                values: new object[,]
                {
                    { 1, 80, 75, 98.5m, 1, new DateTime(2026, 8, 14, 9, 44, 0, 371, DateTimeKind.Utc).AddTicks(1144), 120 },
                    { 2, 82, 82, 97.0m, 1, new DateTime(2026, 8, 14, 10, 44, 0, 371, DateTimeKind.Utc).AddTicks(1154), 125 },
                    { 3, 75, 70, 99.0m, 2, new DateTime(2026, 8, 14, 11, 14, 0, 371, DateTimeKind.Utc).AddTicks(1156), 115 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_UserId",
                table: "Patients",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_UserId",
                table: "Patients");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f67a-8b9c-0d1e-2f3a4b5c6d7e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f");

            migrationBuilder.DeleteData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Patients",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_UserId",
                table: "Patients",
                column: "UserId",
                unique: true);
        }
    }
}
