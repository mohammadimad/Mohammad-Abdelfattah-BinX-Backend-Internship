using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardiacMonitor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Patients_Gender_LastName",
                table: "Patients",
                columns: new[] { "Gender", "LastName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_Gender_LastName",
                table: "Patients");
        }
    }
}
