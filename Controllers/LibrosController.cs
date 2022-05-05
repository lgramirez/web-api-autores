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

        [HttpGet("{id}")]
        public async Task<ActionResult<LibroDTO>> Get(int id)
        {
            var libro = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);
            return mapper.Map<LibroDTO>(libro);
        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            // var existeAutor = await context.Autores.AnyAsync();

            // if (!existeAutor)
            // {
            //     return BadRequest($"No existe el autor de ID: {libro.AutorId}");
            // }
            
            var libro = mapper.Map<Libro>(libroCreacionDTO);

            context.Add(libro);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}