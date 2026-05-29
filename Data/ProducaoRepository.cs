using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using ControlMachine.Models;

namespace ControlMachine.Data
{
    public class ProducaoRepository
    {
        public void Inserir(Producao producao)
        {
            using (var conn = DatabaseHelper.GetLocalConnection())
            {
                producao.Sincronizado = false;
                string sql = @"
                    INSERT INTO Producoes (Pedido, Cliente, NumeroProducao, Status, Quantidade, DataProducao, UsuarioId, MaquinaId, FichaTecnicaId, Sincronizado)
                    VALUES (@Pedido, @Cliente, @NumeroProducao, @Status, @Quantidade, @DataProducao, @UsuarioId, @MaquinaId, @FichaTecnicaId, 0);
                    SELECT last_insert_rowid();";
                
                producao.Id = conn.ExecuteScalar<int>(sql, new {
                    producao.Pedido,
                    producao.Cliente,
                    producao.NumeroProducao,
                    producao.Status,
                    producao.Quantidade,
                    DataProducao = producao.DataProducao.ToString("yyyy-MM-dd HH:mm:ss"),
                    producao.UsuarioId,
                    producao.MaquinaId,
                    producao.FichaTecnicaId
                });
            }
            
            
            if (DatabaseHelper.IsServerAvailable())
            {
                Services.SyncService.SincronizarProducoesPendentes();
            }
        }

        public IEnumerable<Producao> ObterTodas()
        {
            using (var conn = DatabaseHelper.GetLocalConnection())
            {
                string sql = @"
                    SELECT p.*, u.Nome as NomeUsuario, f.Nome as NomeFichaTecnica 
                    FROM Producoes p 
                    LEFT JOIN Usuarios u ON p.UsuarioId = u.Id 
                    LEFT JOIN FichasTecnicas f ON p.FichaTecnicaId = f.Id
                    ORDER BY p.DataProducao DESC";
                return conn.Query<Producao>(sql);
            }
        }

        public void AtualizarStatus(int id, string novoStatus)
        {
            using (var conn = DatabaseHelper.GetLocalConnection())
            {
                conn.Execute("UPDATE Producoes SET Status = @Status, Sincronizado = 0 WHERE Id = @Id", new { Status = novoStatus, Id = id });
            }
            if (DatabaseHelper.IsServerAvailable())
            {
                Services.SyncService.SincronizarProducoesPendentes();
            }
        }
    }
}
