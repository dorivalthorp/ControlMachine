using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Dapper;
using ControlMachine.Data;
using ControlMachine.Models;
using ControlMachine.Helpers;

namespace ControlMachine.Helpers
{
    public static class CsvImporter
    {
        private static char DetectSeparator(string headerLine)
        {
            if (string.IsNullOrEmpty(headerLine)) return ';';
            
            int semicolons = headerLine.Split(';').Length;
            int commas = headerLine.Split(',').Length;
            
            return semicolons >= commas ? ';' : ',';
        }

        private static List<string> SplitCsvLine(string line, char separator)
        {
            List<string> parts = new List<string>();
            bool inQuotes = false;
            string current = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == separator && !inQuotes)
                {
                    parts.Add(current.Trim().Trim('"'));
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            parts.Add(current.Trim().Trim('"'));
            return parts;
        }

        public static string ImportBrincos(string filePath)
        {
            int success = 0;
            int failed = 0;
            List<string> errors = new List<string>();

            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length <= 1) return "O arquivo CSV está vazio ou contém apenas o cabeçalho.";

                string headerLine = lines[0];
                char sep = DetectSeparator(headerLine);
                var headers = SplitCsvLine(headerLine, sep).Select(h => h.ToLower().Trim()).ToList();

                int idxNumero = headers.IndexOf("numero");
                if (idxNumero == -1) idxNumero = headers.IndexOf("brinco");
                if (idxNumero == -1) idxNumero = headers.IndexOf("serial");

                if (idxNumero == -1)
                {
                    return "Coluna 'Numero' (ou 'brinco'/'serial') obrigatória não encontrada no cabeçalho.";
                }

                int idxData = headers.IndexOf("datagravacao");
                if (idxData == -1) idxData = headers.IndexOf("data");
                
                int idxMaquina = headers.IndexOf("maquinaid");
                if (idxMaquina == -1) idxMaquina = headers.IndexOf("maquina");

                int idxMotivo = headers.IndexOf("motivoregravacao");
                if (idxMotivo == -1) idxMotivo = headers.IndexOf("motivo");

