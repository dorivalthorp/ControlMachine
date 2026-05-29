using System;
using Dapper;
using ControlMachine.Models;

namespace ControlMachine.Data
{
    public class BrincoRepository
    {
        public string GerarESalvarNovoBrinco(string prefixo, int maquinaId)
        {
            if (prefixo.Length != 3)
            {
                throw new Exception("O prefixo deve conter exatamente 3 caracteres.");
            }

            using (var conn = DatabaseHelper.GetLocalConnection())
            {
                
                string sqlBusca = "SELECT MAX(Numero) FROM Brincos WHERE Numero LIKE @Prefixo";
                string maxNumero = conn.ExecuteScalar<string>(sqlBusca, new { Prefixo = prefixo + "%" });

                string novoNumero;

                if (string.IsNullOrEmpty(maxNumero))
                {
                    novoNumero = prefixo + "000000000001";
                }
                else
                {
                    string sulfixo = maxNumero.Substring(3); 
                    if (long.TryParse(sulfixo, out long ultimoNumero))
                    {
                        long proximo = ultimoNumero + 1;
                        novoNumero = prefixo + proximo.ToString("D12"); 
                    }
                    else
                    {
                        novoNumero = prefixo + "000000000001";
                    }
                }

                
                string sqlInsert = @"
                    INSERT INTO Brincos (Numero, DataGravacao, MaquinaId, Sincronizado)
                    VALUES (@Numero, @DataGravacao, @MaquinaId, 0);";
                
                conn.Execute(sqlInsert, new { Numero = novoNumero, DataGravacao = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), MaquinaId = maquinaId });

                return novoNumero;
            }
        }
    }
}
