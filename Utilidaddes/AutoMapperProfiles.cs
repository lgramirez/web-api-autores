using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebAPIAutores.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // el primer param es de donde a donde quiero mapear
            // en este caso de AutorCreacionDTO a Autor
            CreateMap<AutorCreacionDTO, Autor>();
            CreateMap<Autor, AutorDTO>();

            // creamos una regla especial para el atributo AutoresLibros de la entidad libros,
            // estamos configurando el mapeo a la propiedad AutoresLibros
            CreateMap<LibroCreacionDTO, Libro>().ForMember(libro => libro.AutoresLibros, opciones => opciones.MapFrom(MapAutoresLibros));
            CreateMap<Libro, LibroDTO>()
                .ForMember(libroDTO => libroDTO.Autores, opciones => opciones.MapFrom(MapLibroDTOAutores));

            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<Comentario, ComentarioDTO>();
        }

        // logica para mapear de un autorLibro a un Autor para el libroDTO
        private List<AutorDTO> MapLibroDTOAutores(Libro libro, LibroDTO libroDTO)
        {
            var resultado = new List<AutorDTO>();

            if (libro.AutoresLibros == null) { return resultado; }

            foreach (var autorLibro in libro.AutoresLibros)
            {
                resultado.Add(new AutorDTO()
                {
                    Id = autorLibro.AutorId,
                    Nombre = autorLibro.Autor.Nombre
                });
            }
            
            return resultado;
        }

        // logica para mapear desde un entero hacia AutoresLibros
        private List<AutorLibro> MapAutoresLibros(LibroCreacionDTO libroCreacionDTO, Libro libro)
        {
            var resultado = new List<AutorLibro>();

            if (libroCreacionDTO.AutoresIds == null) { return resultado; }

            foreach (var autorId in libroCreacionDTO.AutoresIds)
            {
                resultado.Add(new AutorLibro() { AutorId = autorId });
            }

            return resultado;
        }
    }
}