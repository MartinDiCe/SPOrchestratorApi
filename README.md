# SPOrchestratorAPI

## Introducción
Este documento describe el diseño, arquitectura y funcionalidades de un microservicio para la ejecución de stored procedures de manera dinámica. 

El objetivo principal de este microservicio es proporcionar una interfaz estandarizada para acceder a distintos procedimientos almacenados en bases de datos, permitiendo su configuración y administración desde una estructura centralizada.

El microservicio está diseñado para ser flexible y escalable, permitiendo la adición de nuevos servicios sin modificar el código principal. A través de una configuración almacenada en base de datos, se pueden definir los procedimientos almacenados, los parámetros requeridos, las conexiones a distintas bases de datos y políticas de reintento en caso de fallas. Además, se han incorporado funcionalidades para el registro de ejecuciones, manejo de errores y trazabilidad, asegurando un monitoreo adecuado del sistema.

Este microservicio permite su consumo a través de endpoints estandarizados, los cuales reciben peticiones HTTP con la configuración necesaria para ejecutar los procedimientos almacenados y devolver los resultados en formato JSON. Además, soporta la gestión de auditoría y errores mediante registros automáticos en la base de datos.

## Proceso
El flujo de trabajo del microservicio sigue una estructura bien definida desde la recepción de una solicitud hasta la respuesta final.

1. **Recepción de la Solicitud**
   - Un cliente envía una petición HTTP al endpoint del microservicio.
   - La petición debe contener el nombre del servicio a ejecutar y, en caso de ser necesario, los parámetros requeridos.

2. **Validación de la Solicitud**
   - Se verifica que el servicio solicitado exista en la base de datos de configuración.
   - Se valida que los parámetros proporcionados coincidan con la configuración esperada.
   - Si la validación falla, se devuelve un error con código `INVALID_PARAMETERS`.

3. **Obtención de Configuración del Servicio**
   - Se consulta en la base de datos la configuración del servicio, incluyendo el procedimiento almacenado asociado y la conexión a la base de datos donde debe ejecutarse.

4. **Ejecución del Stored Procedure**
   - Se establece conexión con la base de datos correspondiente.
   - Se ejecuta el procedimiento almacenado con los parámetros recibidos.
   - Se mide el tiempo de ejecución y se registra en logs.

5. **Manejo de Resultados y Respuesta**
   - Si la ejecución es exitosa, se devuelve un JSON con el resultado.
   - Si ocurre un error, se captura y se registra en la tabla de errores con un ID de referencia único.
   - Si el servicio tiene configurados reintentos, se vuelve a ejecutar el procedimiento según la política definida.

6. **Escenarios No Esperados y Manejo de Errores**
   - **Fallo en la conexión a la base de datos**: Se devuelve un error y se registra en logs.
   - **Parámetros inválidos**: Si los parámetros no coinciden con los esperados, se devuelve `INVALID_PARAMETERS`.
   - **Timeout**: Si la ejecución excede el tiempo máximo configurado, se finaliza y se registra como fallo.
   - **Carga excesiva**: Se puede implementar una cola de ejecución.
   - **Error en la Base de Datos**: Se devuelve `SP_EXECUTION_FAILED`.
   - **Reprocesamiento Manual**: Un administrador puede reejecutar manualmente una consulta fallida.

7. **Registro de Auditoría**
   - Se almacena cada ejecución en la tabla de auditoría, incluyendo usuario, fecha, resultado y tiempo de procesamiento.
   - Los errores también se registran con un identificador único para trazabilidad.

## Especificaciones
### Funcionalidades

1. **Ejecución dinámica de stored procedures**
   - No requiere cambios en el código para agregar nuevos procedimientos.
   - Soporta múltiples tipos de procedimientos con distintos parámetros.

2. **Configuración centralizada de servicios y conexiones**
   - Administración de conexiones a distintas bases de datos desde una única fuente.
   - Facilita la migración de servicios sin afectar la lógica de ejecución.

3. **Registro de ejecuciones y errores con ID de referencia único**
   - Proporciona trazabilidad completa de cada ejecución.
   - Permite auditoría y monitoreo del sistema.

4. **Gestión de Políticas de Reintento**
   - Mejora la resiliencia ante fallos transitorios de base de datos.

5. **Reprocesamiento manual**
   - Permite a los administradores tomar control sobre ejecuciones fallidas.

6. **Uso de Entity Framework para administración desde frontend**
   - Facilita la gestión de servicios y configuraciones desde una aplicación web.

### Casos de Uso
1. Ejecutar un stored procedure dinámico.
2. Reintentar ejecución en caso de fallo.
3. Reprocesar ejecución fallida de forma manual.
4. Consulta de logs y auditoría.

### Estructura de Base de Datos
1. **Servicio**: Almacena los servicios.
2. **ServicioConfiguracion**: Define los stored procedures y parámetros.
3. **ServicioEjecucion**: Registra cada ejecución.
4. **ServicioReintento**: Define la política de reintentos.
5. **ServicioReprocesamiento**: Registra intentos de reprocesamiento.
6. **ServicioErrores**: Registra errores con ID único.
7. **ServicioLogs**: Registra ejecución de SPs con tiempos y errores.

### Arquitectura del Proyecto
1. Entities
2. Controllers
3. Services
4. Repositories
5. Models
6. Configuration

## Lógica de Negocio

1. **Ejecución con Reintentos**
   - Se reintenta la ejecución según una política predefinida.

2. **Timeout Dinámico**
   - Se configura un tiempo máximo para la ejecución de un SP.

3. **Reprocesamiento Manual**
   - Permite reejecutar un procedimiento almacenado fallido.

4. **Verificación y creación automática de tablas al iniciar**
   - Se generan dinámicamente las tablas necesarias en la base de datos.

## Definición de Entidades (Entities)
1. **Clase Servicio**
2. **Clase ServicioConfiguracion**
3. **Clase ServicioEjecucion**
4. **Clase ServicioReintento**

## Documentación Automática con Swagger
Se implementará un mecanismo que leerá los servicios disponibles desde la base de datos y construirá los esquemas OpenAPI en tiempo de ejecución.

## Comunicación
1. **Cliente**: Envía solicitudes al API Gateway.
2. **API Gateway**: Redirige las peticiones al SPOrchestratorAPI.
3. **SPOrchestratorAPI**: Ejecuta los Stored Procedures en la Base de Datos.
4. **SPOrchestratorAPI**: Registra logs en el sistema de Logs de Ejecución.
5. **Administración**: Puede interactuar con el SPOrchestratorAPI para revisar ejecuciones y reprocesamientos.
6. **SPOrchestratorAPI**: Devuelve respuestas a la Administración para su control.

