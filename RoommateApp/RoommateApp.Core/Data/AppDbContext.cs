using Microsoft.EntityFrameworkCore;
using RoommateApp.Core.Models;

namespace RoommateApp.Core.Data {
    public class AppDbContext : DbContext {
        public DbSet<Uzivatel> Uzivatele { get; set; }
        public DbSet<Skupina> Skupiny { get; set; }
        public DbSet<Vydaj> Vydaje { get; set; }
        public DbSet<Dluh> Dluhy { get; set; }
        public DbSet<Clenstvi> Clenstvi { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            
            // Composite key pro členství - uživatel může být v každé skupině pouze jednou
            modelBuilder.Entity<Clenstvi>()
                .HasKey(c => new { c.UzivatelId, c.SkupinaId });
            
            // Zamezení cascade delete pro dluhy - musíme je explicitně smazat
            modelBuilder.Entity<Dluh>()
                .HasOne(d => d.Dluznik)
                .WithMany()
                .HasForeignKey(d => d.DluznikId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Dluh>()
                .HasOne(d => d.Veritel)
                .WithMany()
                .HasForeignKey(d => d.VeritelId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}