using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiAutores.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.Entidades
{
    public class Comentario
    {
        public int Id { get; set; }
        public string Contenido { get; set; }
        public int LibroId { get; set; }

        // propiedad de navegacion nos permite pasar de una entidad a otra entidad relacionada
        // nos permiten hacer joins de maneras sencillas y asi de esta manera, en este caso,
        // poder traer la informacion del libro de un comentario
        public Libro Libro { get; set; }
    }
}