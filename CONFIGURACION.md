# SPOrchestratorAPI – Smart Process Orchestrator API

## 1 · Portada & datos básicos
| | |
|---|---|
| **Versión mínima** | .NET 8 • SQL Server 2019 • Docker |
| **Imagen oficial** | `mdiceprojects/sporchestratorapi:latest` |
| **Lanzar en un minuto** | ```bash
docker run -d -p 9000:80 --name sporchestratorapi \
-e ASPNETCORE_ENVIRONMENT=Production \
-e ASPNETCORE_URLS=http://+:80 \
mdiceprojects/sporchestratorapi:latest
``` |
| **Swagger UI** | `https://<DOMINIO>/swagger/index.html` |
| **Hangfire dashboard** | `https://<DOMINIO>/hangfire` |
| **Repositorio** | <https://github.com/MartinDiCe/SPOrchestratorApi> |

---

## 2 · Introducción
**Smart Process Orchestrator API (SPOrchestratorAPI)** centraliza la ejecución de *Stored Procedures*, vistas SQL y llamados HTTP externos sin que tengas que crear un backend distinto para cada caso.  
Con una sola API puedes :

* Desplegar rápidamente consultas o procesos existentes (SP/vistas) como servicios REST.
* Encadenar pasos (*continue‑with*) y parametrizar cada salto con JSON‐mapping o JSONPath.
* Programar ejecuciones vía CRON y auditar cada resultado en la base.
* Mezclar orígenes – un SP puede disparar un endpoint que a su vez invoque otro SP, todo sin tocar código.

En producción el pipeline luce así :

`Request ➜ Validación & DI ➜ Ejecución del servicio ➜ (Opc) Continue‑With ➜ Auditoría en ServicioEjecucion / ApiTraces`

---

## 3 · Conceptos clave

| Concepto | Entidad / tabla | ¿Para qué sirve? |
|----------|-----------------|------------------|
| **Parámetros de sistema** | `Parameter` | Flags globales (logging, trazas, seguridad, …) |
| **Servicio** | `Servicio` | Nombre lógico, p. ej. `ObtenerChoferesActivos` |
| **Configuración** | `ServicioConfiguracion` | ConnString, tipo (SP / Vista / EndPoint), flags `EsProgramado`, `ContinuarCon`, `GuardarRegistros`, proveedor DB… |
| **Programación** | `ServicioProgramacion` | CRON + `StartDate` / `EndDate` cuando `EsProgramado = true` |
| **Ejecución** | `ServicioEjecucion` | Log detallado (entrada, salida, duración, error) |
| **Traza HTTP** | `ApiTraces` | Registro de cada request entrante |

---

## 4 · Alta de un flujo paso a paso

### 4.1 Crear Servicio
```http
POST /api/Servicio/create
{
  "name": "Demo",
  "description": "Servicio demostración",
  "status": true
}
```

### 4.2 Crear Configuración
```http
POST /api/ServicioConfiguracion/create
{
  "servicioId": 1006,
  "nombreProcedimiento": "Sp_EjecutarDemo",   // o URL completa si es EndPoint
  "conexionBaseDatos": "Server=…;Database=…;User Id=…;Password=…;TrustServerCertificate=True;",
  "parametros": "p1;p2",       // ';' separa parámetros – null = sin parámetros
  "maxReintentos": 0,
  "timeoutSegundos": 0,
  "provider": "SqlServer",     // SqlServer | MySql | PostgreSql | Oracle | EndPoint
  "tipo": "StoredProcedure",   // StoredProcedure | VistaSql | EndPoint
  "esProgramado": true,
  "guardarRegistros": true,
  "continuarCon": true,
  "jsonConfig": "RequiereApiKey=true;ApiKey=a677YYH99;TipoRequest=GET"
}
```

### 4.3 Programar (opcional)
Se necesita que **EsProgramado = true**.
```http
POST /api/ServicioProgramacion/create
{
  "servicioConfiguracionId": 1006,
  "cronExpression": "*/2 * * * *",   // cada 2 min
  "startDate": "2025-04-23T12:40:18Z",
  "endDate":   "2025-04-24T12:40:18Z"
}
```
_Referencia CRON:_ <https://crontab.guru>

### 4.4 Configurar Continue‑With (opcional)
```http
POST /api/ServicioContinueWith/create
{
  "servicioConfiguracionId": 1006,   // origen
  "servicioContinuacionId": 1007,    // destino
  "camposRelacion": "IdProducto=IdProductoParam;$.extra=Flag"
}
```

### 4.5 Probar el flujo
```http
POST /api/SpOrchestrator/execute
{
  "serviceName": "Demo",
  "parameters": {
    "p1": "Texto",
    "p2": 9599965
  },
  "isFile": false        // true = genera CSV descargable
}
```
*Ver resultados en `ServicioEjecucion`.*

---

## 5 · Monitorizar y auditar

| Necesitas… | Dónde mirar |
|------------|-------------|
| Últimas llamadas HTTP | **ApiTraces** |
| Resultado / errores de procesos | **ServicioEjecucion** |
| Próximas / últimas ejecuciones CRON | Hangfire Dashboard |
| Log en tiempo real | `docker logs -f sporchestratorapi` |

---

## 6 · Mantenimiento

### Refrescar todos los jobs
```bash
curl -X POST http://<DOMINIO>/api/HangfireAdmin/refresh-jobs
```
1. `HangfireJobsInitializer.CleanUnscheduledJobsAsync` limpia huérfanos.
2. `RecurringJobRegistrar.RegisterAllJobs` recrea los válidos.

### Borrar una configuración
Marcar `Deleted = 1` en `ServicioConfiguracion`.  
En el próximo *refresh* desaparecerá su job.

### Cambiar CRON
Solo edita `ServicioProgramacion.CronExpression` → no reinicia.

---

## 7 · Docker / despliegue

```bash
docker build -t sporchestratorapi:latest .
docker run -d --name sporchestratorapi -p 9000:80   -e ASPNETCORE_ENVIRONMENT=Production   sporchestratorapi:latest
```

Variables de entorno más usadas:

| Variable | Propósito |
|----------|-----------|
| `ConnectionStrings__DefaultConnection` | ConnString principal |
| `Serilog__MinimumLevel` | Nivel mínimo de log |
| `ASPNETCORE_URLS` | PUERTO/URL escucha |

---

## 8 · Apéndices

* Tabla completa de endpoints (auto‑generada por Swagger).
* Ejemplos avanzados de mapeo *continue‑with*.
* Errores comunes y cómo solucionarlos.
* Roadmap: próximos conectores (Kafka, gRPC…).

---

> _Actualiza este documento cada vez que agregues nuevas flags o tablas: mantener la doc viva ahorra muchas preguntas en producción._
