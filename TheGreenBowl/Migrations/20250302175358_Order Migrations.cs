using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGreenBowl.Migrations
{
    public partial class OrderMigrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    orderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    orderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    totalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    orderStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    orderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    deliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    postcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contactPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.orderID);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_userID",
                        column: x => x.userID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    orderItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderID = table.Column<int>(type: "int", nullable: false),
                    itemID = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    priceAtTime = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.orderItemID);
                    table.ForeignKey(
                        name: "FK_OrderItems_MenuItem_itemID",
                        column: x => x.itemID,
                        principalTable: "MenuItem",
                        principalColumn: "itemID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_orderID",
                        column: x => x.orderID,
                        principalTable: "Orders",
                        principalColumn: "orderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_itemID",
                table: "OrderItems",
                column: "itemID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_orderID",
                table: "OrderItems",
                column: "orderID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_userID",
                table: "Orders",
                column: "userID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
