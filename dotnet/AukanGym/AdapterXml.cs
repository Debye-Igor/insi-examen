using System;
using System.IO;
using System.Messaging;
using System.Xml.Linq;

namespace AukanGym
{
    // Channel Adapter de las sucursales.
    // Lee los XML del día y publica cada pago como un mensaje independiente en MSMQ.
    public class AdapterXml
    {
        private const string Carpeta = @"C:\AukanGym\PagosXML";
        private const string Cola = @".\private$\dig_suc_pagos";

        public void Ejecutar(string fecha)
        {
            var archivos = Directory.GetFiles(Carpeta, "suc_*-pagos-" + fecha + ".xml");
            Console.WriteLine("Archivos del " + fecha + ": " + archivos.Length);

            foreach (var archivo in archivos)
            {
                // suc_001-pagos-20260120.xml -> 001
                var sucursal = Path.GetFileName(archivo).Substring(4, 3);
                var documento = XDocument.Load(archivo);
                var fechaPagos = (string)documento.Root.Attribute("fecha");
                var enviados = 0;

                foreach (var pago in documento.Root.Elements("Pago"))
                {
                    // La sucursal viene en el nombre del archivo y la fecha en el elemento Pagos.
                    // Se agregan al pago para no perderlas al separarlo.
                    pago.SetAttributeValue("sucursal", sucursal);
                    pago.SetAttributeValue("fecha", fechaPagos);

                    Enviar(pago.ToString());
                    enviados++;
                }

                Console.WriteLine(Path.GetFileName(archivo) + ": " + enviados + " pago(s) enviados");
            }
        }

        private void Enviar(string xmlPago)
        {
            using (var tx = new MessageQueueTransaction())
            {
                try
                {
                    tx.Begin();
                    using (var cola = new MessageQueue(Cola))
                    {
                        var mensaje = new Message(xmlPago, new XmlMessageFormatter(new[] { typeof(string) }))
                        {
                            Label = "pago-sucursal",
                            Recoverable = true
                        };
                        cola.Send(mensaje, tx);
                    }
                    tx.Commit();
                }
                catch (Exception e)
                {
                    tx.Abort();
                    Console.Error.WriteLine("No se pudo enviar el pago: " + e.Message);
                }
            }
        }
    }
}
