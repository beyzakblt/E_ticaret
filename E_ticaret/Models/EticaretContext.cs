using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace E_ticaret.Models;

public partial class EticaretContext : DbContext
{
    public EticaretContext()
    {
    }

    public EticaretContext(DbContextOptions<EticaretContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ApiAyarlari> ApiAyarlaris { get; set; }

    public virtual DbSet<SiteAyarlari> SiteAyarlaris { get; set; }

    public virtual DbSet<Yoneticiler> Yoneticilers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=beyza;Database=Eticaret;TrustServerCertificate=True;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiAyarlari>(entity =>
        {
            entity.ToTable("ApiAyarlari");

            entity.Property(e => e.Ad)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Kod).HasColumnType("text");
        });

        modelBuilder.Entity<SiteAyarlari>(entity =>
        {
            entity.ToTable("SiteAyarlari");

            entity.Property(e => e.Adres).HasColumnType("text");
            entity.Property(e => e.Facebook).HasColumnType("text");
            entity.Property(e => e.Harita).HasColumnType("text");
            entity.Property(e => e.Instagram).HasColumnType("text");
            entity.Property(e => e.Linkedin).HasColumnType("text");
            entity.Property(e => e.Logo).HasColumnType("text");
            entity.Property(e => e.Mail)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Pinteret).HasColumnType("text");
            entity.Property(e => e.SiteAdi)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("Site_Adi");
            entity.Property(e => e.Tel)
                .HasMaxLength(13)
                .IsUnicode(false);
            entity.Property(e => e.Tiktok).HasColumnType("text");
            entity.Property(e => e.Twitter).HasColumnType("text");
            entity.Property(e => e.Whatsapp)
                .HasMaxLength(13)
                .IsUnicode(false);
            entity.Property(e => e.Youtube).HasColumnType("text");
        });

        modelBuilder.Entity<Yoneticiler>(entity =>
        {
            entity.ToTable("Yoneticiler");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Durum).HasColumnName("durum");
            entity.Property(e => e.InfoId).HasColumnName("info_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Pass)
                .HasMaxLength(64)
                .IsFixedLength()
                .HasColumnName("pass");
            entity.Property(e => e.SonGiris)
                .HasColumnType("datetime")
                .HasColumnName("son_giris");
            entity.Property(e => e.StatuId).HasColumnName("statu_id");
            entity.Property(e => e.Users)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