                var processedNumeros = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        for (int i = 1; i < lines.Length; i++)
                        {
                            string line = lines[i];
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            try
                            {
                                var columns = SplitCsvLine(line, sep);
                                if (columns.Count <= idxNumero)
                                {
                                    failed++;
                                    errors.Add($"Linha {i + 1}: dados insuficientes.");
                                    continue;
                                }

                                string numero = columns[idxNumero];
                                if (string.IsNullOrWhiteSpace(numero))
                                {
                                    failed++;
                                    errors.Add($"Linha {i + 1}: número do brinco está em branco.");
                                    continue;
                                }

                                if (!processedNumeros.Add(numero))
                                {
                                    failed++;
                                    errors.Add($"Linha {i + 1}: brinco '{numero}' está duplicado no próprio arquivo CSV.");
                                    continue;
                                }

                                var exists = conn.QueryFirstOrDefault<int?>(
                                    "SELECT Id FROM Brincos WHERE Numero = @Numero", 
                                    new { Numero = numero }, 
                                    transaction);

                                if (exists != null)
                                {
                                    failed++;
                                    errors.Add($"Linha {i + 1}: brinco '{numero}' já existe.");
                                    continue;
                                }

                                string dataGravacao = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                if (idxData != -1 && idxData < columns.Count && !string.IsNullOrWhiteSpace(columns[idxData]))
                                {
                                    if (DateTime.TryParse(columns[idxData], out DateTime parsedDate))
                                    {
                                        dataGravacao = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                                    }
                                }

                                int maquinaId = 1;
                                if (idxMaquina != -1 && idxMaquina < columns.Count && !string.IsNullOrWhiteSpace(columns[idxMaquina]))
                                {
                                    int.TryParse(columns[idxMaquina], out maquinaId);
                                }

                                string motivo = null;
                                if (idxMotivo != -1 && idxMotivo < columns.Count)
                                {
                                    motivo = columns[idxMotivo];
                                }

                                conn.Execute(@"
                                    INSERT INTO Brincos (Numero, DataGravacao, MaquinaId, MotivoRegravacao, Sincronizado)
                                    VALUES (@Numero, @Data, @MaquinaId, @Motivo, 0)",
                                    new { Numero = numero, Data = dataGravacao, MaquinaId = maquinaId, Motivo = motivo },
                                    transaction);

                                success++;
                            }
                            catch (Exception ex)
                            {
                                failed++;
                                errors.Add($"Linha {i + 1}: {ex.Message}");
                            }
                        }
                        transaction.Commit();
                    }
                }

                string summary = $"Importação de Brincos Concluída!\nSucessos: {success}\nFalhas: {failed}";
                if (errors.Count > 0)
                {
                    summary += "\n\nErros:\n" + string.Join("\n", errors.Take(10));
                    if (errors.Count > 10) summary += $"\n... e mais {errors.Count - 10} erros.";
                }
                return summary;
            }
            catch (Exception ex)
            {
                return "Erro crítico de importação: " + ex.Message;
            }
        }

        public static string ImportPedidos(string filePath)
        {
            int success = 0;
            int failed = 0;
            List<string> errors = new List<string>();

            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length <= 1) return "O arquivo CSV está vazio ou contém apenas o cabeçalho.";

                string headerLine = lines[0];
                char sep = DetectSeparator(headerLine);
                var headers = SplitCsvLine(headerLine, sep).Select(h => h.ToLower().Trim()).ToList();

                int idxPedido = headers.IndexOf("pedido");
                int idxCliente = headers.IndexOf("cliente");
                int idxNumero = headers.IndexOf("numeroproducao");
                if (idxNumero == -1) idxNumero = headers.IndexOf("numero");

                if (idxPedido == -1 || idxCliente == -1 || idxNumero == -1)
                {
                    return "Colunas 'Pedido', 'Cliente' e 'NumeroProducao' (ou 'numero') são obrigatórias.";
                }

                int idxStatus = headers.IndexOf("status");
                int idxQuantidade = headers.IndexOf("quantidade");
                if (idxQuantidade == -1) idxQuantidade = headers.IndexOf("qtd");

                int idxData = headers.IndexOf("dataproducao");
                if (idxData == -1) idxData = headers.IndexOf("data");

                int idxMaquina = headers.IndexOf("maquinaid");
                if (idxMaquina == -1) idxMaquina = headers.IndexOf("maquina");

                int idxFicha = headers.IndexOf("fichatecnicaid");
                if (idxFicha == -1) idxFicha = headers.IndexOf("ficha");

                var processedNumeros = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        for (int i = 1; i < lines.Length; i++)
                        {
                            string line = lines[i];
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            try
                            {
                                var columns = SplitCsvLine(line, sep);
                                if (columns.Count <= Math.Max(idxPedido, Math.Max(idxCliente, idxNumero)))
                                {
                                    failed++;
                                    errors.Add($"Linha {i + 1}: dados insuficientes.");
                                    continue;
                                }

                                string pedido = columns[idxPedido];
                                string cliente = columns[idxCliente];
                                string numero = columns[idxNumero];

                                if (string.IsNullOrWhiteSpace(pedido) || string.IsNullOrWhiteSpace(cliente) || string.IsNullOrWhiteSpace(numero))
                                {
                                    failed++;
                                    errors.Add($"Linha {i + 1}: Pedido, Cliente ou Número em branco.");
                                    continue;
                                }

                                if (!processedNumeros.Add(numero))
                                {
                                    failed++;
                                    errors.Add($"Linha {i + 1}: pedido com número '{numero}' está duplicado no próprio arquivo CSV.");
                                    continue;
                                }

                                var exists = conn.QueryFirstOrDefault<int?>(
                                    "SELECT Id FROM Producoes WHERE NumeroProducao = @Numero", 
                                    new { Numero = numero }, 
                                    transaction);

                                if (exists != null)
                                {
                                    failed++;
                                    errors.Add($"Linha {i + 1}: pedido com número '{numero}' já existe no banco de dados.");
                                    continue;
                                }

                                string status = "Aguardando";
                                if (idxStatus != -1 && idxStatus < columns.Count && !string.IsNullOrWhiteSpace(columns[idxStatus]))
                                {
                                    status = columns[idxStatus];
                                }

                                int quantidade = 1;
                                if (idxQuantidade != -1 && idxQuantidade < columns.Count && !string.IsNullOrWhiteSpace(columns[idxQuantidade]))
                                {
                                    int.TryParse(columns[idxQuantidade], out quantidade);
                                }

                                string dataProducao = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                if (idxData != -1 && idxData < columns.Count && !string.IsNullOrWhiteSpace(columns[idxData]))
                                {
                                    if (DateTime.TryParse(columns[idxData], out DateTime parsedDate))
                                    {
                                        dataProducao = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                                    }
                                }

                                int maquinaId = 1;
                                if (idxMaquina != -1 && idxMaquina < columns.Count && !string.IsNullOrWhiteSpace(columns[idxMaquina]))
                                {
                                    int.TryParse(columns[idxMaquina], out maquinaId);
                                }

                                int? fichaId = null;
                                if (idxFicha != -1 && idxFicha < columns.Count && !string.IsNullOrWhiteSpace(columns[idxFicha]))
                                {
                                    if (int.TryParse(columns[idxFicha], out int fId))
                                    {
                                        fichaId = fId;
                                    }
                                }

                                int usuarioId = Program.UsuarioLogado?.Id ?? 1;

                                conn.Execute(@"
                                    INSERT INTO Producoes (Pedido, Cliente, NumeroProducao, Status, Quantidade, DataProducao, UsuarioId, MaquinaId, FichaTecnicaId, Sincronizado)
                                    VALUES (@Pedido, @Cliente, @NumeroProducao, @Status, @Quantidade, @DataProducao, @UsuarioId, @MaquinaId, @FichaTecnicaId, 0)",
                                    new { Pedido = pedido, Cliente = cliente, NumeroProducao = numero, Status = status, Quantidade = quantidade, DataProducao = dataProducao, UsuarioId = usuarioId, MaquinaId = maquinaId, FichaTecnicaId = fichaId },
                                    transaction);

                                success++;
                            }
                            catch (Exception ex)
                            {
                                failed++;
                                errors.Add($"Linha {i + 1}: {ex.Message}");
                            }
                        }
                        transaction.Commit();
                    }
                }

                string summary = $"Importação de Pedidos Concluída!\nSucessos: {success}\nFalhas: {failed}";
                if (errors.Count > 0)
                {
                    summary += "\n\nErros:\n" + string.Join("\n", errors.Take(10));
                    if (errors.Count > 10) summary += $"\n... e mais {errors.Count - 10} erros.";
                }
                return summary;
            }
            catch (Exception ex)
            {
                return "Erro crítico de importação: " + ex.Message;
            }
        }

        public static string ImportUsuarios(string filePath, string defaultPassword)
        {
            int success = 0;
            int failed = 0;
            List<string> errors = new List<string>();

            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length <= 1) return "O arquivo CSV está vazio ou contém apenas o cabeçalho.";

                string headerLine = lines[0];
                char sep = DetectSeparator(headerLine);
                var headers = SplitCsvLine(headerLine, sep).Select(h => h.ToLower().Trim()).ToList();

                int idxNome = headers.IndexOf("nome");
                int idxLogin = headers.IndexOf("login");

                if (idxNome == -1 || idxLogin == -1)
                {
                    return "Colunas 'Nome' e 'Login' são obrigatórias no CSV.";
                }

                int idxCodigo = headers.IndexOf("codigoacesso");
                if (idxCodigo == -1) idxCodigo = headers.IndexOf("pin");
                if (idxCodigo == -1) idxCodigo = headers.IndexOf("cracha");

                int idxMaster = headers.IndexOf("nivelmaster");
                if (idxMaster == -1) idxMaster = headers.IndexOf("master");

                int idxAtivo = headers.IndexOf("ativo");

                string hashPadrao = SecurityHelper.GetSha256Hash(defaultPassword);

                var processedLogins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var processedPins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        
                        int nextId = conn.ExecuteScalar<int?>("SELECT MAX(Id) FROM Usuarios", null, transaction) ?? 0;
                        nextId++;

                        for (int i = 1; i < lines.Length; i++)
                        {
                            string line = lines[i];
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            try
                            {
                                var columns = SplitCsvLine(line, sep);
                                if (columns.Count <= Math.Max(idxNome, idxLogin))
                                {
                                    failed++;
                                    errors.Add($"Linha {i + 1}: dados insuficientes.");
                                    continue;
                                }

                                string nome = columns[idxNome];
                                string login = columns[idxLogin];

                                if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(login))
                                {
                                    failed++;
                                    errors.Add($"Linha {i + 1}: Nome ou Login em branco.");
                                    continue;
                                }

                                if (!processedLogins.Add(login))
                                {
                                    failed++;
                                    errors.Add($"Linha {i + 1}: login '{login}' está duplicado no próprio arquivo CSV.");
                                    continue;
                                }

                                var userExists = conn.QueryFirstOrDefault<int?>(
                                    "SELECT Id FROM Usuarios WHERE Login = @Login",
                                    new { Login = login },
                                    transaction);

                                if (userExists != null)
                                {
                                    failed++;
                                    errors.Add($"Linha {i + 1}: usuário com login '{login}' já existe.");
                                    continue;
                                }

                                string codigoAcesso = null;
                                if (idxCodigo != -1 && idxCodigo < columns.Count && !string.IsNullOrWhiteSpace(columns[idxCodigo]))
                                {
                                    codigoAcesso = columns[idxCodigo];
                                    if (!processedPins.Add(codigoAcesso))
                                    {
                                        failed++;
                                        errors.Add($"Linha {i + 1}: PIN/Crachá '{codigoAcesso}' está duplicado no próprio arquivo CSV.");
                                        continue;
                                    }
                                    
                                    var pinExists = conn.QueryFirstOrDefault<int?>(
                                        "SELECT Id FROM Usuarios WHERE CodigoAcesso = @Pin",
                                        new { Pin = codigoAcesso },
                                        transaction);

                                    if (pinExists != null)
                                    {
                                        failed++;
                                        errors.Add($"Linha {i + 1}: PIN/Crachá '{codigoAcesso}' já está em uso.");
                                        continue;
                                    }
                                }

                                int nivelMaster = 0;
                                if (idxMaster != -1 && idxMaster < columns.Count && !string.IsNullOrWhiteSpace(columns[idxMaster]))
                                {
                                    string val = columns[idxMaster].ToLower();
                                    if (val == "1" || val == "true" || val == "sim" || val == "master")
                                    {
                                        nivelMaster = 1;
                                    }
                                }

                                int ativo = 1;
                                if (idxAtivo != -1 && idxAtivo < columns.Count && !string.IsNullOrWhiteSpace(columns[idxAtivo]))
                                {
                                    string val = columns[idxAtivo].ToLower();
                                    if (val == "0" || val == "false" || val == "não" || val == "inativo")
                                    {
                                        ativo = 0;
                                    }
                                }

                                conn.Execute(@"
                                    INSERT INTO Usuarios (Id, Nome, Login, SenhaHash, CodigoAcesso, NivelMaster, Ativo)
                                    VALUES (@Id, @Nome, @Login, @Senha, @CodigoAcesso, @Master, @Ativo)",
                                    new { Id = nextId, Nome = nome, Login = login, Senha = hashPadrao, CodigoAcesso = codigoAcesso, Master = nivelMaster, Ativo = ativo },
                                    transaction);

                                nextId++;
                                success++;
                            }
                            catch (Exception ex)
                            {
                                failed++;
                                errors.Add($"Linha {i + 1}: {ex.Message}");
                            }
                        }
                        transaction.Commit();
                    }
                }

                string summary = $"Importação de Usuários Concluída!\nSucessos: {success}\nFalhas: {failed}\nSenha Padrão gravada: '{defaultPassword}'";
                if (errors.Count > 0)
                {
                    summary += "\n\nErros:\n" + string.Join("\n", errors.Take(10));
                    if (errors.Count > 10) summary += $"\n... e mais {errors.Count - 10} erros.";
                }
                return summary;
            }
            catch (Exception ex)
            {
                return "Erro crítico de importação: " + ex.Message;
            }
        }
    }
}
