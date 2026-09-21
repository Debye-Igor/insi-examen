package cl.iplacex.aukan;

// Datos de conexión que comparten el Bridge y los adapters de la parte final
final class Config {
    static final String BROKER = "tcp://127.0.0.1:61616";
    static final String USUARIO = "admin";
    static final String CLAVE = "admin";
    static final String TOPICO = "dig_amq_estado_cuenta";

    private Config() {
    }
}
