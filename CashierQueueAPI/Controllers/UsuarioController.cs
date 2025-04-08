using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using System.Security.Cryptography;
using CashierQueueAPI.Models;
using System.Text;

namespace CashierQueueAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly string cadenaSQL;

        public UsuarioController(IConfiguration config)
        {
            cadenaSQL = config.GetConnectionString("CadenaSQL");
        }

        [HttpGet]
        [Route("Listar")]
        public IActionResult Listar([FromQuery] int? idSeccion = null)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    string query = @"
                    SELECT
                        usuario.idUsuario,
                        usuario.nombre,
                        usuario.apellido,
                        usuario.usuario,
                        usuario.rol,
                        caja.nCaja AS caja,
                        seccion.nSeccion AS seccion
                    FROM USUARIO AS usuario
                    INNER JOIN CAJAS AS caja ON caja.idCaja = usuario.caja
                    LEFT JOIN SECCION AS seccion ON seccion.idSeccion = caja.seccion
                    WHERE (@idSeccion IS NULL OR seccion.idSeccion = @idSeccion)
                    ORDER BY CASE WHEN usuario.rol = 1 THEN 0 ELSE 1 END, usuario.rol";

                    var usuarios = conexion.Query(query, new { idSeccion }).ToList();
                    return Ok(usuarios);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("BuscarPorId/{idUsuario}")]
        public IActionResult BuscarPorId(int idUsuario)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    string query = @"
                    SELECT
                        usuario.idUsuario, 
                        usuario.nombre, 
                        usuario.apellido, 
                        usuario.rol,
                        usuario.caja AS idCaja,
                        caja.nCaja,
                        seccion.nSeccion
                    FROM USUARIO AS usuario
                    LEFT JOIN CAJAS AS caja ON caja.idCaja = usuario.caja
                    LEFT JOIN SECCION AS seccion ON seccion.idSeccion = caja.seccion
                    WHERE usuario.idUsuario = @idUsuario
                ";

                    var usuario = conexion.QueryFirstOrDefault(query, new { idUsuario = idUsuario });
                    return usuario != null ? Ok(usuario) : NotFound("Usuario no encontrado");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        [HttpGet]
        [Route("BuscarUsuario")]
        public IActionResult BuscarUser([FromQuery] string usuario)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    string query = @"SELECT
                    idUsuario, 
                    nombre, 
                    apellido, 
                    rol, 
                    caja.nCaja,
                    seccion.nSeccion
                    FROM USUARIO AS usuario
                    LEFT JOIN CAJAS as caja ON caja.idCaja = usuario.caja
                    LEFT JOIN SECCION AS seccion ON seccion.idSeccion = caja.seccion
                    WHERE usuario.usuario = @usuario";
                    var parameters = new { usuario };
                    var usuarios = conexion.Query(query, parameters).ToList();
                    return StatusCode(200, new { code = "200", coincidencia = usuarios });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        public class UsuarioDto
        {
            public int idUsuario { get; set; }
        }


        [HttpPost]
        [Route("Register")]
        public IActionResult Register([FromBody] Usuario usuario)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    string query = @"INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) 
                                     VALUES (@nombre, @apellido, @usuario, @rol, @caja)";
                    var parameters = new { usuario.nombre, usuario.apellido, usuario.usuario, usuario.rol, caja = usuario.caja };
                    conexion.Execute(query, parameters);
                    return StatusCode(200, new { code = "200", mensaje = "Usuario registrado correctamente" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        public class SetCajaRequest
        {
            public int idUsuario { get; set; }
            public int idCaja { get; set; }
        }

        [HttpPut]
        [Route("SetCaja")]
        public IActionResult SetCaja([FromBody] SetCajaRequest request)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    using (var transaccion = conexion.BeginTransaction())
                    {
                        try
                        {
                            // Obtener la caja actual del usuario
                            string obtenerCajaActualQuery = "SELECT caja FROM USUARIO WHERE idUsuario = @idUsuario";
                            int? cajaActual = conexion.ExecuteScalar<int?>(obtenerCajaActualQuery, new { request.idUsuario }, transaction: transaccion);

                            // Si el usuario ya tenía una caja asignada, desloggear esa caja
                            if (cajaActual.HasValue)
                            {
                                string desloggearCajaQuery = "UPDATE CAJAS SET isLogged = 0 WHERE idCaja = @idCaja";
                                conexion.Execute(desloggearCajaQuery, new { idCaja = cajaActual.Value }, transaction: transaccion);
                            }

                            // Asignar la nueva caja al usuario
                            string updateUsuarioQuery = @"UPDATE USUARIO 
                                                  SET caja = @idCaja
                                                  WHERE idUsuario = @idUsuario";
                            conexion.Execute(updateUsuarioQuery, request, transaction: transaccion);

                            // Marcar la nueva caja como logueada
                            string updateCajaQuery = @"UPDATE CAJAS 
                                               SET isLogged = 1 WHERE idCaja = @idCaja";
                            conexion.Execute(updateCajaQuery, new { request.idCaja }, transaction: transaccion);

                            // Confirmar la transacción
                            transaccion.Commit();

                            return StatusCode(200, new { code = "200", mensaje = "Caja actualizada correctamente" });
                        }
                        catch (Exception ex)
                        {
                            // Si algo falla, deshacer la transacción
                            transaccion.Rollback();
                            return StatusCode(500, new { code = "500", mensaje = "Error al actualizar la caja", error = ex.Message });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { code = "500", mensaje = "Error interno del servidor", error = ex.Message });
            }
        }

    }
 }