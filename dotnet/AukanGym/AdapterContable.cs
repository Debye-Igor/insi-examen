using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace AukanGym
{
    // Adapter del sistema contable.
    // Consume los pagos canónicos, los registra por SOAP y publica el estado de cuenta del cliente.
    public class AdapterContable
    {
        private const string Url = "http://localhost:5001/ContabilidadService";
        private const string AccionSoap = "http://tempuri.org/IContabilidadService/RegistrarPago";
        private static readonly HttpClient Cliente = new HttpClient();

        // Lo que devuelve el sistema contable por cada pago registrado
        [DataContract]
        public class EstadoCuenta
        {
            [DataMember(Name = "clienteId", Order = 1)] public string ClienteId { get; set; }
            [DataMember(Name = "saldo", Order = 2)] public decimal Saldo { get; set; }

            public string AJson()
            {
                var serializador = new DataContractJsonSerializer(typeof(EstadoCuenta));
                using (var stream = new MemoryStream())
                {
                    serializador.WriteObject(stream, this);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
        }

        public void Ejecutar()
        {
            Colas.Escuchar(Colas.Pagos, Procesar);
        }

        private void Procesar(string json, MessageQueueTransaction tx)
        {
            var pago = PagoCanonico.DesdeJson(json);
            var estado = RegistrarPago(pago.Rut, pago.Monto);
            if (estado == null) return;

            var salida = estado.AJson();

            // ActiveXMessageFormatter porque de esta cola el mensaje lo saca el Trigger de MSMQ,
            // que entrega el cuerpo como texto al programa del Bridge.
            using (var cola = new MessageQueue(Colas.EstadoCuenta))
            {
                var mensaje = new Message(salida, new ActiveXMessageFormatter())
                {
                    Label = "estado-cuenta",
                    Recoverable = true
                };
                cola.Send(mensaje, tx);
            }
            Console.WriteLine("Estado de cuenta: " + salida);
        }

        private EstadoCuenta RegistrarPago(string rut, long monto)
        {
            var sobre =
                "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                  "<s:Body>" +
                    "<RegistrarPago xmlns=\"http://tempuri.org/\">" +
                      "<clienteId>" + SecurityElement.Escape(rut) + "</clienteId>" +
                      "<monto>" + monto + "</monto>" +
                    "</RegistrarPago>" +
                  "</s:Body>" +
                "</s:Envelope>";

            var peticion = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = new StringContent(sobre, Encoding.UTF8, "text/xml")
            };
            peticion.Headers.Add("SOAPAction", "\"" + AccionSoap + "\"");

            var respuesta = Cliente.SendAsync(peticion).Result;
            var documento = XDocument.Parse(respuesta.Content.ReadAsStringAsync().Result);

            // Si el sistema contable rechaza el pago, por ejemplo con un RUT que no conoce,
            // se deja registrado y el mensaje no se reintenta para siempre.
            var falla = documento.Descendants().FirstOrDefault(e => e.Name.LocalName == "Fault");
            if (falla != null)
            {
                Console.Error.WriteLine("El contable rechazó el pago de " + rut + ": " + falla.Value);
                return null;
            }

            return new EstadoCuenta
            {
                ClienteId = Valor(documento, "ClienteId"),
                Saldo = decimal.Parse(Valor(documento, "Saldo"), CultureInfo.InvariantCulture)
            };
        }

        private static string Valor(XDocument documento, string nombre)
        {
            return documento.Descendants().First(e => e.Name.LocalName == nombre).Value;
        }
    }
}
