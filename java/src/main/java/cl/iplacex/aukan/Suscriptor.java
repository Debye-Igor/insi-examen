package cl.iplacex.aukan;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import jakarta.jms.Connection;
import jakarta.jms.Message;
import jakarta.jms.MessageConsumer;
import jakarta.jms.Session;
import jakarta.jms.Topic;
import org.apache.activemq.artemis.jms.client.ActiveMQConnectionFactory;

// Suscripción durable compartida al tópico de estados de cuenta.
// La usan los dos adapters, cada uno con su propio nombre de suscripción.
final class Suscriptor {

    interface Accion {
        void ejecutar(String rut, boolean alDia) throws Exception;
    }

    static void escuchar(String suscripcion, Accion accion) throws Exception {
        ActiveMQConnectionFactory fabrica = new ActiveMQConnectionFactory(Config.BROKER);
        try (Connection conexion = fabrica.createConnection(Config.USUARIO, Config.CLAVE)) {
            // Sesión transaccional: si falla la llamada al sistema, el mensaje se vuelve a entregar
            Session sesion = conexion.createSession(true, Session.SESSION_TRANSACTED);
            Topic topico = sesion.createTopic(Config.TOPICO);
            MessageConsumer consumidor = sesion.createSharedDurableConsumer(topico, suscripcion);
            conexion.start();
            System.out.println("Escuchando " + Config.TOPICO + " como " + suscripcion);

            while (true) {
                Message mensaje = consumidor.receive();
                String texto = mensaje.getBody(String.class);
                try {
                    JsonObject estado = JsonParser.parseString(texto).getAsJsonObject();
                    String rut = estado.get("clienteId").getAsString();
                    // Saldo menor o igual a cero significa que el cliente está al día
                    boolean alDia = estado.get("saldo").getAsBigDecimal().signum() <= 0;

                    accion.ejecutar(rut, alDia);
                    sesion.commit();
                } catch (Exception e) {
                    System.err.println("No se pudo procesar " + texto + ": " + e.getMessage());
                    sesion.rollback();
                    Thread.sleep(5000);
                }
            }
        }
    }

    private Suscriptor() {
    }
}
