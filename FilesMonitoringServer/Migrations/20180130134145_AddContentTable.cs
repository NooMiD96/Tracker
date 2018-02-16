using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FilesMonitoringServer.Migrations
{
    public partial class AddContentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Changes");

            migrationBuilder.AddColumn<int>(
                name: "ContentId",
                table: "Changes",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Content",
                columns: table => new
                {
                    ContentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Payload = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Content", x => x.ContentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Changes_ContentId",
                table: "Changes",
                column: "ContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Changes_Content_ContentId",
                table: "Changes",
                column: "ContentId",
                principalTable: "Content",
                principalColumn: "ContentId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Changes_Content_ContentId",
                table: "Changes");

            migrationBuilder.DropTable(
                name: "Content");

            migrationBuilder.DropIndex(
                name: "IX_Changes_ContentId",
                table: "Changes");

            migrationBuilder.DropColumn(
                name: "ContentId",
                table: "Changes");

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "Changes",
                nullable: true);
        }
    }
}
