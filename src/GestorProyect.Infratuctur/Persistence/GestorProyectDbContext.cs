using GestorProyect.models.entities;
using Microsoft.EntityFrameworkCore;

namespace GestorProyect.Infratuctur.Persistence;

public sealed class GestorProyectDbContext(DbContextOptions<GestorProyectDbContext> options)
    : DbContext(options)
{
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<Tarea> Tareas => Set<Tarea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Proyecto>(entity =>
        {
            entity.ToTable("Proyectos");
            entity.HasKey(proyecto => proyecto.Id);
            entity.Property(proyecto => proyecto.Nombre).IsRequired();
            entity.Property(proyecto => proyecto.Descripcion).IsRequired();
            entity.Property(proyecto => proyecto.FechaInicio).HasColumnType("date");
            entity.Property(proyecto => proyecto.FechaFinalizacion).HasColumnType("date");
        });

        modelBuilder.Entity<Tarea>(entity =>
        {
            entity.ToTable("Tareas");
            entity.HasKey(tarea => tarea.Id);
            entity.Property(tarea => tarea.Titulo).IsRequired();
            entity.Property(tarea => tarea.Descripcion).IsRequired();
            entity.Property(tarea => tarea.Prioridad).IsRequired();
            entity.Property(tarea => tarea.FechaLimite).HasColumnType("date");
            entity.HasOne<Proyecto>()
                .WithMany()
                .HasForeignKey(tarea => tarea.ProyectoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
