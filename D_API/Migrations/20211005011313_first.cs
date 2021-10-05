using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace D_API.Migrations
{
    public partial class first : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataEntries",
                columns: table => new
                {
                    Owner = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Identifier = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReadOnlyKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataEntries", x => new { x.Owner, x.Identifier });
                });

            migrationBuilder.CreateTable(
                name: "UserDataTrackers",
                columns: table => new
                {
                    Key = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyTransferUploadQuota = table.Column<double>(type: "float", nullable: false),
                    DailyTransferDownloadQuota = table.Column<double>(type: "float", nullable: false),
                    TrackersList = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    StorageQuota = table.Column<double>(type: "float", nullable: false),
                    StorageUsage = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDataTrackers", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Key = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Roles = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Identifier = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Secret = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CurrentStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "UserHandles",
                columns: table => new
                {
                    Key = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Identifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataEntryIdentifier = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DataEntryOwner = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHandles", x => x.Key);
                    table.ForeignKey(
                        name: "FK_UserHandles_DataEntries_DataEntryOwner_DataEntryIdentifier",
                        columns: x => new { x.DataEntryOwner, x.DataEntryIdentifier },
                        principalTable: "DataEntries",
                        principalColumns: new[] { "Owner", "Identifier" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DataEntries",
                columns: new[] { "Identifier", "Owner", "Data", "ReadOnlyKey" },
                values: new object[] { "default", new Guid("07eea8f3-dc1e-458c-a7a7-103cdfd65ebf"), new byte[0], new Guid("da46616d-7e9d-4b27-ae40-975abf208bfe") });

            migrationBuilder.InsertData(
                table: "UserDataTrackers",
                columns: new[] { "Key", "DailyTransferDownloadQuota", "DailyTransferUploadQuota", "StorageQuota", "StorageUsage", "TrackersList" },
                values: new object[] { new Guid("07eea8f3-dc1e-458c-a7a7-103cdfd65ebf"), 0.0, 0.0, 0.0, 0.0, new byte[0] });

            migrationBuilder.InsertData(
                table: "UserHandles",
                columns: new[] { "Key", "DataEntryIdentifier", "DataEntryOwner", "Identifier" },
                values: new object[] { new Guid("07eea8f3-dc1e-458c-a7a7-103cdfd65ebf"), null, null, "Default" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Key", "CurrentStatus", "Identifier", "Roles", "Secret" },
                values: new object[] { new Guid("07eea8f3-dc1e-458c-a7a7-103cdfd65ebf"), 0, "Default", "", "be400e28fdcdd381773d9476d3040262d6042b187955f0cb5dcece9c73539fb0323ae874cc0320fc5d07033ea0a2e6baf7fedc8b9dec89624a804d4fab03ec92" });

            migrationBuilder.CreateIndex(
                name: "IX_UserHandles_DataEntryOwner_DataEntryIdentifier",
                table: "UserHandles",
                columns: new[] { "DataEntryOwner", "DataEntryIdentifier" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDataTrackers");

            migrationBuilder.DropTable(
                name: "UserHandles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "DataEntries");
        }
    }
}
