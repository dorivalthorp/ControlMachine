using System;

namespace ControlMachine.Models
{
    public class Auditoria
    {
        public int Id { get; set; }
        public DateTime DataHora { get; set; }
        public int UsuarioId { get; set; }
        public string Acao { get; set; }
        public string Detalhes { get; set; }
        public bool Sincronizado { get; set; }
    }
}
