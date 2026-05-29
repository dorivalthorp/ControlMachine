using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Dapper;
using ControlMachine.Data;
using ControlMachine.Models;

namespace ControlMachine.Services
{
    public static class SyncService
    {
        private static Timer _timer;
        private static bool _isSyncing = false;

        public static void Start()
        {
            _timer = new Timer(30000); 
            _timer.Elapsed += async (sender, e) => await SincronizarAsync();
            _timer.Start();
        }

        public static void Stop()
        {
            _timer?.Stop();
        }

        private static async Task SincronizarAsync()
        {
            if (_isSyncing) return;
            _isSyncing = true;

            try
            {
                if (DatabaseHelper.IsServerAvailable())
                {
                    SincronizarDominio();
                    SincronizarProducoesPendentes();
                }
            }
            catch (Exception)
            {
                
            }
            finally
            {
                _isSyncing = false;
            }
        }

        public static void SincronizarProducoesPendentes()
        {
            using (var localDb = DatabaseHelper.GetLocalConnection())
            using (var remoteDb = DatabaseHelper.GetRemoteConnection())
            {
                var pendentes = localDb.Query<Producao>("SELECT * FROM Producoes WHERE Sincronizado = 0").ToList();

                foreach (var p in pendentes)
                {
                    if (p.RemoteId == null || p.RemoteId == 0)
                    {
                        
                        string sqlInsert = @"
                            INSERT INTO Producoes (Pedido, Cliente, NumeroProducao, Status, Quantidade, DataProducao, UsuarioId, MaquinaId, FichaTecnicaId)
                            VALUES (@Pedido, @Cliente, @NumeroProducao, @Status, @Quantidade, @DataProducao, @UsuarioId, @MaquinaId, @FichaTecnicaId);
                            SELECT LAST_INSERT_ID();";
                        
                        int remoteId = remoteDb.ExecuteScalar<int>(sqlInsert, p);
                        localDb.Execute("UPDATE Producoes SET RemoteId = @RemoteId, Sincronizado = 1 WHERE Id = @Id", new { RemoteId = remoteId, Id = p.Id });
                    }
                    else
                    {
                        
                        string sqlUpdate = @"
                            UPDATE Producoes SET Status = @Status, Quantidade = @Quantidade
                            WHERE Id = @RemoteId";
                        remoteDb.Execute(sqlUpdate, new { p.Status, p.Quantidade, p.RemoteId });
                        localDb.Execute("UPDATE Producoes SET Sincronizado = 1 WHERE Id = @Id", new { Id = p.Id });
                    }
                }

                
                var brincosPendentes = localDb.Query<Brinco>("SELECT * FROM Brincos WHERE Sincronizado = 0 OR Sincronizado IS NULL");
                foreach (var b in brincosPendentes)
                {
                    try {
                        remoteDb.Execute("INSERT IGNORE INTO Brincos (Numero, DataGravacao, MaquinaId, MotivoRegravacao) VALUES (@Numero, @DataGravacao, @MaquinaId, @MotivoRegravacao)", b);
                        localDb.Execute("UPDATE Brincos SET Sincronizado = 1 WHERE Id = @Id", new { Id = b.Id });
                    } catch { }
                }

                
                var auditorias = localDb.Query<Auditoria>("SELECT * FROM Auditoria WHERE Sincronizado = 0 OR Sincronizado IS NULL");
                foreach (var a in auditorias)
                {
                    try {
                        remoteDb.Execute("INSERT INTO Auditoria (DataHora, UsuarioId, Acao, Detalhes) VALUES (@DataHora, @UsuarioId, @Acao, @Detalhes)", a);
                        localDb.Execute("UPDATE Auditoria SET Sincronizado = 1 WHERE Id = @Id", new { Id = a.Id });
                    } catch { }
                }
            }
        }

