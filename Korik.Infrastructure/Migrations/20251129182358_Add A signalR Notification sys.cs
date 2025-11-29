using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Korik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAsignalRNotificationsys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarOwnerId = table.Column<int>(type: "int", nullable: false),
                    WorkShopProfileId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_CarOwnerProfiles_CarOwnerId",
                        column: x => x.CarOwnerId,
                        principalTable: "CarOwnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_WorkShopProfiles_WorkShopProfileId",
                        column: x => x.WorkShopProfileId,
                        principalTable: "WorkShopProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CarOwnerId",
                table: "Notifications",
                column: "CarOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Status",
                table: "Notifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_WorkShopProfileId",
                table: "Notifications",
                column: "WorkShopProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
