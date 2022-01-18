using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Entidades
{
    public class Autor : IValidatableObject
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(maximumLength: 5, ErrorMessage = "El campo {0} no debe tener mas de {1} caracteres.")]
        // [PrimeraLetraMayuscula]
        public string Nombre { get; set; }

        [Range(18, 120)]
        // el no mapped nos permite decirle que no necesitamos este atributo en la tabla Autor
        [NotMapped]
        public int Edad { get; set; }

        public int Menor { get; set; }
        public int Mayor { get; set; }

        // propiedad de navegacion
        public List<Libro> Libros { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Nombre))
            {
                var primeraLetra = Nombre[0].ToString();

                if (primeraLetra != primeraLetra.ToUpper())
                {
                    // ya que usamos un IEnumerable, usamos el yield para agregar este resultado de validacion a la collecion de
                    // resultados de validacion, para cada validacion que queramos agregar al IEnumerable debemos usar el yield

                    // segundo param es el campo que tiene el error
                    yield return new ValidationResult("La primera letra debe ser mayuscula.", new string[] { nameof(Nombre) });
                }
            }

            if (Menor > Mayor)
            {
                yield return new ValidationResult("Este valor no puede ser mas grande que el campo mayor", new string[] { nameof(Menor) });
            }
        }
    }
}