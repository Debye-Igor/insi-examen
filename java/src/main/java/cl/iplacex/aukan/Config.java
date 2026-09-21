package cl.iplacex.aukan;

// Datos de conexión que comparten el Bridge y los adapters
final class Config {
    static final String BROKER = "tcp://127.0.0.1:61616";
    static final String USUARIO = "admin";
    static final String CLAVE = "admin";
    static final String TOPICO = "dig_amq_estado_cuenta";

    static final String URL_CONTROL_ACCESO = "http://localhost:5003/api/users/";
    static final String URL_AGENDA = "http://localhost:5002/AgendaService";

    private Config() {
    }
}
