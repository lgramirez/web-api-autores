using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;

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

        public AutoresController(ApplicationDbContext context, ILogger<AutoresController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        [HttpGet] // api/autores
        [HttpGet("/listado")] // /listado
        // usamos nuestro custom filter
        [ServiceFilter(typeof(MiFiltroDeAccion))]
        public async Task<ActionResult<List<Autor>>> Get()
        {
            logger.LogInformation("Estamos obteniendo los autores");
            return await context.Autores.Include(x => x.Libros).ToListAsync();
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

        [HttpGet("{id:int}/{param=persona}")]
        public async Task<ActionResult<Autor>> Get(int id, string param)
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);

            if (autor == null)
            {
                return NotFound();
            }

            return autor;
        }

        [HttpGet("{nombre}")]
        public async Task<ActionResult<Autor>> Get(string nombre)
        {
            throw new NotImplementedException();
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Nombre.Contains(nombre));

            if (autor == null)
            {
                return NotFound();
            }

            return autor;
        }

        [HttpPost]
        // usamos async para trabajar mas eficientemente las conexiones con la DB
        // y devolvemos un Task<ActionResult> porque es un requisito para metodos asincronos
        public async Task<ActionResult> Post(Autor autor)
        {
            // validacion para evitar que se agreguen autores con el mismo nombre
            var existeAutorConElMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autor.Nombre);

            if (existeAutorConElMismoNombre)
            {
                return BadRequest($"Ya existe un autor con el nombre {autor.Nombre}");
            }

            context.Add(autor);
            await context.SaveChangesAsync();
            return Ok();
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