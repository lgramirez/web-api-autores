using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace WebApiAutores.Servicios
{
    public class EscribirEnArchivo : IHostedService
    {
        private readonly IWebHostEnvironment env;
        private readonly string nombreArchivo = "Archivo 1.txt";
        private Timer timer;

        public EscribirEnArchivo(IWebHostEnvironment env)
        {
            this.env = env;
        }
        // aqui tenemos funcionalidad que se ejecutara cuando carguemos el web api
        //  una sola vez
        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            Escribir("Proceso iniciado");
            return Task.CompletedTask;
        }

        // este metodo se ejecutara cuando apaguemos nuestro web api
        // en ciertas ocasiones puede ser que ni se ejecute este metodo
        // ejm: si la app se detiene por un error catastrofico
        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Dispose();
            Escribir("Proceso finalizado");
            return Task.CompletedTask;
        }

        private void DoWork(Object state)
        {
            Escribir("Proceso en ejecucion: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"));
        }

        public void Escribir(string mensaje)
        {
            // la ruta wwwroot es una ruta especial desde donde se sirven tipicamente archivos estaticos
            var ruta = $@"{env.ContentRootPath}\wwwroot\{nombreArchivo}";
            // append se usa para decirle que escriba en el mismo archivo sin sustuir el archivo anterior
            using (StreamWriter writer = new StreamWriter(ruta, append: true))
            {
                writer.WriteLine(mensaje);
            }

        }
    }
}