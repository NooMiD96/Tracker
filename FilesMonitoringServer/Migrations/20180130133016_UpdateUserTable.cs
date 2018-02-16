using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FilesMonitoringServer.Migrations
{
    public partial class UpdateUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Trackers_TrackerId",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Changes");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_User_TrackerId",
                table: "Users",
                newName: "IX_Users_TrackerId");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Changes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Trackers_TrackerId",
                table: "Users",
                column: "TrackerId",
                principalTable: "Trackers",
                principalColumn: "TrackerId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Trackers_TrackerId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Changes");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameIndex(
                name: "IX_Users_TrackerId",
                table: "User",
                newName: "IX_User_TrackerId");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Changes",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Trackers_TrackerId",
                table: "User",
                column: "TrackerId",
                principalTable: "Trackers",
                principalColumn: "TrackerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
