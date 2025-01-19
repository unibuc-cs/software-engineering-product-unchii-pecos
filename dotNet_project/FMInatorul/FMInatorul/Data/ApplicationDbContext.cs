using FMInatorul.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;

namespace FMInatorul.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Profesor> Professors { get; set; }
    public DbSet<Student> Students { get; set; }

    public DbSet<Materie> Materii {  get; set; }

    public DbSet<IntrebariRasp> IntrebariRasps { get; set; }
    public DbSet<Variante> Variantes { get; set; }
    public DbSet<Facultate> Facultati { get; set; }

    // room
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Participant> Participants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //relatie one to many pentru Materie si Intrebari
        modelBuilder.Entity<IntrebariRasp>()
            .HasOne<Materie>(a => a.Materie)
            .WithMany(c => c.IntrebariRasp)
            .HasForeignKey(a => a.MaterieId);
        

        //relatia one to many dintre IntrebariRasp si Variante
        modelBuilder.Entity<Variante>()
            .HasOne<IntrebariRasp>(a => a.IntrebariRasp)
            .WithMany(c => c.Variante)
            .HasForeignKey(a => a.IntrebariRaspId);
    }
}