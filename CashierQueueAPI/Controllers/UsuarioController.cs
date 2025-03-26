using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using CashierQueueAPI.Models;

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
        public IActionResult Listar()
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    string query = "SELECT idUsuario, nombre, apellido, rol, usuario, caja FROM USUARIO";
                    var usuarios = conexion.Query(query).ToList();
                    return Ok(usuarios);
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
                                    idUsuario, rol, nombre, apellido FROM 
                                    USUARIO WHERE 
                                    usuario = @usuario";
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

        public class loginRequest
        {
            public string usuario { get; set; }
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult LogIn([FromBody] loginRequest usuario)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    string query = @"SELECT idUsuario, nombre, apellido, rol, caja FROM USUARIO WHERE usuario = @usuario";
                    var parameters = new { usuario.usuario };
                    var usuarioLogueado = conexion.QueryFirstOrDefault(query, parameters);
                    if (usuarioLogueado != null)
                    {
                        return StatusCode(200, new { code = "200", mensaje = "Usuario logueado correctamente", usuario = usuarioLogueado });
                    }
                    else
                    {
                        return StatusCode(401, new { code = "401", mensaje = "Usuario Inexistente" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
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


        //[HttpPut]
        //[Route("Activar/{idUsuario}")]
        //public IActionResult Activar(int idUsuario)
        //{
        //    try
        //    {
        //        using (var conexion = new SqlConnection(cadenaSQL))
        //        {
        //            conexion.Open();
        //            string query = @"UPDATE USUARIO 
        //                         SET activo = 1
        //                         WHERE idUsuario = @idUsuario";
        //            var parameters = new { idUsuario };
        //            conexion.Execute(query, parameters);
        //            return StatusCode(200, new { code = "200", mensaje = "Usuario editado correctamente" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Internal server error: " + ex.Message);
        //    }
        //}


        //[HttpPut]
        //[Route("Desactivar/{idUsuario}")]
        //public IActionResult Desactivar(int idUsuario)
        //{
        //    try
        //    {
        //        using (var conexion = new SqlConnection(cadenaSQL))
        //        {
        //            conexion.Open();
        //            string query = @"UPDATE USUARIO 
        //                         SET activo = 0
        //                         WHERE idUsuario = @idUsuario";
        //            var parameters = new { idUsuario };
        //            conexion.Execute(query, parameters);
        //            return StatusCode(200, new { code="200", mensaje = "Usuario editado correctamente" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Internal server error: " + ex.Message);
        //    }
        //}
    }
 }