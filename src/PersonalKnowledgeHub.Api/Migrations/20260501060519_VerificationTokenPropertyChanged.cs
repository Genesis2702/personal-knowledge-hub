using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalKnowledgeHub.Migrations
{
    /// <inheritdoc />
    public partial class VerificationTokenPropertyChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TokenHash",
                table: "VerificationTokens",
                newName: "Token");

            migrationBuilder.RenameIndex(
                name: "IX_VerificationTokens_TokenHash",
                table: "VerificationTokens",
                newName: "IX_VerificationTokens_Token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Token",
                table: "VerificationTokens",
                newName: "TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_VerificationTokens_Token",
                table: "VerificationTokens",
                newName: "IX_VerificationTokens_TokenHash");
        }
    }
}
