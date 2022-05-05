using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;

namespace WebApiAutores
{
    public class ApplicationDbContext : DbContext
    {
        // a traves de este constructor puedo pasarle distintas configuraciones a EF
        // como ser el connection string que apunta a la DB que vamos a usar
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        // le a EF que cree una tabla llamada Autores con el modelo Autor
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }

        // para crear una migracion
        // dotnet ef migrations add Inicial
        // Inicial -> es el nombre de la migracion

        // para correr la migracion
        // dotnet ef database update

        // para crear una migracion para los comentarios
        // dotnet ef migrations add Comentarios
    }
}