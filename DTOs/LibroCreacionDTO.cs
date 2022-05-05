using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs
{
    public class LibroCreacionDTO
    {
        [PrimeraLetraMayuscula]
        [StringLength(maximumLength: 250)]
        public string Titulo { get; set; }
    }   
}