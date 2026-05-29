CREATE DATABASE IF NOT EXISTS controlmachine;
USE controlmachine;

CREATE TABLE IF NOT EXISTS Usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    Login VARCHAR(50) NOT NULL UNIQUE,
    SenhaHash VARCHAR(255) NOT NULL,
    CodigoAcesso VARCHAR(50) NULL UNIQUE,
    NivelMaster BOOLEAN NOT NULL DEFAULT 0,
    Ativo BOOLEAN NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Parametros (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Chave VARCHAR(50) NOT NULL UNIQUE,
    Valor VARCHAR(255) NOT NULL,
    Descricao TEXT
);

CREATE TABLE IF NOT EXISTS MaquinasLaser (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    Descricao TEXT,
    Ativa BOOLEAN NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS FichasTecnicas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    Potencia DOUBLE NOT NULL,
    Velocidade INT NOT NULL,
    Frequencia INT NOT NULL,
    Passadas INT NOT NULL,
    Ativa BOOLEAN NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Brincos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Numero VARCHAR(15) NOT NULL UNIQUE,
    DataGravacao DATETIME NOT NULL,
    MaquinaId INT NOT NULL,
    MotivoRegravacao VARCHAR(255) NULL,
    FOREIGN KEY (MaquinaId) REFERENCES MaquinasLaser(Id)
);

CREATE TABLE IF NOT EXISTS Producoes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Pedido VARCHAR(50) NOT NULL,
    Cliente VARCHAR(100) NOT NULL,
    NumeroProducao VARCHAR(50) NOT NULL,
    Status VARCHAR(30) NOT NULL,
    Quantidade INT NOT NULL,
    DataProducao DATETIME NOT NULL,
    UsuarioId INT NOT NULL,
    MaquinaId INT NOT NULL,
    FichaTecnicaId INT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    FOREIGN KEY (MaquinaId) REFERENCES MaquinasLaser(Id),
    FOREIGN KEY (FichaTecnicaId) REFERENCES FichasTecnicas(Id)
);

CREATE TABLE IF NOT EXISTS Auditoria (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DataHora DATETIME NOT NULL,
    UsuarioId INT NOT NULL,
    Acao VARCHAR(100) NOT NULL,
    Detalhes TEXT,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);

INSERT IGNORE INTO Usuarios (Id, Nome, Login, SenhaHash, NivelMaster, Ativo) VALUES (1, 'Administrador', 'admin', '123', 1, 1);
INSERT IGNORE INTO MaquinasLaser (Id, Nome, Descricao, Ativa) VALUES (1, 'Máquina Laser 01', 'Fibra Óptica Principal', 1);
INSERT IGNORE INTO MaquinasLaser (Id, Nome, Descricao, Ativa) VALUES (2, 'Máquina Laser 02', 'Laser CO2', 1);

INSERT IGNORE INTO FichasTecnicas (Id, Nome, Potencia, Velocidade, Frequencia, Passadas, Ativa) VALUES (1, 'Gravação Interna Ouro 18k', 35.0, 800, 30, 2, 1);
INSERT IGNORE INTO FichasTecnicas (Id, Nome, Potencia, Velocidade, Frequencia, Passadas, Ativa) VALUES (2, 'Gravação Profunda Prata 925', 50.0, 500, 20, 3, 1);
INSERT IGNORE INTO FichasTecnicas (Id, Nome, Potencia, Velocidade, Frequencia, Passadas, Ativa) VALUES (3, 'Corte Acrílico 3mm (CO2)', 80.0, 100, 50, 1, 1);
