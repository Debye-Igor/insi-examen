using System;

namespace AukanGym
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // La fecha se puede pasar como argumento con formato yyyyMMdd.
            // Sin argumento usa la de hoy, que es como va a correr la tarea de las 23:00.
            var fecha = args.Length > 0 ? args[0] : DateTime.Today.ToString("yyyyMMdd");
            new AdapterXml().Ejecutar(fecha);
        }
    }
}
