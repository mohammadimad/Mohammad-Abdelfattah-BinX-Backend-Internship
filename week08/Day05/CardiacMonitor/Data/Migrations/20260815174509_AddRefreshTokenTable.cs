using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardiacMonitor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "doctor-id-123",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "39143f1f-32af-417e-a9bb-4677a98a05d4", "AQAAAAIAAYagAAAAEBL5yogH/gmLUuVjFuqsUXxhg44x5V+Bm0k68Imyx0vwy0S57ZiBNc/X1G1prBobvA==", "751dbe44-2075-46c7-8161-ca9b538ddb3f" });

            migrationBuilder.UpdateData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 1,
                column: "RecordedAt",
                value: new DateTime(2026, 8, 15, 15, 45, 5, 747, DateTimeKind.Utc).AddTicks(6804));

            migrationBuilder.UpdateData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 2,
                column: "RecordedAt",
                value: new DateTime(2026, 8, 15, 16, 45, 5, 747, DateTimeKind.Utc).AddTicks(6812));

            migrationBuilder.UpdateData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 3,
                column: "RecordedAt",
                value: new DateTime(2026, 8, 15, 17, 15, 5, 747, DateTimeKind.Utc).AddTicks(6814));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "doctor-id-123",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2dbf5bcc-2a1f-4b5c-9b64-8329cc8c18bb", "AQAAAAIAAYagAAAAEHSbb8SBz+O1DR2vwExT8Q7ITqGT5h2aBwm+E1YI96s7MnvovlUrVaUsrg+/t1VdWA==", "70fb2761-d275-4161-994e-8f1c355082fa" });

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
        }
    }
}
