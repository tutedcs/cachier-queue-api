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
                    var query = "SELECT * FROM CAJAS WHERE isLogged = 0 AND idCaja > 0";
                    lista = conexion.Query<Cajas>(query).ToList();
                }
                return StatusCode(StatusCodes.Status200OK, new { code = "200", Response = lista });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message, Response = lista });
            }
        }

    }
}
