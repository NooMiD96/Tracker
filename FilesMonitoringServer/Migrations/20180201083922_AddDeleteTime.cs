using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FilesMonitoringServer.Migrations
{
    public partial class AddDeleteTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Files",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemoveFromDbTime",
                table: "Files",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemoveFromDbTime",
                table: "Files");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Files",
                nullable: true,
                oldClrType: typeof(bool),
                oldDefaultValue: false);
        }
    }
}
