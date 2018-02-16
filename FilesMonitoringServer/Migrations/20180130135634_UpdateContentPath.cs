using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FilesMonitoringServer.Migrations
{
    public partial class UpdateContentPath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Changes_Content_ContentId",
                table: "Changes");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Payload",
                table: "Content",
                nullable: true,
                oldClrType: typeof(byte[]));

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Content",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ContentId",
                table: "Changes",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Changes_Content_ContentId",
                table: "Changes",
                column: "ContentId",
                principalTable: "Content",
                principalColumn: "ContentId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Changes_Content_ContentId",
                table: "Changes");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Content");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Payload",
                table: "Content",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ContentId",
                table: "Changes",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Changes_Content_ContentId",
                table: "Changes",
                column: "ContentId",
                principalTable: "Content",
                principalColumn: "ContentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
