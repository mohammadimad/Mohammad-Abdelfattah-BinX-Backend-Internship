using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardiacMonitor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "doctor-id-123", 0, "2dbf5bcc-2a1f-4b5c-9b64-8329cc8c18bb", "doctor@cardiac.com", true, false, null, "DOCTOR@CARDIAC.COM", "DOCTOR@CARDIAC.COM", "AQAAAAIAAYagAAAAEHSbb8SBz+O1DR2vwExT8Q7ITqGT5h2aBwm+E1YI96s7MnvovlUrVaUsrg+/t1VdWA==", null, false, "70fb2761-d275-4161-994e-8f1c355082fa", false, "doctor@cardiac.com" });

            migrationBuilder.UpdateData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 1,
                column: "RecordedAt",
                value: new DateTime(2026, 8, 14, 16, 41, 31, 276, DateTimeKind.Utc).AddTicks(633));

            migrationBuilder.UpdateData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 2,
                column: "RecordedAt",
                value: new DateTime(2026, 8, 14, 17, 41, 31, 276, DateTimeKind.Utc).AddTicks(643));

            migrationBuilder.UpdateData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 3,
                column: "RecordedAt",
                value: new DateTime(2026, 8, 14, 18, 11, 31, 276, DateTimeKind.Utc).AddTicks(645));

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "b2c3d4e5-f67a-8b9c-0d1e-2f3a4b5c6d7e", "doctor-id-123" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "b2c3d4e5-f67a-8b9c-0d1e-2f3a4b5c6d7e", "doctor-id-123" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "doctor-id-123");

            migrationBuilder.UpdateData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 1,
                column: "RecordedAt",
                value: new DateTime(2026, 8, 14, 9, 44, 0, 371, DateTimeKind.Utc).AddTicks(1144));

            migrationBuilder.UpdateData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 2,
                column: "RecordedAt",
                value: new DateTime(2026, 8, 14, 10, 44, 0, 371, DateTimeKind.Utc).AddTicks(1154));

            migrationBuilder.UpdateData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 3,
                column: "RecordedAt",
                value: new DateTime(2026, 8, 14, 11, 14, 0, 371, DateTimeKind.Utc).AddTicks(1156));
        }
    }
}
