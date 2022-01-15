using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    // permite hacer validaciones automaticas a la informacion que reciba este controlador
    [ApiController]
    [Route("api/autores")]
    public class AutoresController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<Autor>> Get()
        {
            return new List<Autor>() {
                new Autor() {Id=1, Nombre="Luis"},
                new Autor() {Id=2, Nombre="Gonzalo"},
            };
        }
    }
}