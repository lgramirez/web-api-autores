using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;
using WebApiAutores.DTOs;
using Microsoft.AspNetCore.JsonPatch;

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

        [HttpGet("{id:int}", Name = "ObtenerLibro")]
        public async Task<ActionResult<LibroDTOConAutores>> Get(int id)
        {
            // usamos el include para que se incluyan los comentarios en la respuesta del libro seleccionado
            // pero en este caso lo dejaremos sin los comentarios para ser mas eficientes y solo usar el
            // endpoint de comentarios para traer los comentarios de un libro
            // al incluir el .Include debemos tambien modificar el DTO para que adicione la prop comentarios
            // var libro = await context.Libros.Include(libroDB => libroDB.Comentarios).FirstOrDefaultAsync(x => x.Id == id);
            var libro = await context.Libros.Include(libroDB => libroDB.AutoresLibros).ThenInclude(autorLibroDB => autorLibroDB.Autor).FirstOrDefaultAsync(x => x.Id == id);

            if (libro == null)
            {
                return NotFound();
            }

            // ordenar segun el campo orden
            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();

            return mapper.Map<LibroDTOConAutores>(libro);
        }

        [HttpPost(Name = "CrearLibro")]
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

            AsignarOrdenAutores(libro);

            context.Add(libro);
            await context.SaveChangesAsync();

            var libroDTO = mapper.Map<LibroDTO>(libro);

            return CreatedAtRoute("ObtenerLibro", new { id = libro.Id }, libroDTO);
        }

        [HttpPut("{id:int}", Name = "ActualizarLibro")]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
        {
            // traemos el libro de la DB que tiene el id que estamos recibiendo por parametro
            // y tambien estamos incluyendo el listado de autoresLibros para poder actualizarlo
            // ya sea creando, eliminando o modificando el orden de los autores
            var libroDB = await context.Libros.Include(x => x.AutoresLibros).FirstOrDefaultAsync(x => x.Id == id);

            if (libroDB == null)
            {
                return NotFound();
            }

            // vamos a usar automapper para llevar las propiedades de libroCreacionDTO a libroDB
            // y se va a hacer una actualizacion de libroDB y lo asignamos a libroDB para mantener
            // la misma instancia creada al principio, asi nos permite actualizar la entidad libro y
            // tambien autoresLibros sin tener que hacer mucho
            libroDB = mapper.Map(libroCreacionDTO, libroDB);
            AsignarOrdenAutores(libroDB);

            await context.SaveChangesAsync();
            return NoContent();
        }

        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }
        }

        [HttpPatch("{id:int}", Name = "PatchLibro")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var libroDB = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);

            if (libroDB == null)
            {
                return NotFound();
            }

            // llenar el libroPatchDTO con la informacion del libro de la DB
            var libroDTO = mapper.Map<LibroPatchDTO>(libroDB);

            // luego aplicamos a libroPatchDTO los cambios que vinieron de patchDocument
            // por ejm el cambio del titulo
            patchDocument.ApplyTo(libroDTO, ModelState);

            var esValido = TryValidateModel(libroDTO);
            // en el ModelState van a estar los errores de validacion encontrados
            if (!esValido)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(libroDTO, libroDB);
            await context.SaveChangesAsync();
            return NoContent();
        }

        // al borrar el libro se borran las relaciones con autoresLibros y los comentarios
        // que tenia este libro, pero se mantienen los autores que estaban relacionados al libro
        [HttpDelete("{id:int}", Name = "BorrarLibro")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Libros.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Libro() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}