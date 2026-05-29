using System;

namespace ControlMachine.Models
{
    public class Brinco
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public DateTime DataGravacao { get; set; }
        public int MaquinaId { get; set; }
        public string MotivoRegravacao { get; set; }
        public bool Sincronizado { get; set; }
    }
}
