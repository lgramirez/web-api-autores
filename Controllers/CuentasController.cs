using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApiAutores.DTOs;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/cuentas")]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;

        // inyectamos el servicio que nos permite registrar un usuario
        public CuentasController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        [HttpPost("registrar")] // api/cuentas/registrar
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar([FromBody] CredencialUsuario credencialUsuario)
        {
            var usuario = new IdentityUser { UserName = credencialUsuario.Email, Email = credencialUsuario.Email };
            var resultado = await userManager.CreateAsync(usuario, credencialUsuario.Password);

            if (resultado.Succeeded)
            {
                return ConstruirToken(credencialUsuario);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private RespuestaAutenticacion ConstruirToken(CredencialUsuario credencialUsuario)
        {
            // un claim es una informacion del usuario en la cual podemos confiar
            // es una informacion emitida por una fuente en la cual nosotros confiamos
            // esta informacion estara en el token JWT
            var claims = new List<Claim>()
            {
                new Claim("email", credencialUsuario.Email)
            };

            // firmamos nuestro JWT
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llaveJWT"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            // construimos el token
            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }
    }
}