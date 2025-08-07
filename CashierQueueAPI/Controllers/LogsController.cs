using CashierQueueAPI.Models;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CashierQueueAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly string cadenaSQL;

        public LogsController(IConfiguration config)
        {
            cadenaSQL = config.GetConnectionString("CadenaSQL") ?? throw new ArgumentNullException(nameof(cadenaSQL), "Connection string cannot be null.");
        }

        [HttpGet]
        [Route("GetUltimos")]
        public IActionResult ListarUltimos10()
        {
            try 
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    string query = @"
                SELECT TOP 10 
                    lg.idLog, 
                    lg.idUsuario,
                    sc.nombre AS seccion,
                    cj.nCaja AS caja,
                    us.nombre,
                    us.apellido,
                    lg.fechaLog
                FROM SESIONLOG as lg
                INNER JOIN USUARIO AS us ON us.idUsuario = lg.idUsuario
                LEFT JOIN CAJAS AS cj ON cj.idCaja = lg.idCaja
                LEFT JOIN SECCION AS sc ON sc.idSeccion = cj.seccion
                ORDER BY fechaLog DESC";
                    var logs = conexion.Query<SesionLogs>(query).ToList();
                    return Ok(logs);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error interno del servidor: " + ex.Message });
            }
        }

        [HttpGet]
        [Route("LogsPorUsuario")]
        public async Task<IActionResult> ListarLogsPorUsuario(
            [FromQuery] int idUsuario,
            [FromQuery] int top = 10,
            [FromQuery] int page = 1
        )
        {   
            try
            {
                using var conexion = new SqlConnection(cadenaSQL);

                const int MAX_TOP = 50;

                if (top <= 0) top = 10;
                if (top > MAX_TOP) top = MAX_TOP;

                if (page <= 0) page = 1;

                int offset = (page - 1) * top;

                string queryTotal = "SELECT COUNT(*) FROM SESIONLOG WHERE idUsuario = @idUsuario";
                int totalRegistros = await conexion.ExecuteScalarAsync<int>(queryTotal, new { idUsuario });

                int totalPaginas = (int)Math.Ceiling((double)totalRegistros / top);

                string query = @"
            SELECT lg.idLog, 
                lg.idUsuario,
                sc.nombre AS seccion,
                cj.nCaja AS caja,
                us.nombre,
                us.apellido,
                lg.fechaLog
            FROM SESIONLOG AS lg
            INNER JOIN USUARIO AS us ON us.idUsuario = lg.idUsuario
            LEFT JOIN CAJAS AS cj ON cj.idCaja = lg.idCaja
            LEFT JOIN SECCION AS sc ON sc.idSeccion = cj.seccion            
            WHERE lg.idUsuario = @idUsuario
            ORDER BY lg.fechaLog DESC
            OFFSET @offset ROWS FETCH NEXT @top ROWS ONLY;
                    ";

                var logs = await conexion.QueryAsync<SesionLogs>(query, new { idUsuario, offset, top });

                return StatusCode(200, new
                {
                    code = "200",
                    message = "Logs recuperados correctamente",
                    paginaActual = page,
                    totalPaginas,
                    registros = logs.Count(),
                    data = logs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al recuperar los logs",
                    detalle = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("LogsPorSeccion")]
        public async Task<IActionResult> ListarLogsPorSeccion(
            [FromQuery] int idSeccion,
            [FromQuery] int top = 10,
            [FromQuery] int page = 1
        )
        {
            try
            {
                using var conexion = new SqlConnection(cadenaSQL);

                const int MAX_TOP = 50;
                if (top <= 0) top = 10;
                if (top > MAX_TOP) top = MAX_TOP;
                if (page <= 0) page = 1;

                int offset = (page - 1) * top;

                string queryTotal = @"
            SELECT COUNT(*) 
            FROM SESIONLOG AS lg
            INNER JOIN CAJAS AS cj ON cj.idCaja = lg.idCaja
            WHERE cj.seccion = @idSeccion
        ";

                int totalRegistros = await conexion.ExecuteScalarAsync<int>(queryTotal, new { idSeccion });
                int totalPaginas = (int)Math.Ceiling((double)totalRegistros / top);

                string query = @"
            SELECT 
                lg.idLog, 
                lg.idUsuario,
                sc.nombre AS seccion,
                cj.nCaja AS caja,
                us.nombre,
                us.apellido,
                lg.fechaLog
            FROM SESIONLOG AS lg
            INNER JOIN USUARIO AS us ON us.idUsuario = lg.idUsuario
            LEFT JOIN CAJAS AS cj ON cj.idCaja = lg.idCaja
            LEFT JOIN SECCION AS sc ON sc.idSeccion = cj.seccion
            WHERE sc.idSeccion = @idSeccion
            ORDER BY lg.fechaLog DESC
            OFFSET @offset ROWS FETCH NEXT @top ROWS ONLY;
        ";

                var logs = await conexion.QueryAsync<SesionLogs>(query, new { idSeccion, offset, top });

                return Ok(new
                {
                    code = "200",
                    message = "Logs recuperados correctamente",
                    paginaActual = page,
                    totalPaginas,
                    registros = logs.Count(),
                    data = logs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al recuperar los logs",
                    detalle = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("ExportLogs")]
        public IActionResult GetTodosCsv()
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();

                    string query = @"
                SELECT 
                    lg.idLog, 
                    lg.idUsuario,
                    sc.nombre AS seccion,
                    cj.nCaja AS caja,
                    us.nombre,
                    us.apellido,
                    lg.fechaLog
                FROM SESIONLOG as lg
                INNER JOIN USUARIO AS us ON us.idUsuario = lg.idUsuario
                LEFT JOIN CAJAS AS cj ON cj.idCaja = lg.idCaja
                LEFT JOIN SECCION AS sc ON sc.idSeccion = cj.seccion
                ORDER BY lg.fechaLog DESC";

                    var logs = conexion.Query(query).ToList(); // Resultado dinámico

                    var csv = new StringBuilder();

                    // Encabezado CSV
                    csv.AppendLine("idLog,Fecha de la sesión,Sección,N° de caja,idUsuario,Nombre del empleado");

                    // Filas
                    foreach (var log in logs)
                    {
                        string idLog = log.idLog.ToString();
                        string fechaSesion = ((DateTime)log.fechaLog).ToString("yyyy-MM-dd HH:mm:ss");
                        string seccion = log.seccion ?? "";
                        string nCaja = log.caja?.ToString() ?? "";
                        string idUsuario = log.idUsuario.ToString();
                        string nombreEmpleado = $"{log.nombre} {log.apellido}";

                        csv.AppendLine($"{idLog},{fechaSesion},{seccion},{nCaja},{idUsuario},{nombreEmpleado}");
                    }

                    // Convertir a bytes y retornar archivo
                    byte[] bytes = Encoding.UTF8.GetBytes(csv.ToString());
                    return File(bytes, "text/csv", "Inicios_de_sesion.csv");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error interno del servidor: " + ex.Message });
            }
        }


    }
}
