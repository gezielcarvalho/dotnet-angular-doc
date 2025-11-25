using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToNet8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Category_CategoryId",
                table: "Product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameIndex(
                name: "IX_Product_CategoryId",
                table: "Products",
                newName: "IX_Products_CategoryId");

            migrationBuilder.AlterColumn<string>(
                name: "ImgUrl",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1479), new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1514) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1518), new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1519) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1520), new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1521) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1523), new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1524) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 5L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1525), new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1526) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 6L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1528), new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1529) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 7L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1530), new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1531) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 8L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1532), new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1533) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 9L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1534), new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1535) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 10L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1537), new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1538) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 11L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1539), new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1540) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 12L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1541), new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1542) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 13L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1543), new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1544) });

            migrationBuilder.InsertData(
                table: "Post",
                columns: new[] { "Id", "Content", "Title" },
                values: new object[,]
                {
                    { 1, "Content 1", "Post 1" },
                    { 2, "Content 2", "Post 2" },
                    { 3, "Content 3", "Post 3" },
                    { 4, "Content 4", "Post 4" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "ImgUrl", "Name", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, 1L, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1566), "Description 1", null, "Product 1", 1.99, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1567) },
                    { 2L, 2L, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1570), "Description 2", null, "Product 2", 2.9900000000000002, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1571) },
                    { 3L, 3L, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1573), "Description 3", null, "Product 3", 3.9900000000000002, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1574) },
                    { 4L, 4L, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1575), "Description 4", null, "Product 4", 4.9900000000000002, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1576) },
                    { 5L, 5L, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1578), "Description 5", null, "Product 5", 5.9900000000000002, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1579) },
                    { 6L, 6L, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1580), "Description 6", null, "Product 6", 6.9900000000000002, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1581) },
                    { 7L, 7L, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1583), "Description 7", null, "Product 7", 7.9900000000000002, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1584) },
                    { 8L, 8L, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1585), "Description 8", null, "Product 8", 8.9900000000000002, new DateTime(2025, 11, 25, 18, 15, 46, 702, DateTimeKind.Local).AddTicks(1586) }
                });

            migrationBuilder.InsertData(
                table: "Tag",
                columns: new[] { "Id", "TagName" },
                values: new object[,]
                {
                    { 1, "Tag 1" },
                    { 2, "Tag 2" },
                    { 3, "Tag 3" }
                });

            migrationBuilder.InsertData(
                table: "PostTag",
                columns: new[] { "PostId", "TagId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 2, 2 },
                    { 2, 3 }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Category_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Category_CategoryId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DeleteData(
                table: "Post",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Post",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PostTag",
                keyColumns: new[] { "PostId", "TagId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "PostTag",
                keyColumns: new[] { "PostId", "TagId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "PostTag",
                keyColumns: new[] { "PostId", "TagId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "PostTag",
                keyColumns: new[] { "PostId", "TagId" },
                keyValues: new object[] { 2, 3 });

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "Post",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Post",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tag",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tag",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tag",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CategoryId",
                table: "Product",
                newName: "IX_Product_CategoryId");

            migrationBuilder.AlterColumn<string>(
                name: "ImgUrl",
                table: "Product",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2641), new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2671) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2673), new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2674) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2676), new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2677) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2678), new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2679) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 5L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2680), new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2681) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 6L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2683), new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2684) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 7L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2685), new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2686) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 8L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2687), new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2688) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 9L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2689), new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2690) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 10L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2691), new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2692) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 11L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2693), new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2694) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 12L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2695), new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2696) });

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "Id",
                keyValue: 13L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2697), new DateTime(2023, 12, 8, 21, 50, 59, 140, DateTimeKind.Local).AddTicks(2699) });

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Category_CategoryId",
                table: "Product",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
