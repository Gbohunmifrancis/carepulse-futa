using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutaMedical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinalMissingModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DoctorLeaveRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AdminResponse = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReviewedByAdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorLeaveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorLeaveRequests_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthArticles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    AuthorAdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MedicationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Dosage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Frequency = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Duration = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Instructions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionTemplates_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Referrals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referrals_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referrals_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VaccinationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VaccineName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DoseNumber = table.Column<int>(type: "integer", nullable: false),
                    DateAdministered = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Provider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccinationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VaccinationRecords_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WaitingListEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaitingListEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaitingListEntries_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WaitingListEntries_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorLeaveRequests_DoctorId",
                table: "DoctorLeaveRequests",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionTemplates_DoctorId",
                table: "PrescriptionTemplates",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_DoctorId",
                table: "Referrals",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_StudentId",
                table: "Referrals",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_StudentId",
                table: "VaccinationRecords",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitingListEntries_DoctorId",
                table: "WaitingListEntries",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitingListEntries_StudentId",
                table: "WaitingListEntries",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorLeaveRequests");

            migrationBuilder.DropTable(
                name: "HealthArticles");

            migrationBuilder.DropTable(
                name: "PrescriptionTemplates");

            migrationBuilder.DropTable(
                name: "Referrals");

            migrationBuilder.DropTable(
                name: "VaccinationRecords");

            migrationBuilder.DropTable(
                name: "WaitingListEntries");
        }
    }
}
