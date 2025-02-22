using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Final.Data.Migrations.Shop
{
    /// <inheritdoc />
    public partial class InitialShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BaseTopic = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentityRole",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MqttTools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ToolBaseTopic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MqttTools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MqttTools_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyRoles_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyRoles_IdentityRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "IdentityRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MqttTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseTopic = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TopicTemplate = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    HowMany = table.Column<int>(type: "int", nullable: false),
                    DataType = table.Column<int>(type: "int", nullable: false),
                    TopicPurpose = table.Column<int>(type: "int", nullable: false),
                    MqttToolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Data64 = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MqttTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MqttTopics_MqttTools_MqttToolId",
                        column: x => x.MqttToolId,
                        principalTable: "MqttTools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "BaseTopic", "Name" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "ciceklisogukhavadeposu", "Ciceklisogukhavadeposu" });

            migrationBuilder.InsertData(
                table: "IdentityRole",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "role-ciceklisogukhavadeposu", null, "CiceklisogukhavadeposuRole", "CICEKLISOGUKHAVADEPOSUROLE" });

            migrationBuilder.InsertData(
                table: "CompanyRoles",
                columns: new[] { "Id", "CompanyId", "RoleId" },
                values: new object[] { 1, new Guid("11111111-1111-1111-1111-111111111111"), "role-ciceklisogukhavadeposu" });

            migrationBuilder.InsertData(
                table: "MqttTools",
                columns: new[] { "Id", "CompanyId", "Description", "ImageUrl", "Name", "ToolBaseTopic" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("11111111-1111-1111-1111-111111111111"), "Main control room for the company", null, "Control Room", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("11111111-1111-1111-1111-111111111111"), "Compressor tool", null, "Compressor", null }
                });

            migrationBuilder.InsertData(
                table: "MqttTopics",
                columns: new[] { "Id", "BaseTopic", "Data64", "DataType", "HowMany", "MqttToolId", "TopicPurpose", "TopicTemplate" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 1, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room1/temp" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 1, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room2/temp" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 1, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room3/temp" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 1, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room4/temp" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 1, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room5/temp" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 1, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room6/temp" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 1, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room7/temp" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 1, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room8/temp" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 1, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room9/temp" },
                    { new Guid("00000000-0000-0000-0000-00000000000a"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 1, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room10/temp" },
                    { new Guid("00000000-0000-0000-0000-00000000000b"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 1, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room11/temp" },
                    { new Guid("00000000-0000-0000-0000-00000000000c"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 1, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room12/temp" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 3, 1, new Guid("33333333-3333-3333-3333-333333333333"), 0, "ciceklisogukhavadeposu/control_room/compressor/status" },
                    { new Guid("aaaaaaa0-aaaa-aaaa-aaaa-000000000001"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 3, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "pwr_rqst/room1/control_room/ciceklisogukhavadeposu" },
                    { new Guid("aaaaaaa0-aaaa-aaaa-aaaa-000000000002"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 3, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "pwr_rqst/room2/control_room/ciceklisogukhavadeposu" },
                    { new Guid("aaaaaaa0-aaaa-aaaa-aaaa-000000000003"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 3, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "pwr_rqst/room3/control_room/ciceklisogukhavadeposu" },
                    { new Guid("aaaaaaa0-aaaa-aaaa-aaaa-000000000004"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 3, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "pwr_rqst/room4/control_room/ciceklisogukhavadeposu" },
                    { new Guid("aaaaaaa0-aaaa-aaaa-aaaa-000000000005"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 3, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "pwr_rqst/room5/control_room/ciceklisogukhavadeposu" },
                    { new Guid("aaaaaaa0-aaaa-aaaa-aaaa-000000000006"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 3, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "pwr_rqst/room6/control_room/ciceklisogukhavadeposu" },
                    { new Guid("aaaaaaa0-aaaa-aaaa-aaaa-000000000007"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 3, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "pwr_rqst/room7/control_room/ciceklisogukhavadeposu" },
                    { new Guid("aaaaaaa0-aaaa-aaaa-aaaa-000000000008"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 3, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "pwr_rqst/room8/control_room/ciceklisogukhavadeposu" },
                    { new Guid("aaaaaaa0-aaaa-aaaa-aaaa-000000000009"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 3, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "pwr_rqst/room9/control_room/ciceklisogukhavadeposu" },
                    { new Guid("aaaaaaa0-aaaa-aaaa-aaaa-00000000000a"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 3, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "pwr_rqst/room10/control_room/ciceklisogukhavadeposu" },
                    { new Guid("aaaaaaa0-aaaa-aaaa-aaaa-00000000000b"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 3, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "pwr_rqst/room11/control_room/ciceklisogukhavadeposu" },
                    { new Guid("aaaaaaa0-aaaa-aaaa-aaaa-00000000000c"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 3, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "pwr_rqst/room12/control_room/ciceklisogukhavadeposu" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-000000000001"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "set_temp/room1/control_room/ciceklisogukhavadeposu" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-000000000002"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "set_temp/room2/control_room/ciceklisogukhavadeposu" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-000000000003"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "set_temp/room3/control_room/ciceklisogukhavadeposu" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-000000000004"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "set_temp/room4/control_room/ciceklisogukhavadeposu" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-000000000005"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "set_temp/room5/control_room/ciceklisogukhavadeposu" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-000000000006"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "set_temp/room6/control_room/ciceklisogukhavadeposu" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-000000000007"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "set_temp/room7/control_room/ciceklisogukhavadeposu" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-000000000008"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "set_temp/room8/control_room/ciceklisogukhavadeposu" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-000000000009"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "set_temp/room9/control_room/ciceklisogukhavadeposu" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-00000000000a"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "set_temp/room10/control_room/ciceklisogukhavadeposu" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-00000000000b"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "set_temp/room11/control_room/ciceklisogukhavadeposu" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-00000000000c"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 1, "set_temp/room12/control_room/ciceklisogukhavadeposu" },
                    { new Guid("cccccccc-cccc-cccc-cccc-000000000001"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room1/status" },
                    { new Guid("cccccccc-cccc-cccc-cccc-000000000002"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room2/status" },
                    { new Guid("cccccccc-cccc-cccc-cccc-000000000003"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room3/status" },
                    { new Guid("cccccccc-cccc-cccc-cccc-000000000004"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room4/status" },
                    { new Guid("cccccccc-cccc-cccc-cccc-000000000005"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room5/status" },
                    { new Guid("cccccccc-cccc-cccc-cccc-000000000006"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room6/status" },
                    { new Guid("cccccccc-cccc-cccc-cccc-000000000007"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room7/status" },
                    { new Guid("cccccccc-cccc-cccc-cccc-000000000008"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room8/status" },
                    { new Guid("cccccccc-cccc-cccc-cccc-000000000009"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room9/status" },
                    { new Guid("cccccccc-cccc-cccc-cccc-00000000000a"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room10/status" },
                    { new Guid("cccccccc-cccc-cccc-cccc-00000000000b"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room11/status" },
                    { new Guid("cccccccc-cccc-cccc-cccc-00000000000c"), "ciceklisogukhavadeposu", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 8, 12, new Guid("22222222-2222-2222-2222-222222222222"), 0, "ciceklisogukhavadeposu/control_room/room12/status" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRoles_CompanyId",
                table: "CompanyRoles",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRoles_RoleId",
                table: "CompanyRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_MqttTools_CompanyId",
                table: "MqttTools",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_MqttTopics_MqttToolId",
                table: "MqttTopics",
                column: "MqttToolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyRoles");

            migrationBuilder.DropTable(
                name: "MqttTopics");

            migrationBuilder.DropTable(
                name: "IdentityRole");

            migrationBuilder.DropTable(
                name: "MqttTools");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
