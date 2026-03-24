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

    public virtual DbSet<ContactMessage> ContactMessages { get; set; }

    public virtual DbSet<Favoriler> Favorilers { get; set; }

    public virtual DbSet<Kategoriler> Kategorilers { get; set; }

    public virtual DbSet<KullaniciAdresleri> KullaniciAdresleris { get; set; }

    public virtual DbSet<Kullanicilar> Kullanicilars { get; set; }

    public virtual DbSet<Kuponlar> Kuponlars { get; set; }

    public virtual DbSet<Markalar> Markalars { get; set; }

    public virtual DbSet<Siparisler> Siparislers { get; set; }

    public virtual DbSet<SiteAyarlari> SiteAyarlaris { get; set; }

    public virtual DbSet<Slider> Sliders { get; set; }

    public virtual DbSet<Sozlesmeler> Sozlesmelers { get; set; }

    public virtual DbSet<StokHareketleri> StokHareketleris { get; set; }

    public virtual DbSet<UrunFoto> UrunFotos { get; set; }

    public virtual DbSet<UrunYorumlari> UrunYorumlaris { get; set; }

    public virtual DbSet<Urunler> Urunlers { get; set; }

    public virtual DbSet<Yoneticiler> Yoneticilers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=DB");

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

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ContactM__3214EC07B9D38400");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Subject).HasMaxLength(200);
        });

        modelBuilder.Entity<Favoriler>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Favorile__3214EC0788E4EF0F");

            entity.ToTable("Favoriler");

            entity.Property(e => e.EklenmeTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
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

        modelBuilder.Entity<KullaniciAdresleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Kullanic__3214EC0778B3AA40");

            entity.ToTable("KullaniciAdresleri");

            entity.Property(e => e.AdSoyad).HasMaxLength(100);
            entity.Property(e => e.AdresBasligi).HasMaxLength(50);
            entity.Property(e => e.Ilce).HasMaxLength(50);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.PostaKodu).HasMaxLength(10);
            entity.Property(e => e.Sehir).HasMaxLength(50);
            entity.Property(e => e.Telefon).HasMaxLength(20);
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

        modelBuilder.Entity<Kuponlar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Kuponlar__3214EC07652A09F4");

            entity.ToTable("Kuponlar");

            entity.HasIndex(e => e.KuponKodu, "UQ__Kuponlar__F552D46F673DD59E").IsUnique();

            entity.Property(e => e.BitisTarihi).HasColumnType("datetime");
            entity.Property(e => e.Durum).HasDefaultValue(true);
            entity.Property(e => e.KullanilanAdet).HasDefaultValue(0);
            entity.Property(e => e.KuponKodu).HasMaxLength(50);
            entity.Property(e => e.TanimliMail).HasMaxLength(100);
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

            entity.HasIndex(e => e.SiparisNo, "IX_Siparisler_SiparisNo");

            entity.Property(e => e.Adet).HasDefaultValue(1);
            entity.Property(e => e.Durum)
                .HasMaxLength(50)
                .HasDefaultValue("Hazırlanıyor");
            entity.Property(e => e.KullaniciId).HasColumnName("Kullanici_Id");
            entity.Property(e => e.MisafirAdSoyad).HasMaxLength(250);
            entity.Property(e => e.MisafirAdres).HasMaxLength(500);
            entity.Property(e => e.MisafirEmail).HasMaxLength(100);
            entity.Property(e => e.MisafirTelefon).HasMaxLength(20);
            entity.Property(e => e.SiparisNo).HasMaxLength(50);
            entity.Property(e => e.SiparisTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Siparis_Tarihi");
            entity.Property(e => e.TalepTarihi).HasColumnType("datetime");
            entity.Property(e => e.ToplamFiyat)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Toplam_Fiyat");
            entity.Property(e => e.UrunId).HasColumnName("Urun_Id");
            entity.Property(e => e.UygulananKupon).HasMaxLength(50);
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

        modelBuilder.Entity<StokHareketleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StokHare__3214EC07D2DF21FD");

            entity.ToTable("StokHareketleri");

            entity.Property(e => e.AdetDegisimi).HasDefaultValue(0);
            entity.Property(e => e.EskiAlisFiyat)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IslemTipi).HasMaxLength(100);
            entity.Property(e => e.KayitTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.YeniAlisFiyat)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
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
            entity.Property(e => e.AlisFiyati).HasColumnType("decimal(18, 2)");
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
