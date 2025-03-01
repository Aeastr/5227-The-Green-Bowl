using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGreenBowl.Migrations
{
    public partial class BasketsMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "Basket",
                columns: table => new
                {
                    basketID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Basket", x => x.basketID);
                    table.ForeignKey(
                        name: "FK_Basket_AspNetUsers_userID",
                        column: x => x.userID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BasketItem",
                columns: table => new
                {
                    basketItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    basketID = table.Column<int>(type: "int", nullable: false),
                    itemID = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasketItem", x => x.basketItemID);
                    table.ForeignKey(
                        name: "FK_BasketItem_Basket_basketID",
                        column: x => x.basketID,
                        principalTable: "Basket",
                        principalColumn: "basketID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BasketItem_MenuItem_itemID",
                        column: x => x.itemID,
                        principalTable: "MenuItem",
                        principalColumn: "itemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Basket_userID",
                table: "Basket",
                column: "userID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BasketItem_basketID",
                table: "BasketItem",
                column: "basketID");

            migrationBuilder.CreateIndex(
                name: "IX_BasketItem_itemID",
                table: "BasketItem",
                column: "itemID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BasketItem");

            migrationBuilder.DropTable(
                name: "Basket");

        }
    }
}
