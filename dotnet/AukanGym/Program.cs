using System;

namespace AukanGym
{
    internal class Program
    {
        // adapters [yyyyMMdd] -> lee los pagos del día y los deja en MSMQ. Es la tarea de las 23:00.
        // procesar            -> deja escuchando los traductores.
        static void Main(string[] args)
        {
            var modo = args.Length > 0 ? args[0] : "";

            if (modo == "adapters")
            {
                var fecha = args.Length > 1 ? args[1] : DateTime.Today.ToString("yyyyMMdd");
                new AdapterXml().Ejecutar(fecha);
                new AdapterWeb().Ejecutar();
            }
            else if (modo == "procesar")
            {
                new TraductorXml().Ejecutar();
                new TraductorWeb().Ejecutar();
                new AdapterContable().Ejecutar();
                Console.WriteLine("Presiona ENTER para terminar...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Uso: AukanGym.exe adapters [yyyyMMdd]");
                Console.WriteLine("     AukanGym.exe procesar");
            }
        }
    }
}
