namespace CashierQueueAPI.Models
{
    public class Usuario
    {
        public int idUsuario { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string usuario { get; set; }
        public string passwordHash { get; set; }
        public int rol { get; set; }

    }
}
