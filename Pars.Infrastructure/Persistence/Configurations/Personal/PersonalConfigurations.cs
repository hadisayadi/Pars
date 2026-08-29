using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pars.Domain.Entities;

namespace Pars.Infrastructure.Persistence.Configurations.Personal;

public sealed class PersonalConfiguration : IEntityTypeConfiguration<Pars.Domain.Entities.Personal>
{
    public void Configure(EntityTypeBuilder<Pars.Domain.Entities.Personal> b)
    {
        b.ToTable("personal", "dbo"); b.HasKey(x => x.Id);
        b.Property(x=>x.Id).HasColumnName("id").HasMaxLength(10).ValueGeneratedNever();
        b.Property(x=>x.UnitCode).HasColumnName("unitcode").HasMaxLength(50); b.Property(x=>x.Taradod).HasColumnName("taradod").HasMaxLength(50);
        b.Property(x=>x.Estekhdam).HasColumnName("estekhdam").HasMaxLength(10); b.Property(x=>x.DateEstekhdam).HasColumnName("dateestekhdam").HasMaxLength(10);
        b.Property(x=>x.DateTavalod).HasColumnName("datetavalod").HasMaxLength(10); b.Property(x=>x.Madrak).HasColumnName("madrak").HasMaxLength(50);
        b.Property(x=>x.Reshte).HasColumnName("reshte").HasMaxLength(50); b.Property(x=>x.Gerayesh).HasColumnName("gerayesh").HasMaxLength(50);
        b.Property(x=>x.University).HasColumnName("university").HasMaxLength(150); b.Property(x=>x.Jensiat).HasColumnName("jensiat").HasMaxLength(50);
        b.Property(x=>x.TavalodCity).HasColumnName("tavalodcity").HasMaxLength(50); b.Property(x=>x.SokonatCity).HasColumnName("sokonatcity").HasMaxLength(50);
        b.Property(x=>x.TelKar).HasColumnName("telkar").HasMaxLength(50); b.Property(x=>x.TelMob).HasColumnName("telmob").HasMaxLength(50);
        b.Property(x=>x.Email).HasColumnName("email").HasMaxLength(80); b.Property(x=>x.UnitCodeTemp).HasColumnName("unitcodetemp").HasMaxLength(50);
        b.Property(x=>x.Company).HasColumnName("company").HasMaxLength(50); b.Property(x=>x.Pos).HasColumnName("pos").HasMaxLength(50); b.Property(x=>x.PosIndex).HasColumnName("posindex");
        b.Property(x=>x.AddBy).HasColumnName("addby").HasMaxLength(250); b.Property(x=>x.Level1).HasColumnName("level1"); b.Property(x=>x.Shift).HasColumnName("Shift").HasMaxLength(5); b.Property(x=>x.NobatKar).HasColumnName("nobatkar");
        b.Property(x=>x.SizeShalvar).HasColumnName("Size_shalvar").HasMaxLength(10); b.Property(x=>x.SizeKafsh).HasColumnName("Size_kafsh").HasMaxLength(10); b.Property(x=>x.SizeLebas).HasColumnName("Size_lebas").HasMaxLength(10); b.Property(x=>x.SizeBlarsoot).HasColumnName("Size_blarsoot").HasMaxLength(10); b.Property(x=>x.SizeKapshan).HasColumnName("Size_kapshan").HasMaxLength(10);
        b.Property(x=>x.Khedmat).HasColumnName("khedmat").HasMaxLength(50); b.Property(x=>x.EllatMoafiyat).HasColumnName("ellatmoafiyat").HasMaxLength(150); b.Property(x=>x.CodeMelli).HasColumnName("codemelli").HasMaxLength(10); b.Property(x=>x.FirstName).HasColumnName("firstname").HasMaxLength(50); b.Property(x=>x.LastName).HasColumnName("lastname").HasMaxLength(50); b.Property(x=>x.FatherName).HasColumnName("fathername").HasMaxLength(50); b.Property(x=>x.CardNo).HasColumnName("cardno");
    }
}

public sealed class PersonalChildConfiguration : IEntityTypeConfiguration<PersonalChild>
{
    public void Configure(EntityTypeBuilder<PersonalChild> b) { b.ToTable("personalChild","dbo"); b.HasKey(x=>x.Id); b.Property(x=>x.Id).HasColumnName("id"); b.Property(x=>x.Pid).HasColumnName("pid").HasMaxLength(10); b.Property(x=>x.Name).HasColumnName("name1").HasMaxLength(150); b.Property(x=>x.Nesbat).HasColumnName("nesbat").HasMaxLength(50); b.Property(x=>x.Date).HasColumnName("date1").HasMaxLength(10); b.Property(x=>x.Jensiat).HasColumnName("jensiat").HasMaxLength(10); b.Property(x=>x.CodeMelli).HasColumnName("codemelli").HasMaxLength(10); b.Property(x=>x.AddBy).HasColumnName("addby").HasMaxLength(250); b.HasOne(x=>x.Personal).WithMany(x=>x.Children).HasForeignKey(x=>x.Pid).OnDelete(DeleteBehavior.NoAction); }
}

public sealed class PersonalFileConfiguration : IEntityTypeConfiguration<PersonalFile>
{
    public void Configure(EntityTypeBuilder<PersonalFile> b) { b.ToTable("personalFile","dbo"); b.HasKey(x=>x.Id); b.Property(x=>x.Id).HasColumnName("id"); b.Property(x=>x.Pid).HasColumnName("pid").HasMaxLength(10); b.Property(x=>x.FileContent).HasColumnName("file1"); b.Property(x=>x.FileName).HasColumnName("filename").HasMaxLength(250); b.Property(x=>x.AddBy).HasColumnName("addby").HasMaxLength(250); b.Property(x=>x.Nesbat).HasColumnName("nesbat").HasMaxLength(50); /* Legacy FK documentation does not declare pid -> personal; navigation intentionally not constrained. */ b.Ignore(x=>x.Personal); }
}
