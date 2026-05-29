using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using ControlMachine.Models;
using ControlMachine.Data;
using ControlMachine.Helpers;

namespace ControlMachine.Forms
{
    public class FrmUsuarios : Form
    {
        private DataGridView grid;
        private TextBox txtNome, txtLogin, txtSenha, txtCodigoAcesso;
        private CheckBox chkMaster, chkAtivo;
        private int idSelecionado = 0;

        public FrmUsuarios()
        {
            if (Program.UsuarioLogado != null && !Program.UsuarioLogado.NivelMaster)
            {
                this.Text = "Meus Dados / Alterar Senha";
            }
            else
            {
                this.Text = "Gerenciar Usuários (Local)";
            }
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 120 };
            
            pnlTop.Controls.Add(new Label { Text = "Nome:", Left = 10, Top = 10, Width = 50 });
            txtNome = new TextBox { Left = 60, Top = 8, Width = 200 };
            pnlTop.Controls.Add(txtNome);

            pnlTop.Controls.Add(new Label { Text = "PIN/Crachá:", Left = 280, Top = 10, Width = 80 });
            txtCodigoAcesso = new TextBox { Left = 360, Top = 8, Width = 80 };
            pnlTop.Controls.Add(txtCodigoAcesso);

            pnlTop.Controls.Add(new Label { Text = "Login:", Left = 10, Top = 40, Width = 50 });
            txtLogin = new TextBox { Left = 60, Top = 38, Width = 150 };
            pnlTop.Controls.Add(txtLogin);

            pnlTop.Controls.Add(new Label { Text = "Senha:", Left = 220, Top = 40, Width = 50 });
            txtSenha = new TextBox { Left = 270, Top = 38, Width = 150, PasswordChar = '*' };
            pnlTop.Controls.Add(txtSenha);

            chkMaster = new CheckBox { Text = "Acesso Master", Left = 60, Top = 70, Width = 110 };
            pnlTop.Controls.Add(chkMaster);

            chkAtivo = new CheckBox { Text = "Usuário Ativo", Left = 180, Top = 70, Width = 100, Checked = true };
            pnlTop.Controls.Add(chkAtivo);

            if (Program.UsuarioLogado != null && !Program.UsuarioLogado.NivelMaster)
            {
                chkMaster.Enabled = false;
                chkAtivo.Enabled = false;
            }

            Button btnSalvar = new Button { Text = "Salvar", Left = 450, Top = 20, Width = 80 };
            btnSalvar.Click += BtnSalvar_Click;
            pnlTop.Controls.Add(btnSalvar);

            Button btnNovo = new Button { Text = "Limpar", Left = 450, Top = 50, Width = 80 };
            btnNovo.Click += (s, e) => LimparForm();
            pnlTop.Controls.Add(btnNovo);

            Button btnTrocarSenha = new Button { Text = "Trocar Senha", Left = 550, Top = 20, Width = 100 };
            btnTrocarSenha.Click += BtnTrocarSenha_Click;
            pnlTop.Controls.Add(btnTrocarSenha);

            Button btnResetarSenha = new Button { Text = "Resetar Senha", Left = 550, Top = 50, Width = 100 };
            btnResetarSenha.Click += BtnResetarSenha_Click;
            pnlTop.Controls.Add(btnResetarSenha);

            this.Controls.Add(pnlTop);

            
            grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false };
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 40 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nome", HeaderText = "Nome", Width = 180 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Login", HeaderText = "Login", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CodigoAcesso", HeaderText = "PIN/Crachá", Width = 100 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "NivelMaster", HeaderText = "Master", Width = 60 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "Ativo", HeaderText = "Ativo", Width = 60 });
            
            grid.CellDoubleClick += Grid_CellDoubleClick;
            this.Controls.Add(grid);
            grid.BringToFront();

