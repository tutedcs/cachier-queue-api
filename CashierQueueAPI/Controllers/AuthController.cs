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

        [HttpPost("register")]
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
                "INSERT INTO USUARIO (nombre, apellido, usuario, passwordHash, rol, activo) VALUES (@Nombre, @Apellido, @Usuario, @PasswordHash, @Rol, 1)",
                new { Nombre = request.Nombre, Apellido = request.Apellido, Usuario = request.Usuario, PasswordHash = passwordHash, Rol = request.Rol });

            return rowsAffected > 0 ? Ok(new { code = "USER_REGISTERED", message = "Usuario registrado" }) : BadRequest(new { code = "REGISTRATION_ERROR", message = "Error al registrar usuario" });
        }


        public class LoginRequest
        {
            public string Usuario { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            using var connection = new SqlConnection(cadenaSQL);

            // Obtener hash de la base de datos
            var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT idUsuario, nombre, apellido, rol, passwordHash FROM USUARIO WHERE usuario = @Usuario AND activo = 1",
                new { Usuario = request.Usuario });

            if (user == null || !VerifyPassword(request.Password, user.passwordHash))
                return Unauthorized(new { code = "INVALID_CREDENTIALS", message = "Credenciales incorrectas" });

            return Ok(new
            {
                code = "LOGIN_SUCCESS",
                message = "Inicio de sesión exitoso",
                idUsuario = user.idUsuario,
                nombre = user.nombre,
                apellido = user.apellido,
                rol = user.rol
            });
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
