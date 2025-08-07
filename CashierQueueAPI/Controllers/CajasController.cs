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

                if (lista == null || !lista.Any())
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "No se encontraron cajas." });
                }

                return StatusCode(StatusCodes.Status200OK, new { code = "200", Response = lista });
            }
            catch (SqlException sqlEx)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al conectar con la base de datos.", detalle = sqlEx.Message });
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
    }
}