            CarregarDados();
        }

        private void CarregarDados()
        {
            try {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    string sql = Program.UsuarioLogado.NivelMaster ? "SELECT * FROM Usuarios" : "SELECT * FROM Usuarios WHERE Id = @Id";
                    var usuarios = conn.Query<Usuario>(sql, new { Id = Program.UsuarioLogado.Id }).ToList();
                    grid.DataSource = usuarios;
                }
            } catch { }
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var user = grid.Rows[e.RowIndex].DataBoundItem as Usuario;
                if (user != null)
                {
                    idSelecionado = user.Id;
                    txtNome.Text = user.Nome;
                    txtLogin.Text = user.Login;
                    txtSenha.Text = ""; 
                    txtCodigoAcesso.Text = user.CodigoAcesso ?? "";
                    chkMaster.Checked = user.NivelMaster;
                    chkAtivo.Checked = user.Ativo;
                }
            }
        }

        private void LimparForm()
        {
            idSelecionado = 0;
            txtNome.Text = "";
            txtLogin.Text = "";
            txtSenha.Text = "";
            txtCodigoAcesso.Text = "";
            chkMaster.Checked = false;
            chkAtivo.Checked = true;
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) || string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Nome e Login são obrigatórios!");
                return;
            }

            try {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    if (idSelecionado == 0)
                    {
                        if (!Program.UsuarioLogado.NivelMaster)
                        {
                            MessageBox.Show("Apenas administradores podem criar novos usuários.");
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(txtSenha.Text))
                        {
                            MessageBox.Show("Senha é obrigatória para novos usuários!");
                            return;
                        }
                        string hash = SecurityHelper.GetSha256Hash(txtSenha.Text);
                        string sql = "INSERT INTO Usuarios (Id, Nome, Login, SenhaHash, CodigoAcesso, NivelMaster, Ativo) VALUES ((SELECT IFNULL(MAX(Id),0)+1 FROM Usuarios u), @Nome, @Login, @Senha, @CodigoAcesso, @Master, @Ativo)";
                        conn.Execute(sql, new { Nome = txtNome.Text, Login = txtLogin.Text, Senha = hash, CodigoAcesso = txtCodigoAcesso.Text, Master = chkMaster.Checked, Ativo = chkAtivo.Checked });
                    }
                    else
                    {
                        if (!Program.UsuarioLogado.NivelMaster && idSelecionado != Program.UsuarioLogado.Id)
                        {
                            MessageBox.Show("Você só pode alterar o seu próprio usuário.");
                            return;
                        }

                        
                        bool masterValue = chkMaster.Checked;
                        bool ativoValue = chkAtivo.Checked;
                        if (!Program.UsuarioLogado.NivelMaster)
                        {
                            var currentUser = conn.QueryFirstOrDefault<Usuario>("SELECT NivelMaster, Ativo FROM Usuarios WHERE Id = @Id", new { Id = idSelecionado });
                            if (currentUser != null)
                            {
                                masterValue = currentUser.NivelMaster;
                                ativoValue = currentUser.Ativo;
                            }
                        }

                        string sql;
                        if (!string.IsNullOrWhiteSpace(txtSenha.Text))
                        {
                            string hash = SecurityHelper.GetSha256Hash(txtSenha.Text);
                            sql = "UPDATE Usuarios SET Nome = @Nome, Login = @Login, SenhaHash = @Senha, CodigoAcesso = @CodigoAcesso, NivelMaster = @Master, Ativo = @Ativo WHERE Id = @Id";
                            conn.Execute(sql, new { Nome = txtNome.Text, Login = txtLogin.Text, Senha = hash, CodigoAcesso = txtCodigoAcesso.Text, Master = masterValue, Ativo = ativoValue, Id = idSelecionado });
                        }
                        else
                        {
                            sql = "UPDATE Usuarios SET Nome = @Nome, Login = @Login, CodigoAcesso = @CodigoAcesso, NivelMaster = @Master, Ativo = @Ativo WHERE Id = @Id";
                            conn.Execute(sql, new { Nome = txtNome.Text, Login = txtLogin.Text, CodigoAcesso = txtCodigoAcesso.Text, Master = masterValue, Ativo = ativoValue, Id = idSelecionado });
                        }
                    }
                }
                
                CarregarDados();
                LimparForm();
                MessageBox.Show("Usuário salvo localmente com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void BtnTrocarSenha_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0)
            {
                MessageBox.Show("Selecione um usuário na grid primeiro.");
                return;
            }

            if (!Program.UsuarioLogado.NivelMaster && idSelecionado != Program.UsuarioLogado.Id)
            {
                MessageBox.Show("Você só tem permissão para trocar a sua própria senha.");
                return;
            }

            using (var form = new Form { Width = 300, Height = 180, Text = "Trocar Senha", StartPosition = FormStartPosition.CenterParent })
            {
                if (Program.AppIcon != null) form.Icon = Program.AppIcon;
                form.Controls.Add(new Label { Text = "Nova Senha:", Left = 20, Top = 20, Width = 150 });
                var txtNovaSenha = new TextBox { Left = 20, Top = 50, Width = 240, PasswordChar = '*' };
                form.Controls.Add(txtNovaSenha);
                var btnOk = new Button { Text = "Alterar", Left = 20, Top = 90, DialogResult = DialogResult.OK };
                form.Controls.Add(btnOk);
                form.AcceptButton = btnOk;

                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(txtNovaSenha.Text)) return;
                    
                    try
                    {
                        string hash = SecurityHelper.GetSha256Hash(txtNovaSenha.Text);
                        using (var conn = DatabaseHelper.GetLocalConnection())
                        {
                            conn.Execute("UPDATE Usuarios SET SenhaHash = @Senha WHERE Id = @Id", new { Senha = hash, Id = idSelecionado });
                        }
                        MessageBox.Show("Senha alterada/resetada com sucesso!");
                    }
                    catch { }
                }
            }
        }

        private void BtnResetarSenha_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0)
            {
                MessageBox.Show("Selecione um usuário na grid primeiro.");
                return;
            }

            if (!Program.UsuarioLogado.NivelMaster && idSelecionado != Program.UsuarioLogado.Id)
            {
                MessageBox.Show("Você só tem permissão para trocar/resetar a sua própria senha.");
                return;
            }

            if (MessageBox.Show("Deseja realmente resetar a senha deste usuário para o padrão '123'?", "Confirmar Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    string hash = SecurityHelper.GetSha256Hash("123");
                    using (var conn = DatabaseHelper.GetLocalConnection())
                    {
                        conn.Execute("UPDATE Usuarios SET SenhaHash = @Senha WHERE Id = @Id", new { Senha = hash, Id = idSelecionado });
                    }
                    MessageBox.Show("Senha resetada para '123' com sucesso!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao resetar senha: " + ex.Message);
                }
            }
        }
    }
}
