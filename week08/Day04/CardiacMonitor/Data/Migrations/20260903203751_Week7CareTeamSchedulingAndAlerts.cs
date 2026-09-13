using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardiacMonitor.Data.Migrations
{
    /// <inheritdoc />
    public partial class Week7CareTeamSchedulingAndAlerts : Migration
    {
        // Creates the Week 7 care-team, schedule, and medical-alert schema.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DoctorProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [DoctorProfiles]
                    ([UserId], [FullName], [LicenseNumber], [Specialty], [Department], [IsActive])
                SELECT
                    [user].[Id],
                    LEFT(COALESCE(NULLIF([user].[UserName], N''), NULLIF([user].[Email], N''), N'Existing Doctor'), 150),
                    CONCAT(N'LEGACY-', LEFT(REPLACE([user].[Id], N'-', N''), 43)),
                    N'General Cardiology',
                    N'Cardiology',
                    CAST(1 AS bit)
                FROM [AspNetUsers] AS [user]
                INNER JOIN [AspNetUserRoles] AS [userRole]
                    ON [user].[Id] = [userRole].[UserId]
                INNER JOIN [AspNetRoles] AS [role]
                    ON [userRole].[RoleId] = [role].[Id]
                WHERE [role].[NormalizedName] = N'DOCTOR'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM [DoctorProfiles] AS [profile]
                      WHERE [profile].[UserId] = [user].[Id]);
                """);

            migrationBuilder.CreateTable(
                name: "MedicalAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    VitalSignId = table.Column<int>(type: "int", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcknowledgedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalAlerts", x => x.Id);
                    table.CheckConstraint("CK_MedicalAlerts_Severity", "[Severity] IN ('Medium', 'High', 'Critical')");
                    table.CheckConstraint("CK_MedicalAlerts_Status", "[Status] IN ('Open', 'Acknowledged', 'Resolved')");
                    table.ForeignKey(
                        name: "FK_MedicalAlerts_AspNetUsers_AcknowledgedByUserId",
                        column: x => x.AcknowledgedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalAlerts_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalAlerts_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalAlerts_VitalSigns_VitalSignId",
                        column: x => x.VitalSignId,
                        principalTable: "VitalSigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NurseProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NurseProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NurseProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorAvailabilitySlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorProfileId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorAvailabilitySlots", x => x.Id);
                    table.CheckConstraint("CK_DoctorAvailability_DayOfWeek", "[DayOfWeek] BETWEEN 0 AND 6");
                    table.CheckConstraint("CK_DoctorAvailability_TimeRange", "[StartTime] < [EndTime]");
                    table.ForeignKey(
                        name: "FK_DoctorAvailabilitySlots_DoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientCareAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    NurseProfileId = table.Column<int>(type: "int", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientCareAssignments", x => x.Id);
                    table.CheckConstraint("CK_PatientCareAssignments_EndState", "([IsActive] = 1 AND [EndedAt] IS NULL) OR ([IsActive] = 0 AND [EndedAt] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_PatientCareAssignments_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientCareAssignments_NurseProfiles_NurseProfileId",
                        column: x => x.NurseProfileId,
                        principalTable: "NurseProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientCareAssignments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "d4e5f67a-8b9c-0d1e-2f3a-4b5c6d7e8f90", null, "Nurse", "NURSE" });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilitySlots_DoctorProfileId_DayOfWeek_IsActive",
                table: "DoctorAvailabilitySlots",
                columns: new[] { "DoctorProfileId", "DayOfWeek", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilitySlots_DoctorProfileId_DayOfWeek_StartTime_EndTime",
                table: "DoctorAvailabilitySlots",
                columns: new[] { "DoctorProfileId", "DayOfWeek", "StartTime", "EndTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProfiles_LicenseNumber",
                table: "DoctorProfiles",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProfiles_Specialty_IsActive",
                table: "DoctorProfiles",
                columns: new[] { "Specialty", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProfiles_UserId",
                table: "DoctorProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAlerts_AcknowledgedByUserId",
                table: "MedicalAlerts",
                column: "AcknowledgedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAlerts_PatientId_Status_CreatedAt",
                table: "MedicalAlerts",
                columns: new[] { "PatientId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAlerts_ResolvedByUserId",
                table: "MedicalAlerts",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAlerts_Status_Severity",
                table: "MedicalAlerts",
                columns: new[] { "Status", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAlerts_VitalSignId",
                table: "MedicalAlerts",
                column: "VitalSignId",
                unique: true,
                filter: "[VitalSignId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NurseProfiles_LicenseNumber",
                table: "NurseProfiles",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NurseProfiles_UserId",
                table: "NurseProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientCareAssignments_AssignedByUserId",
                table: "PatientCareAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientCareAssignments_NurseProfileId_IsActive",
                table: "PatientCareAssignments",
                columns: new[] { "NurseProfileId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientCareAssignments_PatientId_IsActive",
                table: "PatientCareAssignments",
                columns: new[] { "PatientId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientCareAssignments_PatientId_NurseProfileId",
                table: "PatientCareAssignments",
                columns: new[] { "PatientId", "NurseProfileId" },
                unique: true);
        }

        // Removes only the schema introduced by this Week 7 migration.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorAvailabilitySlots");

            migrationBuilder.DropTable(
                name: "MedicalAlerts");

            migrationBuilder.DropTable(
                name: "PatientCareAssignments");

            migrationBuilder.DropTable(
                name: "DoctorProfiles");

            migrationBuilder.DropTable(
                name: "NurseProfiles");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d4e5f67a-8b9c-0d1e-2f3a-4b5c6d7e8f90");
        }
    }
}
