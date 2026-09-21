package cl.iplacex.aukan;

import com.google.gson.JsonParser;
import jakarta.jms.Connection;
import jakarta.jms.DeliveryMode;
import jakarta.jms.MessageProducer;
import jakarta.jms.Session;
import java.io.PrintWriter;
import java.io.StringWriter;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.StandardOpenOption;
import java.time.LocalDateTime;
import org.apache.activemq.artemis.jms.client.ActiveMQConnectionFactory;

// Bridge entre MSMQ y ActiveMQ Artemis.
// Lo ejecuta el Trigger de MSMQ, que le entrega el cuerpo del mensaje como argumento.
public class Bridge {

    // El Trigger corre sin consola, así que el programa deja su propio registro
    private static final Path LOG = Path.of("C:\\AukanGym\\bridge\\bridge.log");

    public static void main(String[] args) {
        // Se unen los argumentos por si Windows partió el texto en varios
        String mensaje = String.join(" ", args).trim();
        registrar("recibido: " + mensaje);

        if (mensaje.isEmpty()) {
            registrar("no llegó ningún mensaje desde el Trigger");
            System.exit(1);
        }

        try {
            publicar(normalizar(mensaje));
        } catch (Exception e) {
            var detalle = new StringWriter();
            e.printStackTrace(new PrintWriter(detalle));
            registrar("error al publicar: " + detalle);
            System.exit(1);
        }
    }

    private static void publicar(String mensaje) throws Exception {
        ActiveMQConnectionFactory fabrica = new ActiveMQConnectionFactory(Config.BROKER);
        try (Connection conexion = fabrica.createConnection(Config.USUARIO, Config.CLAVE)) {
            Session sesion = conexion.createSession(false, Session.AUTO_ACKNOWLEDGE);
            MessageProducer productor = sesion.createProducer(sesion.createTopic(Config.TOPICO));
            // PERSISTENT deja el mensaje en disco del broker
            productor.setDeliveryMode(DeliveryMode.PERSISTENT);
            productor.send(sesion.createTextMessage(mensaje));
        }
        registrar("publicado en " + Config.TOPICO + ": " + mensaje);
        System.out.println("Publicado en " + Config.TOPICO + ": " + mensaje);
    }

    // Windows quita las comillas al pasar el mensaje como argumento.
    // Gson lee el texto en modo tolerante y lo vuelve a escribir como JSON válido.
    private static String normalizar(String texto) {
        try {
            return JsonParser.parseString(texto).getAsJsonObject().toString();
        } catch (Exception e) {
            registrar("el mensaje no venía en JSON, se publica tal cual");
            return texto;
        }
    }

    private static void registrar(String linea) {
        try {
            Files.writeString(LOG, LocalDateTime.now() + " " + linea + System.lineSeparator(),
                    StandardCharsets.UTF_8, StandardOpenOption.CREATE, StandardOpenOption.APPEND);
        } catch (Exception e) {
            System.err.println("No se pudo escribir el log: " + e.getMessage());
        }
    }
}
