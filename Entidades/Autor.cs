using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Entidades
{
    public class Autor
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(maximumLength: 5, ErrorMessage = "El campo {0} no debe tener mas de {1} caracteres.")]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
    }
}