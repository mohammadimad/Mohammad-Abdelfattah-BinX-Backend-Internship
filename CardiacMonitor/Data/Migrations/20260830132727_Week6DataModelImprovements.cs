using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardiacMonitor.Data.Migrations
{
    /// <inheritdoc />
    public partial class Week6DataModelImprovements : Migration
    {
        // Applies the Week 6 constraints, indexes, seed updates, and token table.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VitalSigns_PatientId",
                table: "VitalSigns");

            migrationBuilder.DropIndex(
                name: "IX_Medications_PatientId",
                table: "Medications");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ContactNumber",
                table: "Patients",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Medications",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Frequency",
                table: "Medications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Dosage",
                table: "Medications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Appointments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [RefreshTokens] (
                        [Id] int NOT NULL IDENTITY,
                        [Token] nvarchar(256) NOT NULL,
                        [JwtId] nvarchar(100) NOT NULL,
                        [IsUsed] bit NOT NULL,
                        [IsRevoked] bit NOT NULL,
                        [AddedDate] datetime2 NOT NULL,
                        [ExpiryDate] datetime2 NOT NULL,
                        [UserId] nvarchar(450) NOT NULL,
                        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId]
                            FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
                            ON DELETE CASCADE
                    );
                END
                ELSE
                BEGIN
                    ALTER TABLE [RefreshTokens] ALTER COLUMN [Token] nvarchar(256) NOT NULL;
                    ALTER TABLE [RefreshTokens] ALTER COLUMN [JwtId] nvarchar(100) NOT NULL;
                    ALTER TABLE [RefreshTokens] ALTER COLUMN [UserId] nvarchar(450) NOT NULL;
                END
                """);

            migrationBuilder.UpdateData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 1,
                column: "RecordedAt",
                value: new DateTime(2026, 8, 15, 15, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 2,
                column: "RecordedAt",
                value: new DateTime(2026, 8, 15, 16, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 3,
                column: "RecordedAt",
                value: new DateTime(2026, 8, 15, 16, 30, 0, 0, DateTimeKind.Utc));

            migrationBuilder.CreateIndex(
                name: "IX_VitalSigns_PatientId_RecordedAt",
                table: "VitalSigns",
                columns: new[] { "PatientId", "RecordedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_VitalSigns_DiastolicBP",
                table: "VitalSigns",
                sql: "[DiastolicBP] BETWEEN 40 AND 130");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VitalSigns_HeartRate",
                table: "VitalSigns",
                sql: "[HeartRate] BETWEEN 30 AND 250");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VitalSigns_OxygenSaturation",
                table: "VitalSigns",
                sql: "CAST([OxygenSaturation] AS REAL) BETWEEN 50 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VitalSigns_SystolicBP",
                table: "VitalSigns",
                sql: "[SystolicBP] BETWEEN 70 AND 220");

            migrationBuilder.CreateIndex(
                name: "IX_Medications_PatientId_IsActive",
                table: "Medications",
                columns: new[] { "PatientId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId_AppointmentDate",
                table: "Appointments",
                columns: new[] { "PatientId", "AppointmentDate" });

            migrationBuilder.CreateIndex(
                name: "UX_Appointments_DoctorId_AppointmentDate",
                table: "Appointments",
                columns: new[] { "DoctorId", "AppointmentDate" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Appointments_Status",
                table: "Appointments",
                sql: "[Status] IN ('Scheduled', 'Completed', 'Cancelled')");

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1 FROM [sys].[indexes]
                    WHERE [name] = N'IX_RefreshTokens_ExpiryDate'
                      AND [object_id] = OBJECT_ID(N'[RefreshTokens]'))
                    CREATE INDEX [IX_RefreshTokens_ExpiryDate]
                    ON [RefreshTokens] ([ExpiryDate]);

                IF NOT EXISTS (
                    SELECT 1 FROM [sys].[indexes]
                    WHERE [name] = N'IX_RefreshTokens_UserId'
                      AND [object_id] = OBJECT_ID(N'[RefreshTokens]'))
                    CREATE INDEX [IX_RefreshTokens_UserId]
                    ON [RefreshTokens] ([UserId]);

                IF NOT EXISTS (
                    SELECT 1 FROM [sys].[indexes]
                    WHERE [name] = N'UX_RefreshTokens_Token'
                      AND [object_id] = OBJECT_ID(N'[RefreshTokens]'))
                    CREATE UNIQUE INDEX [UX_RefreshTokens_Token]
                    ON [RefreshTokens] ([Token]);
                """);
        }

        // Reverts the Week 6 database model improvements.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_VitalSigns_PatientId_RecordedAt",
                table: "VitalSigns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VitalSigns_DiastolicBP",
                table: "VitalSigns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VitalSigns_HeartRate",
                table: "VitalSigns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VitalSigns_OxygenSaturation",
                table: "VitalSigns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VitalSigns_SystolicBP",
                table: "VitalSigns");

            migrationBuilder.DropIndex(
                name: "IX_Medications_PatientId_IsActive",
                table: "Medications");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId_AppointmentDate",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "UX_Appointments_DoctorId_AppointmentDate",
                table: "Appointments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Appointments_Status",
                table: "Appointments");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ContactNumber",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Medications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Frequency",
                table: "Medications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Dosage",
                table: "Medications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_VitalSigns_PatientId",
                table: "VitalSigns",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Medications_PatientId",
                table: "Medications",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");
        }
    }
}
