# SPOrchestratorAPI - Smart Process Orchestrator API

## Introducción

**Smart Process Orchestrator API** es un microservicio diseñado para orquestar la ejecución de procesos de manera dinámica y escalable. Inicialmente creado para ejecutar stored procedures en diversas bases de datos, el sistema ha evolucionado para soportar también la lectura de vistas SQL y la invocación de endpoints externos. Además, incorpora un módulo de programación que permite:

- Configurar la ejecución mediante expresiones CRON.
- Decidir si se deben guardar los registros de la ejecución para auditoría.
- Encadenar procesos ("continue with"): extraer datos de un proceso inicial y, para cada registro, invocar otro proceso de continuación.

La arquitectura se basa en los principios SOLID y utiliza programación reactiva para la ejecución y validación centralizada. Los middlewares globales se encargan del logging, manejo de excepciones y trazabilidad, dejando a los controladores la responsabilidad de la presentación.

## Proceso General

1. **Recepción de la Solicitud**  
   El cliente envía una petición HTTP al microservicio, especificando el `serviceName` y, opcionalmente, un diccionario de parámetros. Además, se puede incluir un flag `isFile` para definir el formato de la respuesta.

2. **Validación y Configuración**
   - Se verifica que el servicio solicitado exista en la base de datos de configuración.
   - Se valida que el diccionario de parámetros enviado coincida con lo definido en la configuración (sin parámetros extra ni faltantes).
   - La configuración (almacenada en la entidad `ServicioConfiguracion`) incluye:
      - **NombreProcedimiento:** Nombre del proceso (StoredProcedure, VistaSql o EndPoint).
      - **ConexionBaseDatos:** Cadena de conexión para SP o vistas. Para EndPoints, se asigna el valor "No requiere" y se omite la validación.
      - **Provider:** Proveedor de la base de datos (por ejemplo, SqlServer) o EndPoint.
      - **Tipo:** Tipo de proceso (StoredProcedure, VistaSql o EndPoint).
      - **Parametros:** Parámetros esperados (almacenados en formato JSON).
      - **EsProgramado:** Flag que indica si la ejecución se realizará de forma programada.
      - **ContinueWith:** Flag que habilita el siguiente servicio que continuará después del proceso configurado. Requiere asociar un servicio existente y configurado.
      - **SalvarResponse:** Flag que habilita el guardado de los registros que se obtienen llamando a los servicios en una tabla del sistema.
      - Se pueden configurar también políticas de reintento y timeout.

3. **Ejecución del Proceso Inicial**  
   Según el tipo de configuración, se ejecuta:
   - **StoredProcedure:** Se conecta a la base de datos y se ejecuta el procedimiento con los parámetros proporcionados.
   - **VistaSql:** Se construye dinámicamente una consulta (incluyendo cláusulas WHERE basadas en los parámetros) y se ejecuta para obtener los registros.
   - **EndPoint:** Se invoca un servicio externo mediante HTTP (utilizando HttpClient).

4. **Orquestación y Continuación de Procesos**
   - **Servicio de Programación:** Permite agendar ejecuciones periódicas mediante expresiones CRON y definir si se guardan los registros de la ejecución.
   - **Flujo "Continue With":**
      - Tras obtener los resultados del proceso inicial, se extraen campos específicos (por ejemplo, ID y descripción) de cada registro.
      - Para cada registro, se invoca un proceso de continuación (por ejemplo, un endpoint) usando un mapeo configurable que asocia campos del registro con los parámetros esperados.
      - Esto permite encadenar procesos sin modificar la lógica de cada etapa.

5. **Transformación y Respuesta**
   - Dependiendo del flag `isFile`, el resultado se devuelve en formato JSON o se transforma a CSV para descarga.
   - Si la consulta inicial no retorna registros, se devuelve un mensaje informativo.

6. **Manejo de Errores y Trazabilidad**
   - Los errores de validación y ejecución se gestionan con códigos HTTP adecuados y mensajes descriptivos.
   - Middlewares globales (por ejemplo, `ExceptionMiddleware` y `RequestResponseLoggingMiddleware`) capturan excepciones y registran logs detallados de cada request/response.
   - Cada ejecución se registra en la base de datos (por ejemplo, en la entidad `ApiTrace`) para auditoría y trazabilidad.

## Implementación con Docker

Construir la imagen:
```bash
docker build -t smartprocessorchestratorapi:latest .
```
Ejecutar la imagen:
```bash
docker run -d -p 9000:80 --name smartprocessorchestratorapi -e ASPNETCORE_ENVIRONMENT=Development smartprocessorchestratorapi:latest
```

## Documentación Swagger

La documentación OpenAPI se genera dinámicamente, facilitando la integración con clientes y el desarrollo frontend.

## Conclusión

Smart Process Orchestrator API centraliza y orquesta la ejecución de procesos (Stored Procedures, Vistas SQL y EndPoints) mediante una configuración dinámica y flexible. Su arquitectura modular permite:

- Agregar nuevos procesos sin modificar el código.
- Encadenar procesos con flujos "continue with".
- Configurar ejecuciones programadas mediante expresiones CRON.
- Registrar cada ejecución para auditoría y trazabilidad.

## Documentación

- 📄 [Guía de configuración](./CONFIGURACION.md)

