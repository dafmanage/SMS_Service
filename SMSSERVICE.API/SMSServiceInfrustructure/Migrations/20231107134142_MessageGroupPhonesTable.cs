using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMSServiceInfrustructure.Migrations
{
    /// <inheritdoc />
    public partial class MessageGroupPhonesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageGroupPhones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Rowstatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageGroupPhones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageGroupPhones_MessageGroups_MessageGroupId",
                        column: x => x.MessageGroupId,
                        principalTable: "MessageGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageGroupPhones_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageGroupPhones_CreatedById",
                table: "MessageGroupPhones",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MessageGroupPhones_MessageGroupId",
                table: "MessageGroupPhones",
                column: "MessageGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageGroupPhones_PhoneNumber_MessageGroupId",
                table: "MessageGroupPhones",
                columns: new[] { "PhoneNumber", "MessageGroupId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageGroupPhones");
        }
    }
}
