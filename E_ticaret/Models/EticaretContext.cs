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

    public virtual DbSet<AltKategoriler> AltKategorilers { get; set; }

    public virtual DbSet<ApiAyarlari> ApiAyarlaris { get; set; }

    public virtual DbSet<Kategoriler> Kategorilers { get; set; }

    public virtual DbSet<Kullanicilar> Kullanicilars { get; set; }

    public virtual DbSet<Markalar> Markalars { get; set; }

    public virtual DbSet<Siparisler> Siparislers { get; set; }

    public virtual DbSet<SiteAyarlari> SiteAyarlaris { get; set; }

    public virtual DbSet<Slider> Sliders { get; set; }

    public virtual DbSet<Sozlesmeler> Sozlesmelers { get; set; }

    public virtual DbSet<UrunFoto> UrunFotos { get; set; }

    public virtual DbSet<UrunYorumlari> UrunYorumlaris { get; set; }

    public virtual DbSet<Urunler> Urunlers { get; set; }

    public virtual DbSet<Yoneticiler> Yoneticilers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=beyza;Database=Eticaret;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AltKategoriler>(entity =>
        {
            entity.ToTable("Alt_Kategoriler");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnaKategoriId).HasColumnName("Ana_Kategori_Id");
            entity.Property(e => e.KategoriAdi)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("Kategori_Adi");
            entity.Property(e => e.Kdv).HasColumnName("KDV");
        });

        modelBuilder.Entity<ApiAyarlari>(entity =>
        {
            entity.ToTable("ApiAyarlari");

            entity.Property(e => e.Ad)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Kod).HasColumnType("text");
        });

        modelBuilder.Entity<Kategoriler>(entity =>
        {
            entity.ToTable("Kategoriler");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.KategoriAdi)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("Kategori_Adi");
        });

        modelBuilder.Entity<Kullanicilar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Kullanic__3214EC07B9CEBA97");

            entity.ToTable("Kullanicilar");

            entity.HasIndex(e => e.Email, "UQ__Kullanic__A9D10534DDDC8A10").IsUnique();

            entity.Property(e => e.AdSoyad).HasMaxLength(100);
            entity.Property(e => e.Durum).HasDefaultValue(false);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Ilce).HasMaxLength(50);
            entity.Property(e => e.KayitTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OnayKodu).HasMaxLength(6);
            entity.Property(e => e.PostaKodu).HasMaxLength(10);
            entity.Property(e => e.Sehir).HasMaxLength(50);
            entity.Property(e => e.Telefon).HasMaxLength(20);
        });

        modelBuilder.Entity<Markalar>(entity =>
        {
            entity.ToTable("Markalar");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MarkaAd)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("Marka_Ad");
        });

        modelBuilder.Entity<Siparisler>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Siparisl__3214EC0723FC0B2A");

            entity.ToTable("Siparisler");

            entity.Property(e => e.Adet).HasDefaultValue(1);
            entity.Property(e => e.Durum)
                .HasMaxLength(50)
                .HasDefaultValue("Hazırlanıyor");
            entity.Property(e => e.KullaniciId).HasColumnName("Kullanici_Id");
            entity.Property(e => e.SiparisTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Siparis_Tarihi");
            entity.Property(e => e.ToplamFiyat)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Toplam_Fiyat");
            entity.Property(e => e.UrunId).HasColumnName("Urun_Id");
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

        modelBuilder.Entity<Slider>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Slider__3214EC078FC780AA");

            entity.ToTable("Slider");

            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.Aktif).HasDefaultValue(true);
            entity.Property(e => e.Baslik).HasMaxLength(200);
            entity.Property(e => e.Sira).HasDefaultValue(0);
        });

        modelBuilder.Entity<Sozlesmeler>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sozlesme__3214EC0783EC3204");

            entity.ToTable("Sozlesmeler");

            entity.Property(e => e.Baslik).HasMaxLength(200);
            entity.Property(e => e.GuncellenmeTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<UrunFoto>(entity =>
        {
            entity.ToTable("Urun_Foto");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ad).HasColumnType("text");
            entity.Property(e => e.UrunId).HasColumnName("Urun_Id");
        });

        modelBuilder.Entity<UrunYorumlari>(entity =>
        {
            entity.ToTable("Urun_Yorumlari");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.KullaniciId).HasColumnName("Kullanici_Id");
            entity.Property(e => e.UrunId).HasColumnName("Urun_Id");
            entity.Property(e => e.Yorum).HasColumnType("text");
        });

        modelBuilder.Entity<Urunler>(entity =>
        {
            entity.ToTable("Urunler");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Aciklama).HasColumnType("text");
            entity.Property(e => e.Fiyat).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FotoId).HasColumnName("Foto_Id");
            entity.Property(e => e.IndirimliFiyat)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Indirimli_Fiyat");
            entity.Property(e => e.KategoriId).HasColumnName("Kategori_Id");
            entity.Property(e => e.MarkaId).HasColumnName("Marka_Id");
            entity.Property(e => e.UrunAdi)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("Urun_Adi");
            entity.Property(e => e.YorumId).HasColumnName("Yorum_Id");
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
