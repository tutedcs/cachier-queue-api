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
                    string query = "SELECT idUsuario, nombre, apellido, rol, usuario FROM USUARIO WHERE activo = 1";
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
        public IActionResult BuscarUser([FromQuery] string nombre, [FromQuery] string apellido)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    string query = @"SELECT 
                                    idUsuario, rol, usuario, activo FROM 
                                    USUARIO WHERE 
                                    nombre = @nombre AND apellido = @apellido";
                    var parameters = new { nombre, apellido };
                    var usuarios = conexion.Query(query, parameters).ToList();
                    return Ok(usuarios);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut]
        [Route("Activar/{idUsuario}")]
        public IActionResult Activar(int idUsuario)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    string query = @"UPDATE USUARIO 
                                 SET activo = 1
                                 WHERE idUsuario = @idUsuario";
                    var parameters = new { idUsuario };
                    conexion.Execute(query, parameters);
                    return StatusCode(200, new { code = "200", mensaje = "Usuario editado correctamente" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut]
        [Route("Desactivar/{idUsuario}")]
        public IActionResult Desactivar(int idUsuario)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    string query = @"UPDATE USUARIO 
                                 SET activo = 0
                                 WHERE idUsuario = @idUsuario";
                    var parameters = new { idUsuario };
                    conexion.Execute(query, parameters);
                    return StatusCode(200, new { code="200", mensaje = "Usuario editado correctamente" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
 }