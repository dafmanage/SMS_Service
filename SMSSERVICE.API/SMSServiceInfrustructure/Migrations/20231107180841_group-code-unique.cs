using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMSServiceInfrustructure.Migrations
{
    /// <inheritdoc />
    public partial class groupcodeunique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GroupCode",
                table: "MessageGroups",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_MessageGroups_GroupCode",
                table: "MessageGroups",
                column: "GroupCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MessageGroups_GroupCode",
                table: "MessageGroups");

            migrationBuilder.AlterColumn<string>(
                name: "GroupCode",
                table: "MessageGroups",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
