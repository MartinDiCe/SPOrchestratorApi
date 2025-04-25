# SPOrchestratorAPI – Smart Process Orchestrator API

## 1 · Portada & datos básicos

| Elemento                           | Valor                                                                                                                                                                      |
|------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Versión mínima**                 | .NET 8 · SQL Server 2019 · Docker                                                                                                                                          |
| **Imagen oficial**                 | `mdiceprojects/sporchestratorapi:latest`                                                                                                                                   |
| **Lanzar en un minuto Producción** | ```bash docker run -d -p 9000:80 --name sporchestratorapi -e ASPNETCORE_ENVIRONMENT=Production -e ASPNETCORE_URLS=http://+:80 mdiceprojects/sporchestratorapi:latest```    |
| **Lanzar en un minuto Desarrollo** | ```bash docker run -d -p 9001:80 --name sporchestratorapiQA -e ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_URLS=http://+:80 mdiceprojects/sporchestratorapi:latest``` |
| **Swagger UI**                     | `https://<DOMINIO>/swagger/index.html`                                                                                                                                     |
| **Hangfire dashboard**             | `https://<DOMINIO>/hangfire`                                                                                                                                               |
| **Repositorio**                    | <https://github.com/MartinDiCe/SPOrchestratorApi>                                                                                                                          |

---

## 2 · Introducción

**Smart Process Orchestrator API (SPOrchestratorAPI)** centraliza la ejecución de *Stored Procedures*, vistas SQL y llamados HTTP externos sin que tengas que crear un backend distinto para cada caso. Con una sola API puedes:

* Desplegar SP/vistas como servicios REST.
* Encadenar pasos (*continue-with*) y pasar parámetros vía JSON/JSONPath.
* Programar ejecuciones con CRON y auditar cada resultado en la base.
* Mezclar orígenes — un SP puede disparar un endpoint, un endpoint una vista, etc.

> **Pipeline general**  
> `Request → Validación → Ejecución → (Opc) Continue-With → Auditoría`

---

## 3 · Conceptos clave

| Concepto              | Entidad / tabla         | ¿Para qué sirve?                                           |
|-----------------------|-------------------------|------------------------------------------------------------|
| Parámetros de sistema | `Parameter`             | Flags globales (logging, trazas, etc.)                     |
| Servicio              | `Servicio`              | Nombre lógico, p. ej. `ObtenerChoferesActivos`             |
| Configuración         | `ServicioConfiguracion` | ConnString, tipo (SP/Vista/EndPoint), flags, proveedor DB… |
| Programación          | `ServicioProgramacion`  | CRON + `StartDate` / `EndDate`                             |
| Ejecución             | `ServicioEjecucion`     | Log detallado de cada run                                  |
| Traza HTTP            | `ApiTraces`             | Registro de cada request entrante                          |

---

## 4 · Alta de un flujo paso a paso

### 4.1 Crear Servicio

```http
POST /api/Servicio/create
{
  "name": "Demo",
  "description": "Servicio demostración",
  "status": true
}
```

### 4.2 Crear Configuración
```http
POST /api/ServicioConfiguracion/create
{
  "servicioId": 1006,
  "nombreProcedimiento": "Sp_EjecutarDemo",   // o URL completa si es EndPoint
  "conexionBaseDatos": "Server=…;Database=…;UserId=…;Password=…;TrustServerCertificate=True;",
  "parametros": "p1;p2",       // ';' separa parámetros – null = sin parámetros
  "maxReintentos": 0,
  "timeoutSegundos": 0,
  "provider": "SqlServer",     // SqlServer | MySql | PostgreSql | Oracle | EndPoint
  "tipo": "StoredProcedure",   // StoredProcedure | VistaSql | EndPoint
  "esProgramado": true,
  "guardarRegistros": true,
  "continuarCon": true,
  "jsonConfig": "RequiereApiKey=true;ApiKey=a677YYH99;TipoRequest=GET" //Si no es EndPoint va null o sin clave - Si viene vacio y es endpoint, por defecto TipoRequest=GET, Si no configurar POST, PUT, DELETED, ETC, y el resto de claves en null.
}
```

### 4.3 Programar (opcional)
Se necesita que **EsProgramado = true**.
```http
POST /api/ServicioProgramacion/create
{
  "servicioConfiguracionId": 1006,
  "cronExpression": "*/2 * * * *",   // cada 2 min
  "startDate": "2025-04-23T12:40:18Z",
  "endDate":   "2025-04-24T12:40:18Z"
}
```
_Referencia CRON:_ <https://crontab.guru>

### 4.4 Configurar Continue‑With (opcional)
```http
POST /api/ServicioContinueWith/create
{
  "servicioConfiguracionId": 1006,   // origen
  "servicioContinuacionId": 1007,    // destino
  "camposRelacion": "IdProducto=IdProductoParam;$.extra=Flag"
}
```

### 4.5 Probar el flujo
```http
POST /api/SpOrchestrator/execute
{
  "serviceName": "Demo",
  "parameters": { //sin parametros, no completar enviar null
    "p1": "Texto",
    "p2": 9599965 //si no es requerido enviar null
  },
  "isFile": false        // true = genera CSV descargable
}
```
*Ver resultados en `ServicioEjecucion`.*

---

## 5 · Monitorizar y auditar

| Necesitas…                          | Dónde mirar                        |
|-------------------------------------|------------------------------------|
| Últimas llamadas HTTP               | **ApiTraces**                      |
| Resultado / errores de procesos     | **ServicioEjecucion**              |
| Próximas / últimas ejecuciones CRON | Hangfire Dashboard                 |
| Log en tiempo real                  | `docker logs -f sporchestratorapi` |

---

## 6 · Mantenimiento

### Refrescar todos los jobs

```bash
curl -X POST https://<DOMINIO>/api/HangfireAdmin/refresh-jobs
```
1. `HangfireJobsInitializer.CleanUnscheduledJobsAsync` limpia huérfanos.
2. `RecurringJobRegistrar.RegisterAllJobs` recrea los válidos.

### Borrar una configuración
Marcar `Deleted = 1` en `ServicioConfiguracion`. 
Marcar `EsProgramado = False` en `ServicioConfiguracion`.
Borrar `ServicioProgramacion`.
En el próximo *refresh* desaparecerá o no se ejecutará su job.

### Cambiar CRON
Solo edita `ServicioProgramacion.CronExpression` → no reinicia.

---

## 7 · Docker / despliegue

```bash
docker build -t sporchestratorapi:latest .
docker run -d --name sporchestratorapi -p 9000:80   -e ASPNETCORE_ENVIRONMENT=Production   sporchestratorapi:latest
```

Variables de entorno más usadas:

| Variable                               | Propósito            |
|----------------------------------------|----------------------|
| `ConnectionStrings__DefaultConnection` | ConnString principal |
| `Serilog__MinimumLevel`                | Nivel mínimo de log  |
| `ASPNETCORE_URLS`                      | PUERTO/URL escucha   |

---

## 8 · Apéndices

* Tabla completa de endpoints (autogenerada por Swagger).
* Ejemplos avanzados de mapeo *continue‑with*.
* Errores comunes y cómo solucionarlos.
* Roadmap: próximos conectores (Kafka, gRPC…).

## 8 · Consideraciones

* Validar tabla parameters para activar o desactivar parametros de configuración.
* Recordar uso de configuración de EndPoints es muy distinto de StoreProcedures o VistaSQL que dependen de una base de datos, mientras que el otro del JSONConfig.
* Para reiniciar la Aplicación: POST /api/Admin/restart
---

