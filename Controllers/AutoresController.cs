using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;
using WebApiAutores.DTOs;

namespace WebApiAutores.Controllers
{
    // permite hacer validaciones automaticas a la informacion que reciba este controlador
    [ApiController]
    [Route("api/autores")]
    // [Route("api/[controller]")] otra opcion para que este placeholder controller se modifique con el nombre del controlador
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<AutoresController> logger;
        private readonly IMapper mapper;

        public AutoresController(ApplicationDbContext context, ILogger<AutoresController> logger, IMapper mapper)
        {
            this.context = context;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet("primero")] // api/autores/primero
        // con este filtro habilitamos el cache para este metodo, lo que hace es 
        // que la primera llamada a este endpoint todo respondera como deberia ser,
        // pero las sgts llamadas dentro de los 10s sacaran la informacion del cache
        [ResponseCache(Duration = 10)]
        // este filtro se usa para autorizar que no todos usen este endpoint
        [Authorize]
        public async Task<ActionResult<Autor>> PrimerAutor()
        {
            return await context.Autores.FirstOrDefaultAsync();
        }

        [HttpGet] // api/autores
        public async Task<List<AutorDTO>> Get()
        {
            var autores = await context.Autores.ToListAsync();
            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpGet("{id:int}", Name = "ObtenerAutor")]
        public async Task<ActionResult<AutorDTOConLibros>> Get(int id)
        {
            var autor = await context.Autores
                .Include(autorDB => autorDB.AutoresLibros)
                .ThenInclude(autorLibroDB => autorLibroDB.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (autor == null)
            {
                return NotFound();
            }

            return mapper.Map<AutorDTOConLibros>(autor);
        }

        [HttpGet("{nombre}")]
        public async Task<ActionResult<List<AutorDTO>>> Get(string nombre)
        {
            var autores = await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();

            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost]
        // usamos async para trabajar mas eficientemente las conexiones con la DB
        // y devolvemos un Task<ActionResult> porque es un requisito para metodos asincronos
        public async Task<ActionResult> Post(AutorCreacionDTO autorCreacionDTO)
        {
            // validacion para evitar que se agreguen autores con el mismo nombre
            var existeAutorConElMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTO.Nombre);

            if (existeAutorConElMismoNombre)
            {
                return BadRequest($"Ya existe un autor con el nombre {autorCreacionDTO.Nombre}");
            }

            // en este caso le estoy pasando como param un autorCreacionDTO para convertirlo en Autor
            var autor = mapper.Map<Autor>(autorCreacionDTO);

            // es una forma de solucionar el problema de que tenemos que mandar un autor a context.add()
            // var autor = new Autor()
            // {
            //     Nombre = autorCreacionDTO.nombre;
            // }

            context.Add(autor);
            await context.SaveChangesAsync();

            var autorDTO = mapper.Map<AutorDTO>(autor);

            // con el siguiente metodo retornamos los valores esperados con las buenas practicas para
            // un POST request de un API que son: 1. la ruta donde puedo obtener la informacion del autor
            // creado, 2. y retonamos la informacion del autor creado
            // este metodo recibe 3 params: 1. el nombre de la ruta que usaremos, 2. le mandamos el param
            // que necesita esta ruta y 3. los datos del autor creado
            return CreatedAtRoute("ObtenerAutor", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}")] // api/autores/1
        public async Task<ActionResult> Put(Autor autor, int id)
        {
            if (autor.Id != id)
            {
                return BadRequest("El ID del autor no coincide con el ID de la URL.");
            }

            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Update(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}