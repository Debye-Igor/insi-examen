using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AukanGym
{
    // Channel Adapter del sitio web.
    // Consulta los pagos del día por REST y publica cada uno en MSMQ, en su JSON original.
    public class AdapterWeb
    {
        private const string Url = "http://localhost:5000/api/payments/today";

        // Un pago tal como lo entrega la API del sitio web
        [DataContract]
        public class PagoWeb
        {
            [DataMember(Name = "identifier", Order = 1)] public string Identifier { get; set; }
            [DataMember(Name = "amount", Order = 2)] public long Amount { get; set; }
            [DataMember(Name = "paymentType", Order = 3)] public string PaymentType { get; set; }
            [DataMember(Name = "authorizationCode", Order = 4)] public string AuthorizationCode { get; set; }
            [DataMember(Name = "card", Order = 5)] public string Card { get; set; }
        }

        public void Ejecutar()
        {
            string json;
            using (var cliente = new HttpClient())
            {
                json = cliente.GetStringAsync(Url).Result;
            }

            var serializador = new DataContractJsonSerializer(typeof(List<PagoWeb>));
            List<PagoWeb> pagos;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                pagos = (List<PagoWeb>)serializador.ReadObject(stream);
            }

            foreach (var pago in pagos)
            {
                Colas.Enviar(Colas.WebPagos, AJson(pago), "pago-web");
            }
            Console.WriteLine("Pagos web enviados: " + pagos.Count);
        }

        private string AJson(PagoWeb pago)
        {
            var serializador = new DataContractJsonSerializer(typeof(PagoWeb));
            using (var stream = new MemoryStream())
            {
                serializador.WriteObject(stream, pago);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

    }
}
