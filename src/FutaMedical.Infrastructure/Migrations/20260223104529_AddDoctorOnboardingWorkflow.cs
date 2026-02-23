using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutaMedical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorOnboardingWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Departments_DepartmentId",
                table: "Doctors");

            migrationBuilder.AddColumn<string>(
                name: "PasswordSetupToken",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordSetupTokenExpiry",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DepartmentId",
                table: "Doctors",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationReviewedAt",
                table: "Doctors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationStatus",
                table: "Doctors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationSubmittedAt",
                table: "Doctors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Doctors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificateDocument",
                table: "Doctors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentificationDocument",
                table: "Doctors",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Departments_DepartmentId",
                table: "Doctors",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Departments_DepartmentId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "PasswordSetupToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordSetupTokenExpiry",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApplicationReviewedAt",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ApplicationStatus",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ApplicationSubmittedAt",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "CertificateDocument",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "IdentificationDocument",
                table: "Doctors");

            migrationBuilder.AlterColumn<Guid>(
                name: "DepartmentId",
                table: "Doctors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Departments_DepartmentId",
                table: "Doctors",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
