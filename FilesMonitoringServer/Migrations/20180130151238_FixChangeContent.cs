using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FilesMonitoringServer.Migrations
{
    public partial class FixChangeContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Changes_Contents_ContentId",
                table: "Changes");

            migrationBuilder.DropIndex(
                name: "IX_Changes_ContentId",
                table: "Changes");

            migrationBuilder.AlterColumn<int>(
                name: "ContentId",
                table: "Changes",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateIndex(
                name: "IX_Changes_ContentId",
                table: "Changes",
                column: "ContentId",
                unique: true,
                filter: "[ContentId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Changes_Contents_ContentId",
                table: "Changes",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "ContentId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Changes_Contents_ContentId",
                table: "Changes");

            migrationBuilder.DropIndex(
                name: "IX_Changes_ContentId",
                table: "Changes");

            migrationBuilder.AlterColumn<int>(
                name: "ContentId",
                table: "Changes",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Changes_ContentId",
                table: "Changes",
                column: "ContentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Changes_Contents_ContentId",
                table: "Changes",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "ContentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
