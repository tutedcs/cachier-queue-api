using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace CashierQueueAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string cadenaSQL;

        public AuthController(IConfiguration config)
        {
            cadenaSQL = config.GetConnectionString("CadenaSQL") ?? throw new ArgumentNullException(nameof(cadenaSQL), "Connection string cannot be null.");
        }

        public class RegisterRequest
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string Usuario { get; set; }
            public string Password { get; set; }
            public int Rol { get; set; }
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            string passwordHash = HashPassword(request.Password);

            using var connection = new SqlConnection(cadenaSQL);

            // Verificar si el usuario ya existe
            var existingUser = await connection.QueryFirstOrDefaultAsync<string>(
                "SELECT usuario FROM USUARIO WHERE usuario = @Usuario",
                new { Usuario = request.Usuario });

            if (existingUser != null)
                return BadRequest(new { code = "USER_EXISTS", message = "El usuario ya existe" });

            // Insertar el nuevo usuario
            int rowsAffected = await connection.ExecuteAsync(
                "INSERT INTO USUARIO (nombre, apellido, usuario, passwordHash, rol, caja) VALUES (@Nombre, @Apellido, @Usuario, @PasswordHash, @Rol, 2)",
                new { Nombre = request.Nombre, Apellido = request.Apellido, Usuario = request.Usuario, PasswordHash = passwordHash, Rol = request.Rol });

            return rowsAffected > 0 ? Ok(new { code = "200", message = "Usuario registrado" }) : BadRequest(new { code = "REGISTRATION_ERROR", message = "Error al registrar usuario" });
          
        }

        ///////////////////////////
        // METODOS PARA EL LOGIN //
        ///////////////////////////

        public class LoginRequest
        {
            public string Usuario { get; set; }
            public string? Password { get; set; }
            public int? idCaja { get; set; }
        }

        [HttpPost]
        [Route("Check-User")]
        public async Task<IActionResult> CheckUser([FromBody] LoginRequest request)
        {
            using var connection = new SqlConnection(cadenaSQL);

            var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                @"SELECT idUsuario, nombre, apellido, rol, passwordHash 
        FROM USUARIO WHERE usuario = @Usuario",
                new { Usuario = request.Usuario });

            if (user == null)
                return Unauthorized(new { code = "401", message = "Usuario no encontrado" });

            // Si el rol = 1, necesita contraseña
            if (user.rol == 1)
            {
                return Ok(new
                {
                    code = "NEED_PASSWORD",
                    usuario = new
                    {
                        nombre = user.nombre,
                        apellido = user.apellido,
                        rol = user.rol
                    }
                });
            }

            // Si es rol = 2, puede continuar seleccionando una caja
            return Ok(new
            {
                code = "NEED_CAJANUM",
                usuario = new
                {
                    idUsuario = user.idUsuario,
                    nombre = user.nombre,
                    apellido = user.apellido,
                    rol = user.rol
                }
            });
        }

        public class CajaAssignRequest
        {
            public int idUsuario { get; set; }
            public int idCaja { get; set; }
        }

        [HttpPost]
        [Route("Assign-Caja")]
        public async Task<IActionResult> AssignCaja([FromBody] CajaAssignRequest request)
        {
            using var connection = new SqlConnection(cadenaSQL);

            // Verificar si la caja está libre
            var cajaDisponible = await connection.QueryFirstOrDefaultAsync<bool>(
                "SELECT CASE WHEN isLogged = 0 THEN 1 ELSE 0 END FROM CAJAS WHERE idCaja = @idCaja",
                new { idCaja = request.idCaja });

            if (!cajaDisponible)
            {
                return Conflict(new { code = "CAJA_OCUPADA", message = "La caja ya está en uso. Seleccione otra." });
            }

            // Asignar caja al usuario y marcar caja como logueada
            var query = @"
        UPDATE USUARIO SET caja = @idCaja WHERE idUsuario = @idUsuario;
        UPDATE CAJAS SET isLogged = 1 WHERE idCaja = @idCaja;
    ";

            await connection.ExecuteAsync(query, new
            {
                idCaja = request.idCaja,
                idUsuario = request.idUsuario
            });

            return Ok(new
            {
                code = "200",
                message = "Caja asignada correctamente",
                data = new { idUsuario = request.idUsuario, idCaja = request.idCaja }
            });
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            using var connection = new SqlConnection(cadenaSQL);

            var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                @"SELECT idUsuario, nombre, apellido, rol, passwordHash 
          FROM USUARIO WHERE usuario = @Usuario",
                new { Usuario = request.Usuario });

            if (user == null || user.rol != 1 || !VerifyPassword(request.Password, user.passwordHash))
                return Unauthorized(new { code = "INVALID_CREDENTIALS", message = "Credenciales incorrectas" });

            return Ok(new
            {
                code = "LOGIN_SUCCESS",
                usuario = new
                {
                    idUsuario = user.idUsuario,
                    nombre = user.nombre,
                    apellido = user.apellido,
                    rol = user.rol
                }
            });
        }

        [HttpPost]
        [Route("LogOut")]
        public IActionResult LogOut([FromBody] int idUsuario)
        {
            try
            {
                using var conexion = new SqlConnection(cadenaSQL);
                conexion.Open();

                // 1. Obtener la caja asignada al usuario
                var idCaja = conexion.QueryFirstOrDefault<int?>(
                    "SELECT caja FROM USUARIO WHERE idUsuario = @idUsuario AND caja IS NOT NULL",
                    new { idUsuario });

                if (idCaja == null || idCaja == 2)
                {
                    return StatusCode(404, new { code = "404", mensaje = "Usuario no encontrado o ya estaba deslogueado" });
                }

                // 2. Marcar la caja como disponible y desasignarla del usuario
                string query = @"
                    UPDATE CAJAS SET isLogged = 0 WHERE idCaja = @idCaja;
                    UPDATE USUARIO SET caja = 2 WHERE idUsuario = @idUsuario;
                ";

                int filasAfectadas = conexion.Execute(query, new { idUsuario, idCaja });

                return Ok(new { code = "200", mensaje = "Usuario deslogueado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { code = "500", mensaje = "Error interno: " + ex.Message });
            }
        }



        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}