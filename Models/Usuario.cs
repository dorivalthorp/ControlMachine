namespace ControlMachine.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Login { get; set; }
        public string SenhaHash { get; set; }
        public string CodigoAcesso { get; set; }
        public bool NivelMaster { get; set; }
        public bool Ativo { get; set; }
    }
}
