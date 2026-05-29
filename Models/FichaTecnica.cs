namespace ControlMachine.Models
{
    public class FichaTecnica
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public double Potencia { get; set; } 
        public int Velocidade { get; set; } 
        public int Frequencia { get; set; } 
        public int Passadas { get; set; } 
        public bool Ativa { get; set; } 
    }
}
