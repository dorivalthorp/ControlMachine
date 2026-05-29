using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using ControlMachine.Models;
using ControlMachine.Data;
using Dapper;

namespace ControlMachine.Forms
{
    public class FrmPrincipal : Form
    {
        private DataGridView gridProducoes;
        private Timer timerRefresh;
        private ProducaoRepository _repository;

        
        private TextBox txtPedidoFiltro;
        private TextBox txtUsuarioFiltro;
        private ComboBox cmbStatusFiltro;
        private TextBox txtScanner;
        private Button btnBuscar;
        private TextBox txtClienteFiltro;
        private TextBox txtNumeroFiltro;
        private DateTimePicker dtpInicio;
        private DateTimePicker dtpFim;
        private CheckBox chkFiltroData;

        
        private Button btnNovaProducao;
        private Button btnRegravarBrinco;
        private Button btnModoOperador;
        private Button btnExportar;
        private MenuStrip menuStrip1;

        public FrmPrincipal()
        {
            _repository = new ProducaoRepository();
            InitializeComponent();
            CarregarDados();
        }

        private void InitializeComponent()
        {
            this.Text = "ControlMachine - Sistema de Produção Laser";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;

            
            menuStrip1 = new MenuStrip();
            var menuSistema = new ToolStripMenuItem("Sistema");
            
            if (Program.UsuarioLogado != null && Program.UsuarioLogado.NivelMaster)
            {
                menuSistema.DropDownItems.Add("Gerenciar Usuários", null, (s, e) => { var f = new FrmUsuarios(); if (Program.AppIcon != null) f.Icon = Program.AppIcon; f.ShowDialog(); });
                menuSistema.DropDownItems.Add("Gerenciar Máquinas", null, (s, e) => { var f = new FrmMaquinas(); if (Program.AppIcon != null) f.Icon = Program.AppIcon; f.ShowDialog(); });
                menuSistema.DropDownItems.Add("Fichas Técnicas (Receitas)", null, (s, e) => { var f = new FrmFichasTecnicas(); if (Program.AppIcon != null) f.Icon = Program.AppIcon; f.ShowDialog(); });
                menuSistema.DropDownItems.Add("Parâmetros do Sistema", null, (s, e) => { var f = new FrmParametros(); if (Program.AppIcon != null) f.Icon = Program.AppIcon; f.ShowDialog(); });
                menuSistema.DropDownItems.Add("Histórico de Brincos", null, (s, e) => { var f = new FrmBrincos(); if (Program.AppIcon != null) f.Icon = Program.AppIcon; f.ShowDialog(); });
                menuSistema.DropDownItems.Add("Log de Auditoria", null, (s, e) => { var f = new FrmAuditoria(); if (Program.AppIcon != null) f.Icon = Program.AppIcon; f.ShowDialog(); });
                menuSistema.DropDownItems.Add("Dashboard", null, (s, e) => { var f = new FrmDashboard(); if (Program.AppIcon != null) f.Icon = Program.AppIcon; f.ShowDialog(); });
            }
            else
            {
                menuSistema.DropDownItems.Add("Alterar Minha Senha", null, (s, e) => { var f = new FrmUsuarios(); if (Program.AppIcon != null) f.Icon = Program.AppIcon; f.ShowDialog(); });
            }
            menuSistema.DropDownItems.Add(new ToolStripSeparator());
            menuSistema.DropDownItems.Add("Sair da Aplicação", null, (s, e) => {
                if (MessageBox.Show("Deseja realmente sair do sistema?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Application.Exit();
                }
            });
            menuStrip1.Items.Add(menuSistema);

            if (Program.UsuarioLogado != null && Program.UsuarioLogado.NivelMaster)
            {
                var menuImportar = new ToolStripMenuItem("Importação");
                menuImportar.DropDownItems.Add("Importar Brincos de CSV...", null, MenuImportarBrincos_Click);
                menuImportar.DropDownItems.Add("Importar Pedidos de CSV...", null, MenuImportarPedidos_Click);
                menuImportar.DropDownItems.Add("Importar Usuários de CSV...", null, MenuImportarUsuarios_Click);
                menuStrip1.Items.Add(menuImportar);
            }

            var menuAjuda = new ToolStripMenuItem("Ajuda");
            menuAjuda.DropDownItems.Add("Manual Rápido", null, MenuManual_Click);
            menuAjuda.DropDownItems.Add("Sobre", null, MenuSobre_Click);
            menuStrip1.Items.Add(menuAjuda);

            this.Controls.Add(menuStrip1);

            
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 125 };
            
            
            pnlTop.Controls.Add(new Label { Text = "Pedido:", Left = 10, Top = 15, Width = 50 });
            txtPedidoFiltro = new TextBox { Left = 65, Top = 12, Width = 100 };
            txtPedidoFiltro.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; CarregarDados(); } };
            pnlTop.Controls.Add(txtPedidoFiltro);

