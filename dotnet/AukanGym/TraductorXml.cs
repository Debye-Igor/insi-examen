using System;
using System.Messaging;
using System.Xml.Linq;

namespace AukanGym
{
    // Message Translator: pago XML de sucursal -> pago canónico en JSON
    public class TraductorXml
    {
        public void Ejecutar()
        {
            Colas.Escuchar(Colas.SucPagos, Traducir);
        }

        private void Traducir(string xml, MessageQueueTransaction tx)
        {
            var pago = XElement.Parse(xml);

            var canonico = new PagoCanonico
            {
                Rut = ((string)pago.Element("Rut")).Trim(),
                Monto = (long)pago.Element("Monto"),
                Fecha = (string)pago.Attribute("fecha"),
                FormaPago = (string)pago.Element("FormaPago"),
                Origen = "SUC_" + (string)pago.Attribute("sucursal"),
                CodigoAutorizacion = (string)pago.Element("CodigoAutorizacion"),
                Tarjeta = (string)pago.Element("Tarjeta")
            };

            var json = canonico.AJson();
            Colas.Enviar(Colas.Pagos, json, "pago-canonico", tx);
            Console.WriteLine("XML -> " + json);
        }
    }
}
