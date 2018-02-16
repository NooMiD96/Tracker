using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FilesMonitoring.Migrations
{
    public partial class InitMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientExceptions",
                columns: table => new
                {
                    ExceptionId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateTime = table.Column<DateTime>(nullable: false),
                    ExceptionInner = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientExceptions", x => x.ExceptionId);
                });

            migrationBuilder.CreateTable(
                name: "TrackerEvent",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<byte[]>(nullable: true),
                    DateTime = table.Column<DateTime>(nullable: false),
                    EventName = table.Column<int>(nullable: false),
                    FullName = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    OldFullName = table.Column<string>(nullable: true),
                    OldName = table.Column<string>(nullable: true),
                    TrackerEventInfoId = table.Column<int>(nullable: false),
                    UserName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerEvent", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TrackerEventInfo",
                columns: table => new
                {
                    TrackerEventInfoId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsContentInTrackerEvent = table.Column<bool>(nullable: false),
                    PathToContent = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerEventInfo", x => x.TrackerEventInfoId);
                    table.ForeignKey(
                        name: "FK_TrackerEventInfo_TrackerEvent_TrackerEventInfoId",
                        column: x => x.TrackerEventInfoId,
                        principalTable: "TrackerEvent",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientExceptions");

            migrationBuilder.DropTable(
                name: "TrackerEventInfo");

            migrationBuilder.DropTable(
                name: "TrackerEvent");
        }
    }
}
