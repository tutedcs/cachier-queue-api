namespace CashierQueueAPI.Models
{
    public class SesionLogs
    {
        public int idLog { get; set; }
        public int idUsuario { get; set; }
        public string seccion { get; set; }
        public string caja { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string fechaLog { get; set; }
    }
}
