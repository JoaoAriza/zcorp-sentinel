using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityDefense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskReasons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityRiskCases",
                table: "IdentityRiskCases");

            migrationBuilder.RenameTable(
                name: "IdentityRiskCases",
                newName: "identity_risk_cases");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "identity_risk_cases",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "identity_risk_cases",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Classification",
                table: "identity_risk_cases",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Channel",
                table: "identity_risk_cases",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<List<string>>(
                name: "RiskReasons",
                table: "identity_risk_cases",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_identity_risk_cases",
                table: "identity_risk_cases",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_identity_risk_cases",
                table: "identity_risk_cases");

            migrationBuilder.DropColumn(
                name: "RiskReasons",
                table: "identity_risk_cases");

            migrationBuilder.RenameTable(
                name: "identity_risk_cases",
                newName: "IdentityRiskCases");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "IdentityRiskCases",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "IdentityRiskCases",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Classification",
                table: "IdentityRiskCases",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Channel",
                table: "IdentityRiskCases",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityRiskCases",
                table: "IdentityRiskCases",
                column: "Id");
        }
    }
}
