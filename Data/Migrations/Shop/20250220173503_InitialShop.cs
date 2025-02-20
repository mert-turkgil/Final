using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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
                name: "MqttTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseTopic = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TopicTemplate = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    HowMany = table.Column<int>(type: "int", nullable: false),
                    DataType = table.Column<int>(type: "int", nullable: false),
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
                name: "MqttTopics");

            migrationBuilder.DropTable(
                name: "MqttTools");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
