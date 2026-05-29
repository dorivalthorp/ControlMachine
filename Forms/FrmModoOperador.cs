using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using ControlMachine.Models;
using ControlMachine.Data;
using ControlMachine.Services;

namespace ControlMachine.Forms
{
    public class FrmModoOperador : Form
    {
        private Producao _producao;
        private FichaTecnica _ficha;
        private Label lblPedido;
        private Label lblCliente;
        private Label lblStatus;
        private Label lblQuantidade;
        private Label lblFichaNome;
        private Label lblFichaParams;
        private Button btnIniciar;
        private Button btnFinalizar;
        private Button btnRegravar;
        private Button btnVoltar;

        public FrmModoOperador(int producaoId)
        {
            CarregarProducao(producaoId);
            InitializeComponent();
            AtualizarTela();
        }

        private void CarregarProducao(int id)
        {
            using (var conn = DatabaseHelper.GetLocalConnection())
            {
                _producao = conn.QueryFirstOrDefault<Producao>(@"
                    SELECT p.*, u.Nome as NomeUsuario, f.Nome as NomeFichaTecnica 
                    FROM Producoes p 
                    LEFT JOIN Usuarios u ON p.UsuarioId = u.Id 
                    LEFT JOIN FichasTecnicas f ON p.FichaTecnicaId = f.Id 
                    WHERE p.Id = @Id", new { Id = id });

                if (_producao != null && _producao.FichaTecnicaId.HasValue)
                {
                    _ficha = conn.QueryFirstOrDefault<FichaTecnica>("SELECT * FROM FichasTecnicas WHERE Id = @Id", new { Id = _producao.FichaTecnicaId.Value });
                }
                else
                {
                    _ficha = null;
                }
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Painel do Operador - ControlMachine";
            this.Size = new Size(850, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(24, 24, 27); 

            if (Program.AppIcon != null) this.Icon = Program.AppIcon;

            
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.FromArgb(39, 39, 42) };
            var lblTitle = new Label
            {
                Text = "MODO OPERADOR",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(14, 165, 233), 
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);
            this.Controls.Add(pnlHeader);

            
            TableLayoutPanel pnlMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(20, 90, 20, 100) 
            };
            pnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.Controls.Add(pnlMain);
            pnlMain.BringToFront();

            
            GroupBox gbPedido = new GroupBox
            {
                Text = " DADOS DA ORDEM DE PRODUÇÃO ",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(161, 161, 170),
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                FlatStyle = FlatStyle.Flat
            };
            
            Panel pnlPedido = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            gbPedido.Controls.Add(pnlPedido);

            lblPedido = new Label { Text = "Ordem/Pedido: -", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Top = 15, Width = 350, Height = 30 };
            lblCliente = new Label { Text = "Cliente: -", Font = new Font("Segoe UI", 12, FontStyle.Regular), ForeColor = Color.FromArgb(212, 212, 216), Top = 55, Width = 350, Height = 25 };
            lblQuantidade = new Label { Text = "Quantidade: -", Font = new Font("Segoe UI", 12, FontStyle.Regular), ForeColor = Color.FromArgb(212, 212, 216), Top = 85, Width = 350, Height = 25 };
            
            var lblStatusTitle = new Label { Text = "Status:", Font = new Font("Segoe UI", 12, FontStyle.Regular), ForeColor = Color.FromArgb(212, 212, 216), Top = 125, Width = 70, Height = 25 };
            lblStatus = new Label
            {
                Text = "AGUARDANDO",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(234, 179, 8),
                BackColor = Color.FromArgb(66, 32, 6),
                TextAlign = ContentAlignment.MiddleCenter,
                Top = 122,
                Left = 75,
                Width = 140,
                Height = 30
            };

            pnlPedido.Controls.Add(lblPedido);
            pnlPedido.Controls.Add(lblCliente);
            pnlPedido.Controls.Add(lblQuantidade);
            pnlPedido.Controls.Add(lblStatusTitle);
            pnlPedido.Controls.Add(lblStatus);
            pnlMain.Controls.Add(gbPedido, 0, 0);

            
            GroupBox gbFicha = new GroupBox
            {
                Text = " PARÂMETROS DO LASER (FICHA TÉCNICA) ",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(161, 161, 170),
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                FlatStyle = FlatStyle.Flat
            };

            Panel pnlFicha = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            gbFicha.Controls.Add(pnlFicha);

            lblFichaNome = new Label { Text = "Nenhuma Ficha Técnica Vinculada", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.FromArgb(251, 146, 60), Top = 15, Width = 350, Height = 30 };
            lblFichaParams = new Label
            {
                Text = "Parâmetros não configurados.",
                Font = new Font("Consolas", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(167, 243, 208), 
                Top = 55,
                Width = 350,
                Height = 120
            };

            pnlFicha.Controls.Add(lblFichaNome);
            pnlFicha.Controls.Add(lblFichaParams);
            pnlMain.Controls.Add(gbFicha, 1, 0);

            
            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 90, BackColor = Color.FromArgb(39, 39, 42) };
            this.Controls.Add(pnlBottom);
            pnlBottom.BringToFront();

            btnIniciar = new Button
            {
                Text = "Iniciar Gravação",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(34, 197, 94), 
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(20, 20),
                Size = new Size(180, 50)
            };
            btnIniciar.FlatAppearance.BorderSize = 0;
            btnIniciar.Click += BtnIniciar_Click;

            btnFinalizar = new Button
            {
                Text = "Finalizar Gravação",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(59, 130, 246), 
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(210, 20),
                Size = new Size(180, 50)
            };
            btnFinalizar.FlatAppearance.BorderSize = 0;
            btnFinalizar.Click += BtnFinalizar_Click;

            btnRegravar = new Button
            {
                Text = "Regravar Brinco",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(239, 68, 68), 
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(400, 20),
                Size = new Size(180, 50)
            };
            btnRegravar.FlatAppearance.BorderSize = 0;
            btnRegravar.Click += BtnRegravar_Click;
            btnRegravar.Visible = Program.UsuarioLogado != null && Program.UsuarioLogado.NivelMaster;

            btnVoltar = new Button
            {
                Text = "Fechar / Voltar",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(113, 113, 122), 
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(640, 20),
                Size = new Size(180, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnVoltar.FlatAppearance.BorderSize = 0;
            btnVoltar.Click += (s, e) => this.Close();

            pnlBottom.Controls.Add(btnIniciar);
            pnlBottom.Controls.Add(btnFinalizar);
            pnlBottom.Controls.Add(btnRegravar);
            pnlBottom.Controls.Add(btnVoltar);
        }

        private void AtualizarTela()
        {
            if (_producao == null) return;

            lblPedido.Text = $"Ordem/Pedido: {_producao.Pedido}";
            lblCliente.Text = $"Cliente: {_producao.Cliente}";
            lblQuantidade.Text = $"Quantidade: {_producao.Quantidade} un";

            
            lblStatus.Text = _producao.Status.ToUpper();
            if (_producao.Status.Equals("Aguardando", StringComparison.OrdinalIgnoreCase))
            {
                lblStatus.ForeColor = Color.FromArgb(234, 179, 8); 
                lblStatus.BackColor = Color.FromArgb(66, 32, 6);
                btnIniciar.Enabled = true;
                btnFinalizar.Enabled = false;
            }
            else if (_producao.Status.Equals("Em Andamento", StringComparison.OrdinalIgnoreCase))
            {
                lblStatus.ForeColor = Color.FromArgb(34, 197, 94); 
                lblStatus.BackColor = Color.FromArgb(20, 83, 45);
                btnIniciar.Enabled = false;
                btnFinalizar.Enabled = true;
            }
            else 
            {
                lblStatus.ForeColor = Color.FromArgb(161, 161, 170); 
                lblStatus.BackColor = Color.FromArgb(39, 39, 42);
                btnIniciar.Enabled = true; 
                btnFinalizar.Enabled = false;
            }

            
            if (_ficha != null)
            {
                lblFichaNome.Text = _ficha.Nome;
                lblFichaParams.Text = $@"POTÊNCIA: {_ficha.Potencia:0.0}%
VELOCIDADE: {_ficha.Velocidade} mm/s
FREQUÊNCIA: {_ficha.Frequencia} kHz
PASSADAS: {_ficha.Passadas}
STATUS: {(_ficha.Ativa ? "ATIVA" : "INATIVA")}";
            }
            else
            {
                lblFichaNome.Text = "Nenhuma Ficha Técnica Vinculada";
                lblFichaParams.Text = "Parâmetros não configurados.";
            }
        }

        private void BtnIniciar_Click(object sender, EventArgs e)
        {
            if (_producao == null) return;

            try
            {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    conn.Execute("UPDATE Producoes SET Status = 'Em Andamento', Sincronizado = 0 WHERE Id = @Id", new { Id = _producao.Id });
                }
                LogAuditoria("Iniciar Gravação", $"Produção {_producao.Pedido} colocada Em Andamento pelo operador.");
                
                CarregarProducao(_producao.Id);
                AtualizarTela();

                if (DatabaseHelper.IsServerAvailable())
                {
                    SyncService.SincronizarProducoesPendentes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar status: " + ex.Message);
            }
        }

        private void BtnFinalizar_Click(object sender, EventArgs e)
        {
            if (_producao == null) return;

            try
            {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    conn.Execute("UPDATE Producoes SET Status = 'Finalizada', Sincronizado = 0 WHERE Id = @Id", new { Id = _producao.Id });
                }
                LogAuditoria("Finalizar Gravação", $"Produção {_producao.Pedido} finalizada pelo operador.");

                CarregarProducao(_producao.Id);
                AtualizarTela();

                if (DatabaseHelper.IsServerAvailable())
                {
                    SyncService.SincronizarProducoesPendentes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao finalizar produção: " + ex.Message);
            }
        }

        private void BtnRegravar_Click(object sender, EventArgs e)
        {
            if (_producao == null) return;

            string motivo = FrmPrincipal.PromptMotivoRegravacao();
            if (motivo == null) return; 

            string prefixo = "963";
            string numeroBrinco = "";

            try
            {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    
                    string sqlLast = "SELECT MAX(CAST(SUBSTR(Numero, 4) AS INTEGER)) FROM Brincos";
                    long lastSeq = conn.ExecuteScalar<long?>(sqlLast) ?? 0;
                    long nextSeq = lastSeq + 1;
                    numeroBrinco = prefixo + nextSeq.ToString("D12");

                    
                    conn.Execute(@"
                        INSERT INTO Brincos (Numero, DataGravacao, MaquinaId, MotivoRegravacao, Sincronizado) 
                        VALUES (@Numero, @Data, @MaquinaId, @Motivo, 0)",
                        new { Numero = numeroBrinco, Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), MaquinaId = _producao.MaquinaId, Motivo = motivo });

                    
                    var nova = new Producao
                    {
                        Pedido = _producao.Pedido + "-R",
                        Cliente = _producao.Cliente,
                        NumeroProducao = numeroBrinco,
                        Status = "Aguardando",
                        Quantidade = 1,
                        DataProducao = DateTime.Now,
                        UsuarioId = Program.UsuarioLogado?.Id ?? 1,
                        MaquinaId = _producao.MaquinaId,
                        FichaTecnicaId = _producao.FichaTecnicaId,
                        Sincronizado = false
                    };

                    string sqlProd = @"
                        INSERT INTO Producoes (Pedido, Cliente, NumeroProducao, Status, Quantidade, DataProducao, UsuarioId, MaquinaId, FichaTecnicaId, Sincronizado) 
                        VALUES (@Pedido, @Cliente, @NumeroProducao, @Status, @Quantidade, @DataProducao, @UsuarioId, @MaquinaId, @FichaTecnicaId, 0)";
                    conn.Execute(sqlProd, new {
                        nova.Pedido,
                        nova.Cliente,
                        nova.NumeroProducao,
                        nova.Status,
                        nova.Quantidade,
                        DataProducao = nova.DataProducao.ToString("yyyy-MM-dd HH:mm:ss"),
                        nova.UsuarioId,
                        nova.MaquinaId,
                        nova.FichaTecnicaId
                    });
                }

                LogAuditoria("Regravar Brinco", $"Brinco {numeroBrinco} gerado para o pedido {_producao.Pedido}. Motivo: {motivo}");
                MessageBox.Show($"Regravação criada!\nNúmero do brinco gerado: {numeroBrinco}\nMotivo: {motivo}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (DatabaseHelper.IsServerAvailable())
                {
                    SyncService.SincronizarProducoesPendentes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao gerar regravação: " + ex.Message);
            }
        }

        private void LogAuditoria(string acao, string detalhes)
        {
            try
            {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    conn.Execute("INSERT INTO Auditoria (DataHora, UsuarioId, Acao, Detalhes, Sincronizado) VALUES (@DataHora, @UsuarioId, @Acao, @Detalhes, 0)",
                        new { DataHora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), UsuarioId = Program.UsuarioLogado?.Id ?? 1, Acao = acao, Detalhes = detalhes });
                }
            }
            catch { }
        }
    }
}
