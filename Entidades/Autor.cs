using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiAutores.Entidades
{
    public class Autor
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(maximumLength: 5, ErrorMessage = "El campo {0} no debe tener mas de {1} caracteres.")]
        public string Nombre { get; set; }

        [Range(18, 120)]
        // el no mapped nos permite decirle que no necesitamos este atributo en la tabla Autor
        [NotMapped]
        public int Edad { get; set; }

        // propiedad de navegacion
        public List<Libro> Libros { get; set; }
    }
}