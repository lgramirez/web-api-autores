using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiAutores.DTOs
{
    public class LibroDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }

        // debemos agregar esta propiedad si queremos que se muestren los comentarios de un libro
        //public List<ComentarioDTO> Comentarios { get; set; }
    }
}