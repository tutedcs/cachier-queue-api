using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;
using CashierQueueAPI.Models;

namespace CashierQueueAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolController : ControllerBase
    {
        private readonly string cadenaSQL;

        public RolController(IConfiguration config)
        {
            cadenaSQL = config.GetConnectionString("CadenaSQL") ?? throw new ArgumentNullException(nameof(cadenaSQL), "Connection string cannot be null.");
        }

        [HttpGet]
        [Route("Listar")]
        public IActionResult Listar()
        {
            IEnumerable<Rol> roles = new List<Rol>();
            string mensaje = "Error interno del servidor";
            bool success = false;

            try
            {
                using var conexion = new SqlConnection(cadenaSQL);
                conexion.Open();

                string query = "SELECT * FROM ROL";
                roles = conexion.Query<Rol>(query);

                success = true;
                mensaje = "Roles obtenidos correctamente";
            }
            catch (Exception ex)
            {
                mensaje = "Error al obtener roles: " + ex.Message;
            }

            return success
                ? StatusCode(200, new { code = "200", mensaje, data = roles })
                : StatusCode(500, new { code = "500", mensaje });
        }

        [HttpPost]
        [Route("Crear")]
        public IActionResult Crear([FromBody] Rol nuevoRol)
        {
            string mensaje = "Error interno del servidor";
            bool success = false;

            try
            {
                using var conexion = new SqlConnection(cadenaSQL);
                conexion.Open();

                string query = "INSERT INTO ROL (nombre) VALUES (@nombre)";
                int filasAfectadas = conexion.Execute(query, new { nuevoRol.nombre });

                if (filasAfectadas > 0)
                {
                    success = true;
                    mensaje = "Rol creado correctamente";
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error al crear rol: " + ex.Message;
            }

            return success
                ? StatusCode(200, new { code = "200", mensaje })
                : StatusCode(500, new { code = "500", mensaje });
        }
    }
}
