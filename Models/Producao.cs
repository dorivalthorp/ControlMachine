using System;

namespace ControlMachine.Models
{
    public class Producao
    {
        public int Id { get; set; }
        public int? RemoteId { get; set; }
        public string Pedido { get; set; }
        public string Cliente { get; set; }
        public string NumeroProducao { get; set; }
        public string Status { get; set; } 
        public int Quantidade { get; set; }
        public DateTime DataProducao { get; set; }
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; }
        public int MaquinaId { get; set; }
        public int? FichaTecnicaId { get; set; }
        
        
        public string NomeFichaTecnica { get; set; }
        
        
        public bool Sincronizado { get; set; } 
    }
}
