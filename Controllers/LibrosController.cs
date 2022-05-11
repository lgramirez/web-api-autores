using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;
using WebApiAutores.DTOs;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libros")]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LibroDTO>> Get(int id)
        {
            // usamos el include para que se incluyan los comentarios en la respuesta del libro seleccionado
            // pero en este caso lo dejaremos sin los comentarios para ser mas eficientes y solo usar el
            // endpoint de comentarios para traer los comentarios de un libro
            // al incluir el .Include debemos tambien modificar el DTO para que adicione la prop comentarios
            // var libro = await context.Libros.Include(libroDB => libroDB.Comentarios).FirstOrDefaultAsync(x => x.Id == id);
            var libro = await context.Libros.Include(libroDB => libroDB.AutoresLibros).ThenInclude(autorLibroDB => autorLibroDB.Autor).FirstOrDefaultAsync(x => x.Id == id);

            // ordenar segun el campo orden
            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();

            return mapper.Map<LibroDTO>(libro);
        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds == null)
            {
                return BadRequest("No se puede crear un libro sin autores.");
            }

            // ir a la tabla autores y mediante un query ver si el id del autor se encuentra en el listado de IDs que estoy pasando
            // .Select le dice que solo me traiga el dato de los IDs sin el resto de informacion del autor
            var autoresIds = await context.Autores.Where(autorDB => libroCreacionDTO.AutoresIds.Contains(autorDB.Id)).Select(x => x.Id).ToListAsync();

            if (libroCreacionDTO.AutoresIds.Count != autoresIds.Count)
            {
                return BadRequest("No existe uno de los autores enviados.");
            }

            var libro = mapper.Map<Libro>(libroCreacionDTO);

            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }

            context.Add(libro);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}