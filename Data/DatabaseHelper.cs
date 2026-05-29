using System;
using System.IO;
using System.Data;
using System.Data.SQLite;
using MySqlConnector;
using Dapper;

namespace ControlMachine.Data
{
    public static class DatabaseHelper
    {
        public static string MySqlConnectionString = "Server=localhost;Database=controlmachine;Uid=root;Pwd=segredo@64;";
        public static string SqliteConnectionString = "Data Source=localdb.sqlite";

        public static IDbConnection GetLocalConnection()
        {
            return new SQLiteConnection(SqliteConnectionString);
        }

        public static IDbConnection GetRemoteConnection()
        {
            return new MySqlConnection(MySqlConnectionString);
        }

        public static bool IsServerAvailable()
        {
            try
            {
                using (var conn = GetRemoteConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void InitializeLocalDatabase()
        {
            using (var conn = GetLocalConnection())
            {
                conn.Open();
                string sql = @"
                    CREATE TABLE IF NOT EXISTS Usuarios (
                        Id INTEGER PRIMARY KEY,
                        Nome TEXT,
                        Login TEXT,
                        SenhaHash TEXT,
                        CodigoAcesso TEXT,
                        NivelMaster INTEGER,
                        Ativo INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS MaquinasLaser (
                        Id INTEGER PRIMARY KEY,
                        Nome TEXT,
                        Descricao TEXT,
                        Ativa INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS Producoes (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        RemoteId INTEGER NULL,
                        Pedido TEXT,
                        Cliente TEXT,
                        NumeroProducao TEXT,
                        Status TEXT,
                        Quantidade INTEGER,
                        DataProducao TEXT,
                        UsuarioId INTEGER,
                        MaquinaId INTEGER,
                        Sincronizado INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS Parametros (
                        Id INTEGER PRIMARY KEY,
                        Chave TEXT,
                        Valor TEXT,
                        Descricao TEXT
                    );

                    CREATE TABLE IF NOT EXISTS Brincos (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Numero TEXT UNIQUE,
                        DataGravacao TEXT,
                        MaquinaId INTEGER,
                        MotivoRegravacao TEXT,
                        Sincronizado INTEGER
                    );
 
                    CREATE TABLE IF NOT EXISTS Auditoria (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        DataHora TEXT,
                        UsuarioId INTEGER,
                        Acao TEXT,
                        Detalhes TEXT,
                        Sincronizado INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS FichasTecnicas (
                        Id INTEGER PRIMARY KEY,
                        Nome TEXT,
                        Potencia REAL,
                        Velocidade INTEGER,
                        Frequencia INTEGER,
                        Passadas INTEGER,
                        Ativa INTEGER
                    );
                ";
                conn.Execute(sql);

                
                try { conn.Execute("ALTER TABLE Usuarios ADD COLUMN CodigoAcesso TEXT;"); } catch { }
                try { conn.Execute("ALTER TABLE Brincos ADD COLUMN MotivoRegravacao TEXT;"); } catch { }
                try { conn.Execute("ALTER TABLE Producoes ADD COLUMN FichaTecnicaId INTEGER;"); } catch { }

                
                string sqlFichas = @"
                    INSERT OR IGNORE INTO FichasTecnicas (Id, Nome, Potencia, Velocidade, Frequencia, Passadas, Ativa)
                    VALUES 
                    (1, 'Gravação Interna Ouro 18k', 35.0, 800, 30, 2, 1),
                    (2, 'Gravação Profunda Prata 925', 50.0, 500, 20, 3, 1),
                    (3, 'Corte Acrílico 3mm (CO2)', 80.0, 100, 50, 1, 1);";
                conn.Execute(sqlFichas);

                
                string sqlUser = "INSERT OR IGNORE INTO Usuarios (Id, Nome, Login, SenhaHash, NivelMaster, Ativo) VALUES (1, 'Admin', 'admin', '123', 1, 1);";
                conn.Execute(sqlUser);
            }
        }

        public static void InitializeRemoteDatabase()
        {
            if (IsServerAvailable())
            {
                try
                {
                    using (var conn = GetRemoteConnection())
                    {
                        string sql = @"
                            CREATE TABLE IF NOT EXISTS FichasTecnicas (
                                Id INT AUTO_INCREMENT PRIMARY KEY,
                                Nome VARCHAR(100) NOT NULL,
                                Potencia DOUBLE NOT NULL,
                                Velocidade INT NOT NULL,
                                Frequencia INT NOT NULL,
                                Passadas INT NOT NULL,
                                Ativa BOOLEAN NOT NULL DEFAULT 1
                            );

                            CREATE TABLE IF NOT EXISTS Auditoria (
                                Id INT AUTO_INCREMENT PRIMARY KEY,
                                DataHora DATETIME NOT NULL,
                                UsuarioId INT NOT NULL,
                                Acao VARCHAR(100) NOT NULL,
                                Detalhes TEXT,
                                FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
                            );
                        ";
                        conn.Execute(sql);

                        
                        try { conn.Execute("ALTER TABLE Usuarios ADD COLUMN CodigoAcesso VARCHAR(50) NULL UNIQUE;"); } catch { }
                        try { conn.Execute("ALTER TABLE Brincos ADD COLUMN MotivoRegravacao VARCHAR(255) NULL;"); } catch { }
                        try { conn.Execute("ALTER TABLE Producoes ADD COLUMN FichaTecnicaId INT NULL;"); } catch { }
                        try { conn.Execute("ALTER TABLE Producoes ADD CONSTRAINT fk_producoes_fichatecnica FOREIGN KEY (FichaTecnicaId) REFERENCES FichasTecnicas(Id);"); } catch { }
                    }
                }
                catch { }
            }
        }
    }
}
