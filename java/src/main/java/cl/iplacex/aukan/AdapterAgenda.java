package cl.iplacex.aukan;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

// Adapter del Sistema de Agendamiento de Clases, que se integra por SOAP
public class AdapterAgenda {

    private static final HttpClient HTTP = HttpClient.newHttpClient();

    public static void main(String[] args) throws Exception {
        Suscriptor.escuchar("dig_agenda", AdapterAgenda::actualizar);
    }

    static void actualizar(String rut, boolean alDia) throws Exception {
        String operacion = alDia ? "HabilitarUsuario" : "DeshabilitarUsuario";

        String sobre = """
                <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                  <s:Body>
                    <%s xmlns="http://tempuri.org/">
                      <clienteId>%s</clienteId>
                    </%s>
                  </s:Body>
                </s:Envelope>""".formatted(operacion, rut, operacion);

        HttpRequest peticion = HttpRequest.newBuilder(URI.create(Config.URL_AGENDA))
                .header("Content-Type", "text/xml; charset=utf-8")
                .header("SOAPAction", "\"http://tempuri.org/IAgendaService/" + operacion + "\"")
                .POST(HttpRequest.BodyPublishers.ofString(sobre))
                .build();
        HttpResponse<String> respuesta = HTTP.send(peticion, HttpResponse.BodyHandlers.ofString());

        if (respuesta.statusCode() != 200) {
            throw new IllegalStateException("Agenda respondió " + respuesta.statusCode() + ": " + respuesta.body());
        }
        System.out.println(rut + " -> " + operacion + ": " + resultado(respuesta.body()));
    }

    // La respuesta SOAP trae el texto de confirmación dentro del elemento Result
    private static String resultado(String cuerpo) {
        Matcher m = Pattern.compile("<[^>]*Result>([^<]*)<").matcher(cuerpo);
        return m.find() ? m.group(1) : cuerpo;
    }
}