            pnlTop.Controls.Add(new Label { Text = "Cliente:", Left = 175, Top = 15, Width = 50 });
            txtClienteFiltro = new TextBox { Left = 225, Top = 12, Width = 100 };
            txtClienteFiltro.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; CarregarDados(); } };
            pnlTop.Controls.Add(txtClienteFiltro);

            pnlTop.Controls.Add(new Label { Text = "Nº Prod.:", Left = 335, Top = 15, Width = 60 });
            txtNumeroFiltro = new TextBox { Left = 395, Top = 12, Width = 100 };
            txtNumeroFiltro.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; CarregarDados(); } };
            pnlTop.Controls.Add(txtNumeroFiltro);

            pnlTop.Controls.Add(new Label { Text = "Status:", Left = 505, Top = 15, Width = 50 });
            cmbStatusFiltro = new ComboBox { Left = 555, Top = 12, Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatusFiltro.Items.AddRange(new string[] { "Todos", "Aguardando", "Em Andamento", "Finalizada" });
            cmbStatusFiltro.SelectedIndex = 0;
            pnlTop.Controls.Add(cmbStatusFiltro);

            pnlTop.Controls.Add(new Label { Text = "Usuário:", Left = 665, Top = 15, Width = 50 });
            txtUsuarioFiltro = new TextBox { Left = 715, Top = 12, Width = 100 };
            txtUsuarioFiltro.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; CarregarDados(); } };
            pnlTop.Controls.Add(txtUsuarioFiltro);

            
            pnlTop.Controls.Add(new Label { Text = "Período:", Left = 10, Top = 45, Width = 55 });
            dtpInicio = new DateTimePicker { Left = 65, Top = 42, Width = 100, Format = DateTimePickerFormat.Short };
            pnlTop.Controls.Add(dtpInicio);

            pnlTop.Controls.Add(new Label { Text = "até", Left = 170, Top = 45, Width = 25 });
            dtpFim = new DateTimePicker { Left = 195, Top = 42, Width = 100, Format = DateTimePickerFormat.Short };
            pnlTop.Controls.Add(dtpFim);

            chkFiltroData = new CheckBox { Text = "Filtrar Data", Left = 305, Top = 45, Width = 100 };
            chkFiltroData.CheckedChanged += (s, e) => CarregarDados();
            pnlTop.Controls.Add(chkFiltroData);

            pnlTop.Controls.Add(new Label { Text = "Leitor/OS:", Left = 410, Top = 45, Width = 60 });
            txtScanner = new TextBox { Left = 470, Top = 42, Width = 100 };
            txtScanner.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ProcessarScanner(txtScanner.Text.Trim()); } };
            pnlTop.Controls.Add(txtScanner);

            
            btnBuscar = new Button { Text = "Buscar", Left = 10, Top = 75, Width = 80 };
            btnBuscar.Click += (s, e) => CarregarDados();
            pnlTop.Controls.Add(btnBuscar);

            btnModoOperador = new Button { Text = "Modo Operador", Left = 100, Top = 75, Width = 110 };
            btnModoOperador.Click += BtnModoOperador_Click;
            pnlTop.Controls.Add(btnModoOperador);

            btnNovaProducao = new Button { Text = "Nova Prod.", Left = 220, Top = 75, Width = 90 };
            btnNovaProducao.Click += BtnNovaProducao_Click;
            pnlTop.Controls.Add(btnNovaProducao);

            btnExportar = new Button { Text = "Exportar", Left = 320, Top = 75, Width = 80 };
            btnExportar.Click += BtnExportar_Click;
            pnlTop.Controls.Add(btnExportar);

            if (Program.UsuarioLogado != null && Program.UsuarioLogado.NivelMaster)
            {
                btnRegravarBrinco = new Button { Text = "Regravar Brinco", Left = 410, Top = 75, Width = 110 };
                btnRegravarBrinco.Click += BtnRegravar_Click;
                pnlTop.Controls.Add(btnRegravarBrinco);

                var btnTransferir = new Button { Text = "Transferir Máq.", Left = 530, Top = 75, Width = 100 };
                btnTransferir.Click += BtnTransferir_Click;
                pnlTop.Controls.Add(btnTransferir);
            }

            var btnSair = new Button
            {
                Text = "Sair",
                Left = 840,
                Top = 75,
                Width = 80,
                BackColor = Color.FromArgb(220, 38, 38), 
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnSair.FlatAppearance.BorderSize = 0;
            btnSair.Click += (s, e) => {
                if (MessageBox.Show("Deseja realmente sair do sistema?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Application.Exit();
                }
            };
            pnlTop.Controls.Add(btnSair);

            this.Controls.Add(pnlTop);

            
            gridProducoes = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            gridProducoes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
            gridProducoes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Pedido", HeaderText = "Pedido", Width = 100 });
            gridProducoes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Cliente", HeaderText = "Cliente", Width = 150 });
            gridProducoes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NumeroProducao", HeaderText = "Número", Width = 140 });
            gridProducoes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Status", Width = 100 });
            gridProducoes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NomeUsuario", HeaderText = "Resp.", Width = 100 });
            gridProducoes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NomeFichaTecnica", HeaderText = "Ficha Técnica", Width = 150 });
            gridProducoes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantidade", HeaderText = "Qtd", Width = 60 });
            gridProducoes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DataProducao", HeaderText = "Data", Width = 120 });
            gridProducoes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Sincronizado", HeaderText = "Sync?", Width = 60 });

            
            gridProducoes.CellDoubleClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var prod = gridProducoes.Rows[e.RowIndex].DataBoundItem as Producao;
                    if (prod != null)
                    {
                        var f = new FrmModoOperador(prod.Id);
                        f.ShowDialog();
                        CarregarDados();
                    }
                }
            };

            this.Controls.Add(gridProducoes);
            gridProducoes.BringToFront(); 
            pnlTop.SendToBack();
            menuStrip1.SendToBack();

            
            timerRefresh = new Timer();
            timerRefresh.Interval = 5000; 
            timerRefresh.Tick += (s, e) => CarregarDados();
            timerRefresh.Start();
        }

        private void CarregarDados()
        {
            var todas = _repository.ObterTodas();

            
            if (Program.UsuarioLogado != null && !Program.UsuarioLogado.NivelMaster)
            {
                todas = todas.Where(p => p.Status != "Finalizada");
            }

            if (!string.IsNullOrEmpty(txtPedidoFiltro.Text))
                todas = todas.Where(p => p.Pedido != null && p.Pedido.IndexOf(txtPedidoFiltro.Text, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrEmpty(txtClienteFiltro.Text))
                todas = todas.Where(p => p.Cliente != null && p.Cliente.IndexOf(txtClienteFiltro.Text, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrEmpty(txtNumeroFiltro.Text))
                todas = todas.Where(p => p.NumeroProducao != null && p.NumeroProducao.IndexOf(txtNumeroFiltro.Text, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrEmpty(txtUsuarioFiltro.Text))
                todas = todas.Where(p => p.NomeUsuario != null && p.NomeUsuario.IndexOf(txtUsuarioFiltro.Text, StringComparison.OrdinalIgnoreCase) >= 0);

            if (cmbStatusFiltro.SelectedIndex > 0)
                todas = todas.Where(p => p.Status == cmbStatusFiltro.SelectedItem.ToString());

            if (chkFiltroData.Checked)
            {
                var inicio = dtpInicio.Value.Date;
                var fim = dtpFim.Value.Date.AddDays(1).AddSeconds(-1);
                todas = todas.Where(p => p.DataProducao >= inicio && p.DataProducao <= fim);
            }

            
            int rowIndex = -1;
            if (gridProducoes.SelectedRows.Count > 0)
                rowIndex = gridProducoes.SelectedRows[0].Index;

            gridProducoes.DataSource = todas.ToList();

            if (rowIndex >= 0 && rowIndex < gridProducoes.Rows.Count)
                gridProducoes.Rows[rowIndex].Selected = true;
        }

        private void ProcessarScanner(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return;

            var todas = _repository.ObterTodas().ToList();
            var matched = todas.FirstOrDefault(p => 
                p.Pedido.Equals(code, StringComparison.OrdinalIgnoreCase) || 
                p.NumeroProducao.Equals(code, StringComparison.OrdinalIgnoreCase));

            if (matched != null)
            {
                txtScanner.Text = "";
                foreach (DataGridViewRow row in gridProducoes.Rows)
                {
                    var prod = row.DataBoundItem as Producao;
                    if (prod != null && prod.Id == matched.Id)
                    {
                        gridProducoes.ClearSelection();
                        row.Selected = true;
                        break;
                    }
                }

                
                var f = new FrmModoOperador(matched.Id);
                f.ShowDialog();
                CarregarDados();
            }
            else
            {
                MessageBox.Show($"Nenhuma ordem de produção encontrada para o código: {code}", "Leitor de Código", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtScanner.SelectAll();
                txtScanner.Focus();
            }
        }

        private void BtnNovaProducao_Click(object sender, EventArgs e)
        {
            using (var form = new FrmNovaProducao())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var p = form.NovaProducao;
                    _repository.Inserir(p);
                    LogAuditoria("Nova Produção", $"Pedido {p.Pedido} criado para a máquina base.");
                    CarregarDados();
                }
            }
        }

        private void BtnModoOperador_Click(object sender, EventArgs e)
        {
            if (gridProducoes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecione uma ordem de produção na lista primeiro.", "Modo Operador", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var prod = gridProducoes.SelectedRows[0].DataBoundItem as Producao;
            if (prod != null)
            {
                var f = new FrmModoOperador(prod.Id);
                f.ShowDialog();
                CarregarDados();
            }
        }

        public static string PromptMotivoRegravacao()
        {
            using (var form = new Form { Width = 380, Height = 180, Text = "Motivo da Regravação", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false })
            {
                if (Program.AppIcon != null) form.Icon = Program.AppIcon;
                form.Controls.Add(new Label { Text = "Selecione ou digite o motivo da regravação:", Left = 20, Top = 20, Width = 320 });
                
                var cmbMotivo = new ComboBox { Left = 20, Top = 50, Width = 320, DropDownStyle = ComboBoxStyle.DropDown };
                cmbMotivo.Items.AddRange(new string[] { "Erro do Operador", "Foco Incorreto", "Falha no Laser", "Peça Defeituosa", "Instabilidade Elétrica" });
                cmbMotivo.SelectedIndex = 0;
                form.Controls.Add(cmbMotivo);

                var btnOk = new Button { Text = "Confirmar", Left = 130, Top = 95, Width = 100, DialogResult = DialogResult.OK };
                var btnCancel = new Button { Text = "Cancelar", Left = 240, Top = 95, Width = 100, DialogResult = DialogResult.Cancel };
                form.Controls.Add(btnOk);
                form.Controls.Add(btnCancel);
                form.AcceptButton = btnOk;

                if (form.ShowDialog() == DialogResult.OK)
                {
                    string motivo = cmbMotivo.Text.Trim();
                    return string.IsNullOrEmpty(motivo) ? "Não Especificado" : motivo;
                }
                return null;
            }
        }

        private void BtnRegravar_Click(object sender, EventArgs e)
        {
            if (gridProducoes.SelectedRows.Count == 0) return;
            var prod = gridProducoes.SelectedRows[0].DataBoundItem as Producao;
            
            string motivo = PromptMotivoRegravacao();
            if (motivo == null) return; 

            string prefixo = "963";
            string numeroBrinco = "";

            using (var conn = DatabaseHelper.GetLocalConnection())
            {
                string sqlLast = "SELECT MAX(CAST(SUBSTR(Numero, 4) AS INTEGER)) FROM Brincos";
                long lastSeq = conn.ExecuteScalar<long?>(sqlLast) ?? 0;
                long nextSeq = lastSeq + 1;
                numeroBrinco = prefixo + nextSeq.ToString("D12");

                conn.Execute("INSERT INTO Brincos (Numero, DataGravacao, MaquinaId, MotivoRegravacao, Sincronizado) VALUES (@Numero, @Data, @MaquinaId, @Motivo, 0)",
                    new { Numero = numeroBrinco, Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), MaquinaId = prod.MaquinaId, Motivo = motivo });

                var nova = new Producao
                {
                    Pedido = prod.Pedido + "-R",
                    Cliente = prod.Cliente,
                    NumeroProducao = numeroBrinco,
                    Status = "Aguardando",
                    Quantidade = 1,
                    DataProducao = DateTime.Now,
                    UsuarioId = Program.UsuarioLogado?.Id ?? 1,
                    MaquinaId = prod.MaquinaId,
                    FichaTecnicaId = prod.FichaTecnicaId,
                    Sincronizado = false
                };

                string sqlProd = "INSERT INTO Producoes (Pedido, Cliente, NumeroProducao, Status, Quantidade, DataProducao, UsuarioId, MaquinaId, FichaTecnicaId, Sincronizado) " +
                             "VALUES (@Pedido, @Cliente, @NumeroProducao, @Status, @Quantidade, @DataProducao, @UsuarioId, @MaquinaId, @FichaTecnicaId, 0)";
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
            LogAuditoria("Regravar Brinco", $"Brinco {numeroBrinco} gerado para o pedido {prod.Pedido}. Motivo: {motivo}");
            CarregarDados();
            MessageBox.Show($"Regravação criada!\nNúmero do brinco gerado: {numeroBrinco}\nMotivo: {motivo}", "Sucesso");
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                var producoes = _repository.ObterTodas().ToList();
                string path = @"c:\temp\";
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                
                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Producoes");
                    worksheet.Cell(1, 1).Value = "ID";
                    worksheet.Cell(1, 2).Value = "Pedido";
                    worksheet.Cell(1, 3).Value = "Cliente";
                    worksheet.Cell(1, 4).Value = "Número";
                    worksheet.Cell(1, 5).Value = "Status";
                    worksheet.Cell(1, 6).Value = "Quantidade";
                    worksheet.Cell(1, 7).Value = "Data";
                    worksheet.Cell(1, 8).Value = "Responsável";
                    
                    int row = 2;
                    foreach(var p in producoes)
                    {
                        worksheet.Cell(row, 1).Value = p.Id;
                        worksheet.Cell(row, 2).Value = p.Pedido;
                        worksheet.Cell(row, 3).Value = p.Cliente;
                        worksheet.Cell(row, 4).Value = p.NumeroProducao;
                        worksheet.Cell(row, 5).Value = p.Status;
                        worksheet.Cell(row, 6).Value = p.Quantidade;
                        worksheet.Cell(row, 7).Value = p.DataProducao.ToString("yyyy-MM-dd HH:mm");
                        worksheet.Cell(row, 8).Value = p.NomeUsuario;
                        row++;
                    }

                    workbook.SaveAs(path + "ProducoesExportadas.xlsx");
                }

                
                ControlMachine.Helpers.OdsExporter.Export(path + "ProducoesExportadas.ods", producoes);

                
                var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Producao>));
                using (var writer = new System.IO.StreamWriter(path + "ProducoesExportadas.xml"))
                {
                    xmlSerializer.Serialize(writer, producoes);
                }
                
                LogAuditoria("Exportar", "Exportou produções para XLSX, ODS e XML em c:\\temp\\");
                MessageBox.Show(@"Exportado com sucesso para c:\temp\ nos formatos:
- XML (ProducoesExportadas.xml)
- LibreOffice Calc (ProducoesExportadas.ods)
- Excel (ProducoesExportadas.xlsx)", "Exportação Concluída", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao exportar: " + ex.Message);
            }
        }

        private void BtnTransferir_Click(object sender, EventArgs e)
        {
            if (gridProducoes.SelectedRows.Count == 0) return;
            var producaoAtual = gridProducoes.SelectedRows[0].DataBoundItem as Producao;
            if (producaoAtual != null)
            {
                using (var form = new Form { Width = 350, Height = 180, Text = "Transferir Máquina", StartPosition = FormStartPosition.CenterParent })
                {
                    if (Program.AppIcon != null) form.Icon = Program.AppIcon;
                    form.Controls.Add(new Label { Text = "Selecione a nova máquina:", Left = 20, Top = 20, Width = 300 });
                    var cmbMaquinas = new ComboBox { Left = 20, Top = 50, Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
                    
                    using (var conn = DatabaseHelper.GetLocalConnection())
                    {
                        var maquinas = conn.Query<MaquinaLaser>("SELECT * FROM MaquinasLaser WHERE Ativa = 1").ToList();
                        cmbMaquinas.DataSource = maquinas;
                        cmbMaquinas.DisplayMember = "Nome";
                        cmbMaquinas.ValueMember = "Id";
                    }

                    var btnOk = new Button { Text = "Transferir", Left = 20, Top = 90, DialogResult = DialogResult.OK };
                    form.Controls.Add(cmbMaquinas);
                    form.Controls.Add(btnOk);
                    form.AcceptButton = btnOk;

                    if (form.ShowDialog() == DialogResult.OK && cmbMaquinas.SelectedItem != null)
                    {
                        int newId = (int)cmbMaquinas.SelectedValue;
                        string nomeMaquina = ((MaquinaLaser)cmbMaquinas.SelectedItem).Nome;

                        using (var conn = DatabaseHelper.GetLocalConnection())
                        {
                            conn.Execute("UPDATE Producoes SET MaquinaId = @MaquinaId, Sincronizado = 0 WHERE Id = @Id", new { MaquinaId = newId, Id = producaoAtual.Id });
                        }
                        
                        LogAuditoria("Transferência", $"Produção {producaoAtual.Pedido} transferida para máquina '{nomeMaquina}'.");
                        CarregarDados();
                        MessageBox.Show("Máquina transferida com sucesso!");
                    }
                }
            }
        }

        private void LogAuditoria(string acao, string detalhes)
        {
            try {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    conn.Execute("INSERT INTO Auditoria (DataHora, UsuarioId, Acao, Detalhes, Sincronizado) VALUES (@DataHora, @UsuarioId, @Acao, @Detalhes, 0)",
                        new { DataHora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), UsuarioId = Program.UsuarioLogado?.Id ?? 1, Acao = acao, Detalhes = detalhes });
                }
            } catch { }
        }

        private void MenuImportarBrincos_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "Arquivos CSV (*.csv)|*.csv", Title = "Selecione o CSV de Brincos" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string res = ControlMachine.Helpers.CsvImporter.ImportBrincos(ofd.FileName);
                    MessageBox.Show(res, "Importação de Brincos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LogAuditoria("Importar Brincos", $"Importação de CSV de brincos: {ofd.FileName}");
                }
            }
        }

        private void MenuImportarPedidos_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "Arquivos CSV (*.csv)|*.csv", Title = "Selecione o CSV de Pedidos/Produções" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string res = ControlMachine.Helpers.CsvImporter.ImportPedidos(ofd.FileName);
                    MessageBox.Show(res, "Importação de Pedidos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LogAuditoria("Importar Pedidos", $"Importação de CSV de pedidos: {ofd.FileName}");
                    CarregarDados();
                }
            }
        }

        private void MenuImportarUsuarios_Click(object sender, EventArgs e)
        {
            using (var formPassword = new Form { Width = 350, Height = 180, Text = "Senha Padrão para Novos Usuários", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false })
            {
                if (Program.AppIcon != null) formPassword.Icon = Program.AppIcon;
                formPassword.Controls.Add(new Label { Text = "Digite a senha padrão para os usuários importados:", Left = 20, Top = 20, Width = 300 });
                var txtSenha = new TextBox { Left = 20, Top = 50, Width = 280, PasswordChar = '*' };
                txtSenha.Text = "mudar123";
                formPassword.Controls.Add(txtSenha);

                var btnOk = new Button { Text = "Confirmar", Left = 110, Top = 95, Width = 100, DialogResult = DialogResult.OK };
                var btnCancel = new Button { Text = "Cancelar", Left = 220, Top = 95, Width = 100, DialogResult = DialogResult.Cancel };
                formPassword.Controls.Add(btnOk);
                formPassword.Controls.Add(btnCancel);
                formPassword.AcceptButton = btnOk;

                if (formPassword.ShowDialog() == DialogResult.OK)
                {
                    string senhaPadrao = txtSenha.Text;
                    if (string.IsNullOrWhiteSpace(senhaPadrao))
                    {
                        MessageBox.Show("A senha padrão não pode estar em branco.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    using (var ofd = new OpenFileDialog { Filter = "Arquivos CSV (*.csv)|*.csv", Title = "Selecione o CSV de Usuários" })
                    {
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            string res = ControlMachine.Helpers.CsvImporter.ImportUsuarios(ofd.FileName, senhaPadrao);
                            MessageBox.Show(res, "Importação de Usuários", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LogAuditoria("Importar Usuários", $"Importação de CSV de usuários: {ofd.FileName}");
                        }
                    }
                }
            }
        }

        private void MenuManual_Click(object sender, EventArgs e)
        {
            string msg = @"ControlMachine - Guia Rápido do Usuário

1. Como registrar uma produção:
   Clique no botão 'Nova Prod.', informe o Pedido, Cliente, Quantidade, selecione a Máquina e a Ficha Técnica de gravação desejada.
   
2. Modo Operador (Instruções Visuais):
   Dê um duplo clique em qualquer registro da grid principal ou selecione e clique em 'Modo Operador'. Esta tela exibe botões grandes para Iniciar, Finalizar e solicitar Regravação.

3. Sincronização em Tempo Real:
   O sistema opera de forma local e sincroniza com o servidor remoto MySQL automaticamente a cada 30 segundos, sendo 100% tolerante a falhas ou quedas de internet.

4. Importação e Exportação:
   Você pode exportar a lista atual para XML, Excel (.xlsx) e LibreOffice (.ods) clicando no botão 'Exportar'. Administradores podem importar Brincos, Pedidos e Usuários em lote através do menu 'Importação'.";

            MessageBox.Show(msg, "Manual do Usuário", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MenuSobre_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ControlMachine v1.2\nSistema de Controle e Monitoramento de Produção Laser\n\nDesenvolvido DT com tecnologias de ponta.", "Sobre o ControlMachine", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timerRefresh?.Stop();
            base.OnFormClosed(e);
            Application.Exit();
        }
    }
}