        public static void SincronizarDominio()
        {
            using (var localDb = DatabaseHelper.GetLocalConnection())
            using (var remoteDb = DatabaseHelper.GetRemoteConnection())
            {
                
                var usuariosLocais = localDb.Query<Usuario>("SELECT * FROM Usuarios");
                foreach (var ul in usuariosLocais)
                {
                    try
                    {
                        remoteDb.Execute(@"
                            INSERT INTO Usuarios (Id, Nome, Login, SenhaHash, CodigoAcesso, NivelMaster, Ativo)
                            VALUES (@Id, @Nome, @Login, @SenhaHash, @CodigoAcesso, @NivelMaster, @Ativo)
                            ON DUPLICATE KEY UPDATE
                                Nome = VALUES(Nome),
                                Login = VALUES(Login),
                                SenhaHash = VALUES(SenhaHash),
                                CodigoAcesso = VALUES(CodigoAcesso),
                                NivelMaster = VALUES(NivelMaster),
                                Ativo = VALUES(Ativo)", ul);
                    }
                    catch { }
                }

                
                var maquinasLocais = localDb.Query<MaquinaLaser>("SELECT * FROM MaquinasLaser");
                foreach (var ml in maquinasLocais)
                {
                    try
                    {
                        remoteDb.Execute(@"
                            INSERT INTO MaquinasLaser (Id, Nome, Descricao, Ativa)
                            VALUES (@Id, @Nome, @Descricao, @Ativa)
                            ON DUPLICATE KEY UPDATE
                                Nome = VALUES(Nome),
                                Descricao = VALUES(Descricao),
                                Ativa = VALUES(Ativa)", ml);
                    }
                    catch { }
                }

                
                var fichasLocais = localDb.Query<FichaTecnica>("SELECT * FROM FichasTecnicas");
                foreach (var fl in fichasLocais)
                {
                    try
                    {
                        remoteDb.Execute(@"
                            INSERT INTO FichasTecnicas (Id, Nome, Potencia, Velocidade, Frequencia, Passadas, Ativa)
                            VALUES (@Id, @Nome, @Potencia, @Velocidade, @Frequencia, @Passadas, @Ativa)
                            ON DUPLICATE KEY UPDATE
                                Nome = VALUES(Nome),
                                Potencia = VALUES(Potencia),
                                Velocidade = VALUES(Velocidade),
                                Frequencia = VALUES(Frequencia),
                                Passadas = VALUES(Passadas),
                                Ativa = VALUES(Ativa)", fl);
                    }
                    catch { }
                }

                
                var parametros = remoteDb.Query<Parametro>("SELECT * FROM Parametros");
                foreach(var p in parametros)
                {
                    localDb.Execute("INSERT OR REPLACE INTO Parametros (Id, Chave, Valor, Descricao) VALUES (@Id, @Chave, @Valor, @Descricao)", p);
                }

                var maquinas = remoteDb.Query<MaquinaLaser>("SELECT * FROM MaquinasLaser");
                foreach(var m in maquinas)
                {
                    localDb.Execute("INSERT OR REPLACE INTO MaquinasLaser (Id, Nome, Descricao, Ativa) VALUES (@Id, @Nome, @Descricao, @Ativa)", m);
                }

                var fichas = remoteDb.Query<FichaTecnica>("SELECT * FROM FichasTecnicas");
                foreach(var f in fichas)
                {
                    localDb.Execute("INSERT OR REPLACE INTO FichasTecnicas (Id, Nome, Potencia, Velocidade, Frequencia, Passadas, Ativa) VALUES (@Id, @Nome, @Potencia, @Velocidade, @Frequencia, @Passadas, @Ativa)", f);
                }

                var usuarios = remoteDb.Query<Usuario>("SELECT * FROM Usuarios");
                foreach(var u in usuarios)
                {
                    localDb.Execute("INSERT OR REPLACE INTO Usuarios (Id, Nome, Login, SenhaHash, CodigoAcesso, NivelMaster, Ativo) VALUES (@Id, @Nome, @Login, @SenhaHash, @CodigoAcesso, @NivelMaster, @Ativo)", u);
                }
            }
        }
    }
}
