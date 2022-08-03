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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Controllers
{
    // permite hacer validaciones automaticas a la informacion que reciba este controlador
    [ApiController]
    [Route("api/autores")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    // [Route("api/[controller]")] otra opcion para que este placeholder controller se modifique con el nombre del controlador
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<AutoresController> logger;
        private readonly IMapper mapper;
        private readonly IAuthorizationService authorizationService;

        public AutoresController(
            ApplicationDbContext context,
            ILogger<AutoresController> logger,
            IMapper mapper,
            IAuthorizationService authorizationService
            )
        {
            this.context = context;
            this.logger = logger;
            this.mapper = mapper;
            this.authorizationService = authorizationService;
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

        [HttpGet(Name = "ObtenerAutores")] // api/autores
        // permitimos que usuarios no autorizados puedan consumir este endpoint
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromQuery] bool incluirHATEOAS = true)
        {
            var autores = await context.Autores.ToListAsync();
            var dtos = mapper.Map<List<AutorDTO>>(autores);

            if (incluirHATEOAS)
            {
                var esAdmin = await authorizationService.AuthorizeAsync(User, "esAdmin");
                // dtos.ForEach(dto => GenerarEnlaces(dto, esAdmin.Succeeded));

                var resultado = new ColeccionDeRecursos<AutorDTO> { Valores = dtos };
                resultado.Enlaces.Add(new DatoHATEOAS(
                    enlace: Url.Link("ObtenerAutores", new { }),
                    descripcion: "self",
                    metodo: "GET"
                ));

                if (esAdmin.Succeeded)
                {
                    resultado.Enlaces.Add(new DatoHATEOAS(
                        enlace: Url.Link("CrearAutor", new { }),
                        descripcion: "crear-autor",
                        metodo: "POST"
                    ));
                }

                return Ok(resultado);  
            }
            
            return Ok(dtos);
        }

        [HttpGet("{id:int}", Name = "ObtenerAutor")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<AutorDTOConLibros>> Get(int id, [FromHeader] string incluirHATEOAS)
        {
            var autor = await context.Autores
                .Include(autorDB => autorDB.AutoresLibros)
                .ThenInclude(autorLibroDB => autorLibroDB.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (autor == null)
            {
                return NotFound();
            }

            var dto = mapper.Map<AutorDTOConLibros>(autor);
            return dto;
        }

        [HttpGet("{nombre}", Name = "ObtenerAutorPorNombre")]
        public async Task<ActionResult<List<AutorDTO>>> Get(string nombre)
        {
            var autores = await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();

            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost(Name = "CrearAutor")]
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

        [HttpPut("{id:int}", Name = "ActualizarAutor")] // api/autores/1
        public async Task<ActionResult> Put(AutorCreacionDTO autorCreacionDTO, int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            context.Update(autor);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "BorrarAutor")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}