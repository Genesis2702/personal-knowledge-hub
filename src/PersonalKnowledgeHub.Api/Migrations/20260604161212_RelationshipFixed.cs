using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalKnowledgeHub.Migrations
{
    /// <inheritdoc />
    public partial class RelationshipFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_UserId_Name",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Resources_UserId_Title",
                table: "Resources");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserId_Name",
                table: "Tags",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_UserId_Title",
                table: "Resources",
                columns: new[] { "UserId", "Title" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_UserId_Name",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Resources_UserId_Title",
                table: "Resources");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserId_Name",
                table: "Tags",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_UserId_Title",
                table: "Resources",
                columns: new[] { "UserId", "Title" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
