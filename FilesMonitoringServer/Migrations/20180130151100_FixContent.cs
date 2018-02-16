using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FilesMonitoringServer.Migrations
{
    public partial class FixContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Changes_ContentId",
                table: "Changes");

            migrationBuilder.CreateIndex(
                name: "IX_Changes_ContentId",
                table: "Changes",
                column: "ContentId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Changes_ContentId",
                table: "Changes");

            migrationBuilder.CreateIndex(
                name: "IX_Changes_ContentId",
                table: "Changes",
                column: "ContentId");
        }
    }
}
