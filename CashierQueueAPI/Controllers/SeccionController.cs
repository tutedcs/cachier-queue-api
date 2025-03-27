using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;
using CashierQueueAPI.Models;

namespace CashierQueueAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeccionesController : ControllerBase
    {
        private readonly string cadenaSQL;

        public SeccionesController(IConfiguration config)
        {
            cadenaSQL = config.GetConnectionString("CadenaSQL") ?? throw new ArgumentNullException(nameof(cadenaSQL), "Connection string cannot be null.");
        }

        [HttpGet]
        [Route("Listar")]
        public IActionResult Listar()
        {
            IEnumerable<Seccion> secciones = new List<Seccion>();
            string mensaje = "Error interno del servidor";
            bool success = false;

            try
            {
                using var conexion = new SqlConnection(cadenaSQL);
                conexion.Open();

                string query = "SELECT * FROM SECCION WHERE nSeccion > 0";
                secciones = conexion.Query<Seccion>(query);

                success = true;
                mensaje = "Secciones obtenidas correctamente";
            }
            catch (Exception ex)
            {
                mensaje = "Error al obtener secciones: " + ex.Message;
            }

            return success
                ? StatusCode(200, new { code = "200", mensaje, data = secciones })
                : StatusCode(500, new { code = "500", mensaje });
        }

        [HttpPost]
        [Route("Crear")]
        public IActionResult Crear([FromBody] Seccion nuevaSeccion)
        {
            string mensaje = "Error interno del servidor";
            bool success = false;

            try
            {
                using var conexion = new SqlConnection(cadenaSQL);
                conexion.Open();

                string query = "INSERT INTO SECCION (nSeccion) VALUES (@nSeccion)";
                int filasAfectadas = conexion.Execute(query, new { nuevaSeccion.nSeccion });

                if (filasAfectadas > 0)
                {
                    success = true;
                    mensaje = "Sección creada correctamente";
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error al crear sección: " + ex.Message;
            }

            return success
                ? StatusCode(200, new { code = "200", mensaje })
                : StatusCode(500, new { code = "500", mensaje });
        }
    }
}