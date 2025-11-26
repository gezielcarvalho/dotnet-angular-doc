using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddFileSizeBytesColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Folders",
                keyColumn: "Id",
                keyValue: new Guid("526ec1b7-9979-4326-abb8-23297b8af839"));

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: new Guid("22fdaba6-4871-4de4-a504-58885e1dff26"));

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: new Guid("c0eb820d-0b0f-4a57-8f18-fb75e19ba9fb"));

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: new Guid("ce12a48e-7e4d-4429-a8f7-af124010cd84"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b72fb883-39b1-49f4-8cdc-5f627a572db2"));

            migrationBuilder.RenameColumn(
                name: "VersionComment",
                table: "DocumentVersions",
                newName: "ChangeComment");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "DocumentVersions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "DocumentVersions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "Documents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Color", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("3e418a9c-b1da-42a1-91ac-58527ec61b5e"), "#FF0000", new DateTime(2025, 11, 26, 3, 49, 1, 369, DateTimeKind.Utc).AddTicks(9023), "System", null, null, "Important documents", false, null, null, "Important" },
                    { new Guid("a136eb37-28ca-4f3c-bb2e-436f37a19a64"), "#FFA500", new DateTime(2025, 11, 26, 3, 49, 1, 369, DateTimeKind.Utc).AddTicks(9025), "System", null, null, "Draft documents", false, null, null, "Draft" },
                    { new Guid("cfcdee41-b49f-424c-96d4-3686484b902c"), "#008000", new DateTime(2025, 11, 26, 3, 49, 1, 369, DateTimeKind.Utc).AddTicks(9028), "System", null, null, "Final version documents", false, null, null, "Final" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Department", "Email", "FirstName", "IsActive", "IsDeleted", "LastLoginAt", "LastName", "ModifiedAt", "ModifiedBy", "PasswordHash", "Role", "Username" },
                values: new object[] { new Guid("eb2f0a7e-5863-466f-8f93-2a9d033ec13f"), new DateTime(2025, 11, 26, 3, 49, 1, 369, DateTimeKind.Utc).AddTicks(7997), "System", null, null, null, "admin@edm.local", "System", true, false, null, "Administrator", null, null, "$2a$11$GDuks/ZvidfojBkrwz7A/e3DFPvDkv9h/Kwq8ZeMl.mU0l2jfOFBm", "SystemAdmin", "admin" });

            migrationBuilder.InsertData(
                table: "Folders",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsSystemFolder", "Level", "ModifiedAt", "ModifiedBy", "Name", "OwnerId", "ParentFolderId", "Path" },
                values: new object[] { new Guid("1e3ea9d9-2dae-4537-8132-e8914fada166"), new DateTime(2025, 11, 26, 3, 49, 1, 369, DateTimeKind.Utc).AddTicks(8986), "System", null, null, "Root folder for all documents", false, true, 0, null, null, "Root", new Guid("eb2f0a7e-5863-466f-8f93-2a9d033ec13f"), null, "/" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Folders",
                keyColumn: "Id",
                keyValue: new Guid("1e3ea9d9-2dae-4537-8132-e8914fada166"));

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: new Guid("3e418a9c-b1da-42a1-91ac-58527ec61b5e"));

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: new Guid("a136eb37-28ca-4f3c-bb2e-436f37a19a64"));

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: new Guid("cfcdee41-b49f-424c-96d4-3686484b902c"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("eb2f0a7e-5863-466f-8f93-2a9d033ec13f"));

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "ChangeComment",
                table: "DocumentVersions",
                newName: "VersionComment");

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Color", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("22fdaba6-4871-4de4-a504-58885e1dff26"), "#FF0000", new DateTime(2025, 11, 26, 0, 59, 22, 857, DateTimeKind.Utc).AddTicks(8642), "System", null, null, "Important documents", false, null, null, "Important" },
                    { new Guid("c0eb820d-0b0f-4a57-8f18-fb75e19ba9fb"), "#008000", new DateTime(2025, 11, 26, 0, 59, 22, 857, DateTimeKind.Utc).AddTicks(8658), "System", null, null, "Final version documents", false, null, null, "Final" },
                    { new Guid("ce12a48e-7e4d-4429-a8f7-af124010cd84"), "#FFA500", new DateTime(2025, 11, 26, 0, 59, 22, 857, DateTimeKind.Utc).AddTicks(8645), "System", null, null, "Draft documents", false, null, null, "Draft" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Department", "Email", "FirstName", "IsActive", "IsDeleted", "LastLoginAt", "LastName", "ModifiedAt", "ModifiedBy", "PasswordHash", "Role", "Username" },
                values: new object[] { new Guid("b72fb883-39b1-49f4-8cdc-5f627a572db2"), new DateTime(2025, 11, 26, 0, 59, 22, 857, DateTimeKind.Utc).AddTicks(7555), "System", null, null, null, "admin@edm.local", "System", true, false, null, "Administrator", null, null, "$2a$11$Cga3Q.qAx6njhsxtpntJd.dXaRACvFIoT8lMeLs.fXU5xG89E7GCS", "SystemAdmin", "admin" });

            migrationBuilder.InsertData(
                table: "Folders",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsSystemFolder", "Level", "ModifiedAt", "ModifiedBy", "Name", "OwnerId", "ParentFolderId", "Path" },
                values: new object[] { new Guid("526ec1b7-9979-4326-abb8-23297b8af839"), new DateTime(2025, 11, 26, 0, 59, 22, 857, DateTimeKind.Utc).AddTicks(8580), "System", null, null, "Root folder for all documents", false, true, 0, null, null, "Root", new Guid("b72fb883-39b1-49f4-8cdc-5f627a572db2"), null, "/" });
        }
    }
}
