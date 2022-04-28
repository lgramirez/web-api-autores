using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIAutores.Middlewares
{
    public static class LoguearRespuestaHTTPMiddlewareExtensions
    {
        // esta clase de utilidad estatica es para permitirnos exponer el uso del middleware
        // los metodos de extension solo pueden ir en clases estaticas
        public static IApplicationBuilder UseLoguearRespuestaHTTP(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoguearRespuestaHTTPMiddleware>();
        }
    }
    public class LoguearRespuestaHTTPMiddleware
    {
        private readonly RequestDelegate siguiente;
        private readonly ILogger<LoguearRespuestaHTTPMiddleware> logger;

        // con este param podremos invocar a los sgts middlewares de la tuberia
        public LoguearRespuestaHTTPMiddleware(RequestDelegate siguiente, ILogger<LoguearRespuestaHTTPMiddleware> logger)
        {
            this.siguiente = siguiente;
            this.logger = logger;
        }

        // una regla para utilizar esta clase como middleware es que tiene que tener un metodo
        // publico Invoke o InvokeAsync y este metodo debe retornar un task y aceptar como primer
        // param un HTTPContext
        public async Task InvokeAsync(HttpContext contexto)
        {
            using (var ms = new MemoryStream())
            {
                var cuerpoOriginalRespuesta = contexto.Response.Body;
                contexto.Response.Body = ms;

                // con este metodo pasamos al siguiente middleware
                await siguiente(contexto);

                // despues del invoke se hara todo lo siguiente cuando los middlewares esten volviendo a este middleware
                // para finalizar la cadena de procesos

                ms.Seek(0, SeekOrigin.Begin);
                string respuesta = new StreamReader(ms).ReadToEnd();
                ms.Seek(0, SeekOrigin.Begin);

                await ms.CopyToAsync(cuerpoOriginalRespuesta);
                contexto.Response.Body = cuerpoOriginalRespuesta;

                logger.LogInformation(respuesta);
            }
        }
    }
}