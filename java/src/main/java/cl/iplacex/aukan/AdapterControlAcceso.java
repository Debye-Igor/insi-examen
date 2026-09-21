package cl.iplacex.aukan;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;

// Adapter del Sistema de Control de Acceso, que se integra por REST
public class AdapterControlAcceso {

    private static final HttpClient HTTP = HttpClient.newHttpClient();

    public static void main(String[] args) throws Exception {
        Suscriptor.escuchar("dig_control_acceso", AdapterControlAcceso::actualizar);
    }

    static void actualizar(String rut, boolean alDia) throws Exception {
        // El servicio recibe el estado como parámetro de consulta y no lleva cuerpo
        URI uri = URI.create(Config.URL_CONTROL_ACCESO + rut + "?habilitado=" + alDia);

        HttpRequest peticion = HttpRequest.newBuilder(uri)
                .method("PATCH", HttpRequest.BodyPublishers.noBody())
                .build();
        HttpResponse<String> respuesta = HTTP.send(peticion, HttpResponse.BodyHandlers.ofString());

        if (respuesta.statusCode() == 404) {
            System.out.println("El RUT " + rut + " no existe en Control de Acceso");
            return;
        }
        if (respuesta.statusCode() >= 300) {
            throw new IllegalStateException("Control de Acceso respondió " + respuesta.statusCode());
        }
        System.out.println(rut + (alDia ? " habilitado" : " deshabilitado"));
    }
}
