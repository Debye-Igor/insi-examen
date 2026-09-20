using System;
using System.Messaging;
using System.Threading;

namespace AukanGym
{
    // Nombres de las colas y las dos operaciones que todos los componentes repiten
    public static class Colas
    {
        public const string SucPagos = @".\private$\dig_suc_pagos";
        public const string WebPagos = @".\private$\dig_web_pagos";
        public const string Pagos = @".\private$\dig_pagos";
        public const string EstadoCuenta = @".\private$\dig_estado_cuenta";

        // Define cómo se convierte el cuerpo del mensaje en bytes. Emisor y receptor usan el mismo.
        public static IMessageFormatter Texto()
        {
            return new XmlMessageFormatter(new[] { typeof(string) });
        }

        // Envía dentro de una transacción que ya está abierta
        public static void Enviar(string cola, string cuerpo, string etiqueta, MessageQueueTransaction tx)
        {
            using (var mq = new MessageQueue(cola))
            {
                var mensaje = new Message(cuerpo, Texto()) { Label = etiqueta, Recoverable = true };
                mq.Send(mensaje, tx);
            }
        }

        // Envía abriendo su propia transacción
        public static void Enviar(string cola, string cuerpo, string etiqueta)
        {
            using (var tx = new MessageQueueTransaction())
            {
                tx.Begin();
                Enviar(cola, cuerpo, etiqueta, tx);
                tx.Commit();
            }
        }

        // Escucha una cola en un hilo aparte.
        // Recibir y enviar ocurren en la misma transacción: si algo falla, el mensaje vuelve a la cola.
        public static void Escuchar(string cola, Action<string, MessageQueueTransaction> procesar)
        {
            var hilo = new Thread(() =>
            {
                var mq = new MessageQueue(cola) { Formatter = Texto() };
                Console.WriteLine("Escuchando " + cola);
                while (true)
                {
                    using (var tx = new MessageQueueTransaction())
                    {
                        try
                        {
                            tx.Begin();
                            var mensaje = mq.Receive(tx);
                            procesar((string)mensaje.Body, tx);
                            tx.Commit();
                        }
                        catch (Exception e)
                        {
                            tx.Abort();
                            Console.Error.WriteLine("Error en " + cola + ": " + e.Message);
                            Thread.Sleep(5000);
                        }
                    }
                }
            });
            hilo.IsBackground = true;
            hilo.Start();
        }
    }
}
