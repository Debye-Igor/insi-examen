using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AukanGym
{
    // Modelo canónico de pago. Es el formato único con el que viajan los pagos
    // una vez traducidos, sin importar si vinieron de una sucursal o del sitio web.
    [DataContract]
    public class PagoCanonico
    {
        [DataMember(Name = "rut", Order = 1)] public string Rut { get; set; }
        [DataMember(Name = "monto", Order = 2)] public long Monto { get; set; }
        [DataMember(Name = "fecha", Order = 3)] public string Fecha { get; set; }
        [DataMember(Name = "formaPago", Order = 4)] public string FormaPago { get; set; }
        [DataMember(Name = "origen", Order = 5)] public string Origen { get; set; }

        // EmitDefaultValue en false deja fuera estos campos cuando no hay dato,
        // como en un pago en efectivo.
        [DataMember(Name = "codigoAutorizacion", Order = 6, EmitDefaultValue = false)] public string CodigoAutorizacion { get; set; }
        [DataMember(Name = "tarjeta", Order = 7, EmitDefaultValue = false)] public string Tarjeta { get; set; }

        public string AJson()
        {
            var serializador = new DataContractJsonSerializer(typeof(PagoCanonico));
            using (var stream = new MemoryStream())
            {
                serializador.WriteObject(stream, this);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static PagoCanonico DesdeJson(string json)
        {
            var serializador = new DataContractJsonSerializer(typeof(PagoCanonico));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (PagoCanonico)serializador.ReadObject(stream);
            }
        }
    }
}
