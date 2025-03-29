using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data.SqlClient;
using System.Data;
using CashierQueueAPI.Models;

namespace CashierQueueAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CajasController : ControllerBase
    {
        private readonly string cadenaSQL;

        public CajasController(IConfiguration config)
        {
            cadenaSQL = config.GetConnectionString("CadenaSQL") ?? throw new ArgumentNullException(nameof(cadenaSQL), "Connection string cannot be null.");
        }

        [HttpGet]
        [Route("GetCajas")]
        public IActionResult Lista()
        {
            List<Cajas> lista = new List<Cajas>();

            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var query = "SELECT * FROM CAJAS WHERE idCaja > 0";
                    lista = conexion.Query<Cajas>(query).ToList();
                }
                return StatusCode(StatusCodes.Status200OK, new { code = "200", Response = lista });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message, Response = lista });
            }
        }

        [HttpGet]
        [Route("GetCajasNoLogeadas")]
        public IActionResult ListaNoLogeadas()
        {
            List<Cajas> lista = new List<Cajas>();

            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var query = "SELECT * FROM CAJAS WHERE isLogged = 0 AND nCaja > 0";
                    lista = conexion.Query<Cajas>(query).ToList();
                }
                return StatusCode(StatusCodes.Status200OK, new { code = "200", Response = lista });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message, Response = lista });
            }
        }

        [HttpGet]
        [Route("GetCajasXSeccion/{idSeccion}")]
        public IActionResult ListarXSeccion(int idSeccion)
        {
            List<Cajas> lista = new List<Cajas>();

            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var query = "SELECT * FROM CAJAS WHERE seccion = @idSeccion";
                    lista = conexion.Query<Cajas>(query, new { idSeccion }).ToList();
                }
                return StatusCode(StatusCodes.Status200OK, new { code = "200", Response = lista });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message, Response = lista });
            }
        }

        [HttpGet]
        [Route("GetDisponiblesXSeccion/{idSeccion}")]
        public IActionResult ListarDisponiblesXSeccion(int idSeccion)
        {
            List<Cajas> lista = new List<Cajas>();

            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var query = "SELECT * FROM CAJAS WHERE seccion = @idSeccion AND disponibilidad = 1";
                    lista = conexion.Query<Cajas>(query, new { idSeccion }).ToList();
                }
                return StatusCode(StatusCodes.Status200OK, new { code = "200", Response = lista });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message, Response = lista });
            }
        }

        [HttpGet]
        [Route("GetInfoCaja/{idCaja}")]
        public IActionResult GetInfoCaja(int idCaja)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var query = @"SELECT
	                    caja.idCaja,
                        caja.nCaja AS nCaja,
                        caja.disponibilidad AS disponibilidad,
	                    seccion.idSeccion,
                        seccion.nSeccion AS seccion
                    FROM CAJAS AS caja
                    LEFT JOIN SECCION AS seccion ON seccion.idSeccion = caja.seccion
                    WHERE idCaja = @idCaja";
                    var lista = conexion.Query<Cajas>(query, new { idCaja }).ToList();
                    return StatusCode(StatusCodes.Status200OK, new { code = "200", Response = lista });

                }
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }

        [HttpPut]
        [Route("SwitchDisponibilidad/{idCaja}")]
        public IActionResult SwitchDisponibilidad(int idCaja)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var querySelect = "SELECT disponibilidad FROM CAJAS WHERE idCaja = @idCaja";
                    var disponibilidad = conexion.QuerySingleOrDefault<bool>(querySelect, new { idCaja });

                    if (disponibilidad == null)
                    {
                        return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "Caja no encontrada" });
                    }

                    var nuevaDisponibilidad = !disponibilidad;
                    var queryUpdate = "UPDATE CAJAS SET disponibilidad = @nuevaDisponibilidad WHERE idCaja = @idCaja";
                    conexion.Execute(queryUpdate, new { nuevaDisponibilidad, idCaja });

                    return StatusCode(StatusCodes.Status200OK, new { code = "200", mensaje = "Disponibilidad actualizada correctamente" });
                }
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }

    }
}
