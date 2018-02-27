﻿// <auto-generated />
using FilesMonitoring;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace FilesMonitoring.Migrations
{
    [DbContext(typeof(SQLiteDb))]
    [Migration("20180227133813_AddNewEventToDir")]
    partial class AddNewEventToDir
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("FilesMonitoring.ClientException", b =>
                {
                    b.Property<int>("ExceptionId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateTime");

                    b.Property<string>("ExceptionInner")
                        .IsRequired();

                    b.Property<string>("UserName")
                        .IsRequired();

                    b.HasKey("ExceptionId");

                    b.ToTable("ClientExceptions");
                });

            modelBuilder.Entity("FilesMonitoring.TrackerEvent", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("Content");

                    b.Property<DateTime>("DateTime");

                    b.Property<int>("EventName");

                    b.Property<string>("FullName")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("OldFullName");

                    b.Property<string>("OldName");

                    b.Property<int>("TrackerEventInfoId");

                    b.Property<string>("UserName")
                        .IsRequired();

                    b.HasKey("id");

                    b.ToTable("TrackerEvent");
                });

            modelBuilder.Entity("FilesMonitoring.TrackerEventInfo", b =>
                {
                    b.Property<int>("TrackerEventInfoId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsContentInTrackerEvent");

                    b.Property<string>("PathToContent");

                    b.HasKey("TrackerEventInfoId");

                    b.ToTable("TrackerEventInfo");
                });

            modelBuilder.Entity("FilesMonitoring.TrackerEventInfo", b =>
                {
                    b.HasOne("FilesMonitoring.TrackerEvent", "TrackerEvent")
                        .WithMany()
                        .HasForeignKey("TrackerEventInfoId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
