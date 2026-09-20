using System;
using System.IO;
using System.Messaging;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AukanGym
{
    // Message Translator: pago JSON del sitio web -> pago canónico en JSON
    public class TraductorWeb
    {
        public void Ejecutar()
        {
            Colas.Escuchar(Colas.WebPagos, Traducir);
        }

        private void Traducir(string jsonWeb, MessageQueueTransaction tx)
        {
            AdapterWeb.PagoWeb pago;
            var serializador = new DataContractJsonSerializer(typeof(AdapterWeb.PagoWeb));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonWeb)))
            {
                pago = (AdapterWeb.PagoWeb)serializador.ReadObject(stream);
            }

            var canonico = new PagoCanonico
            {
                Rut = pago.Identifier,
                Monto = pago.Amount,
                // La API no entrega fecha y el adapter consulta solo los pagos de hoy
                Fecha = DateTime.Today.ToString("yyyy-MM-dd"),
                FormaPago = FormaPago(pago.PaymentType),
                Origen = "WEB",
                CodigoAutorizacion = pago.AuthorizationCode,
                Tarjeta = pago.Card
            };

            var json = canonico.AJson();
            Colas.Enviar(Colas.Pagos, json, "pago-canonico", tx);
            Console.WriteLine("Web -> " + json);
        }

        // El sitio web nombra las formas de pago en inglés. El canónico usa los códigos de las sucursales.
        private static string FormaPago(string tipo)
        {
            if (tipo == "Credit Card") return "TC";
            if (tipo == "Debit Card") return "TD";
            if (tipo == "Cash") return "EF";
            return tipo;
        }
    }
}
