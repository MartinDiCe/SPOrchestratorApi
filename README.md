# SPOrchestratorAPI

## Introducción
**SPOrchestratorAPI** es un microservicio diseñado para ejecutar stored procedures de manera dinámica en diversas bases de datos. La configuración completa de cada SP (nombre, cadena de conexión, proveedor y parámetros esperados) se almacena en la base de datos, lo que permite agregar o modificar stored procedures sin necesidad de cambiar el código.

El sistema sigue el los principios SOLID, utilizando programación reactiva para la ejecución y validación centralizada, y middlewares globales para el registro de logs y manejo de excepciones. Esto centraliza la lógica de negocio y la configuración, mientras que los controladores se mantienen delgados y se centran únicamente en la presentación.

## Proceso

El flujo de trabajo del microservicio es el siguiente:

1. **Recepción de la Solicitud**
   - Un cliente envía una petición HTTP al microservicio.
   - La solicitud contiene el nombre del servicio a ejecutar y, opcionalmente, un diccionario con los valores de los parámetros.  
     *Nota:* Para la ejecución final, el cliente solo envía el `serviceName` y `parameters`; el nombre del SP, la cadena de conexión, el proveedor y los parámetros esperados se obtienen de la configuración almacenada en la base de datos.

2. **Validación de la Solicitud**
   - Se verifica que el servicio solicitado exista en la base de datos de configuración.
   - Se valida que el diccionario de parámetros enviado coincida exactamente con los parámetros esperados definidos en la configuración (sin parámetros extra ni faltantes).
   - Si la validación falla, se devuelve un error (por ejemplo, 400) con un mensaje descriptivo.

3. **Obtención de Configuración del Servicio**
   - Se consulta en la base de datos la configuración asociada al servicio, la cual incluye:
      - El nombre del stored procedure (`NombreProcedimiento`).
      - La cadena de conexión.
      - El proveedor (por ejemplo, `SqlServer`).
      - Los parámetros esperados (almacenados en formato JSON).

4. **Ejecución del Stored Procedure**
   - Se establece conexión con la base de datos utilizando la cadena de conexión de la configuración.
   - Se ejecuta el stored procedure con los parámetros proporcionados.
   - Se captura el resultado mediante un `SqlDataReader` y se transforma a una estructura estándar (por ejemplo, una lista de diccionarios).

5. **Transformación y Respuesta**
   - Si el request especifica que la respuesta debe ser un archivo (flag `isFile` en el DTO), el controlador convierte el resultado (por ejemplo, el listado) a CSV mediante un helper y lo devuelve como archivo con el content type `text/csv`.
   - Si no, el resultado se devuelve en formato JSON.

6. **Manejo de Errores y Logging**
   - Los errores de validación (por ejemplo, parámetros faltantes o extra) se devuelven con códigos HTTP adecuados (400, 404 o 500) y mensajes descriptivos.
   - Un middleware global (como `ExceptionMiddleware` y `RequestResponseLoggingMiddleware`) captura excepciones y registra logs detallados de cada request/response.
   - Se registra cada ejecución en la base de datos para auditoría y trazabilidad.

## Especificaciones

### Funcionalidades

1. **Ejecución Dinámica de Stored Procedures**
   - Permite agregar nuevos stored procedures sin necesidad de modificar el código.
   - Soporta procedimientos con diferentes parámetros y en múltiples bases de datos.

2. **Configuración Centralizada**
   - La configuración se almacena en la base de datos (entidad `ServicioConfiguracion`), que define el SP, la cadena de conexión, el proveedor y los parámetros esperados.
   - Facilita la administración y migración de servicios sin afectar la lógica de ejecución.

3. **Validación de Parámetros**
   - Se valida que el request contenga exactamente los parámetros esperados (según lo definido en `ServicioConfiguracion.Parametros`).
   - Se rechazan solicitudes con parámetros extra o faltantes, devolviendo errores claros.

4. **Ejecución en Dos Modos: JSON y Archivo**
   - Dependiendo de un flag (`isFile`) en el request, el resultado se devuelve en formato JSON o se transforma a CSV para descarga.

5. **Registro y Auditoría**
   - Cada ejecución se registra en la base de datos (entidad `ServicioEjecucion`), con información de usuario, fecha, resultado y tiempo de ejecución.
   - Los errores se registran con un identificador único para trazabilidad y auditoría.

6. **Políticas de Reintento y Timeout**
   - Se configuran reintentos y tiempos máximos de ejecución para mejorar la resiliencia ante fallos transitorios.

### Casos de Uso

- **Ejecución de Stored Procedure Dinámico:**  
  El cliente envía el nombre del servicio y los parámetros, y el sistema ejecuta el SP configurado, devolviendo el resultado en JSON o como archivo CSV.

- **Validación de Parámetros:**  
  Si los parámetros enviados no coinciden con los definidos en la configuración, el sistema devuelve un error con la lista de parámetros esperados.

- **Prueba de Conexión y Auditoría:**  
  Se puede probar la conexión a la base de datos y se registra cada ejecución para auditoría.

### Estructura de la Base de Datos

1. **Servicio:**  
   Almacena los servicios (nombre, descripción, estado, etc.).

2. **ServicioConfiguracion:**  
   Define los stored procedures, la cadena de conexión, el proveedor y los parámetros esperados (en formato JSON).

3. **ServicioEjecucion:**  
   Registra cada ejecución del SP, con detalles de usuario, fecha, resultado y tiempo de ejecución.

4. **ServicioReintento:**  
   Define la política de reintentos en caso de fallo.

5. **ServicioReprocesamiento:**  
   Registra intentos de reprocesamiento manual.

6. **ServicioErrores:**  
   Registra errores con un identificador único.

7. **ServicioLogs:**  
   Registra logs detallados de la ejecución de stored procedures.

### Arquitectura del Proyecto

- **Entities**
- **DTOs y Validaciones**
- **Repositories**
- **Services**
- **Controllers**
- **Configuration**
- **Middleware**

## Lógica de Negocio

1. **Ejecución con Reintentos:**  
   Se reintenta la ejecución del SP según una política predefinida.

2. **Timeout Dinámico:**  
   Se configura un tiempo máximo para la ejecución de un SP, finalizando la ejecución si se excede.

3. **Reprocesamiento Manual:**  
   Los administradores pueden reejecutar manualmente un SP fallido.

4. **Auditoría y Logging:**  
   Cada ejecución se registra en la base de datos y se generan logs detallados mediante middlewares globales.

## Documentación Automática con Swagger

La aplicación genera documentación OpenAPI de forma dinámica basándose en los endpoints y esquemas definidos, facilitando la integración con clientes y el desarrollo frontend.

## Comunicación

1. **Cliente:**  
   Envía solicitudes al API Gateway.

2. **API Gateway:**  
   Redirige las peticiones al SPOrchestratorAPI.

3. **SPOrchestratorAPI:**
   - Ejecuta los stored procedures según la configuración.
   - Registra logs y auditoría de cada ejecución.
   - Valida y transforma los resultados.

4. **Administración:**  
   Permite la revisión y reprocesamiento de ejecuciones fallidas.

5. **Respuesta:**  
   Devuelve resultados en formato JSON o como archivos (CSV), según lo solicitado.

## Conclusión

**SPOrchestratorAPI** centraliza la ejecución de stored procedures mediante una configuración dinámica y flexible. Gracias a su arquitectura modular basada en el enfoque B y principios SOLID, se pueden agregar nuevos procedimientos sin modificar el código. La validación estricta de parámetros, la transformación de resultados y el manejo centralizado de errores y logs garantizan una solución robusta, escalable y fácil de mantener.

---

