﻿// <auto-generated />
using FilesMonitoringServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace FilesMonitoringServer.Migrations
{
    [DbContext(typeof(TrackerDb))]
    partial class TrackerDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("FilesMonitoringServer.Change", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ContentId");

                    b.Property<DateTime>("DateTime");

                    b.Property<int>("EventName");

                    b.Property<int>("FileId");

                    b.Property<string>("OldFullName");

                    b.Property<string>("OldName");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ContentId")
                        .IsUnique()
                        .HasFilter("[ContentId] IS NOT NULL");

                    b.HasIndex("FileId");

                    b.ToTable("Changes");
                });

            modelBuilder.Entity("FilesMonitoringServer.ClientException", b =>
                {
                    b.Property<int>("ExceptionId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateTime");

                    b.Property<string>("ExceptionInner")
                        .IsRequired();

                    b.Property<int>("TrackerId");

                    b.Property<int>("UserId");

                    b.HasKey("ExceptionId");

                    b.HasIndex("TrackerId");

                    b.ToTable("ClientExceptions");
                });

            modelBuilder.Entity("FilesMonitoringServer.Content", b =>
                {
                    b.Property<int>("ContentId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FilePath");

                    b.Property<byte[]>("Payload");

                    b.HasKey("ContentId");

                    b.ToTable("Contents");
                });

            modelBuilder.Entity("FilesMonitoringServer.File", b =>
                {
                    b.Property<int>("FileId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FileName")
                        .IsRequired();

                    b.Property<string>("FilePath");

                    b.Property<string>("FullName")
                        .IsRequired();

                    b.Property<bool>("IsNeedDelete")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(true);

                    b.Property<bool>("IsWasDeletedChange")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<DateTime?>("RemoveFromDbTime");

                    b.Property<int>("TrackerId");

                    b.HasKey("FileId");

                    b.HasIndex("TrackerId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("FilesMonitoringServer.Tracker", b =>
                {
                    b.Property<int>("TrackerId");

                    b.HasKey("TrackerId");

                    b.ToTable("Trackers");
                });

            modelBuilder.Entity("FilesMonitoringServer.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsCanAuthohorization");

                    b.Property<int>("TrackerId");

                    b.Property<string>("UserName")
                        .IsRequired();

                    b.HasKey("UserId");

                    b.HasIndex("TrackerId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("FilesMonitoringServer.Change", b =>
                {
                    b.HasOne("FilesMonitoringServer.Content", "Content")
                        .WithOne("Change")
                        .HasForeignKey("FilesMonitoringServer.Change", "ContentId");

                    b.HasOne("FilesMonitoringServer.File", "File")
                        .WithMany("ChangeList")
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("FilesMonitoringServer.ClientException", b =>
                {
                    b.HasOne("FilesMonitoringServer.Tracker")
                        .WithMany("ClientExceptionList")
                        .HasForeignKey("TrackerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("FilesMonitoringServer.File", b =>
                {
                    b.HasOne("FilesMonitoringServer.Tracker", "Tracker")
                        .WithMany("FileList")
                        .HasForeignKey("TrackerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("FilesMonitoringServer.User", b =>
                {
                    b.HasOne("FilesMonitoringServer.Tracker", "Tracker")
                        .WithMany("UserList")
                        .HasForeignKey("TrackerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
