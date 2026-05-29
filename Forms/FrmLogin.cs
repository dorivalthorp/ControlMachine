using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using ControlMachine.Data;
using ControlMachine.Models;
using ControlMachine.Helpers;

namespace ControlMachine.Forms
{
    public class FrmLogin : Form
    {
        private TextBox txtLogin;
        private TextBox txtSenha;
        private TextBox txtQuick;
        private Button btnEntrar;
        private Label lblMessage;

        public FrmLogin()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Login - ControlMachine";
            this.Size = new Size(350, 330);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblUser = new Label { Text = "Usuário:", Left = 30, Top = 20, Width = 80 };
            txtLogin = new TextBox { Left = 120, Top = 20, Width = 180 };

            Label lblPass = new Label { Text = "Senha:", Left = 30, Top = 55, Width = 80 };
            txtSenha = new TextBox { Left = 120, Top = 55, Width = 180, PasswordChar = '*' };

            btnEntrar = new Button { Text = "Entrar", Left = 110, Top = 95, Width = 90 };
            btnEntrar.Click += BtnEntrar_Click;

            var btnSair = new Button { Text = "Sair", Left = 210, Top = 95, Width = 90 };
            btnSair.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            GroupBox gbQuick = new GroupBox { Text = "Acesso Rápido (Crachá / PIN)", Left = 30, Top = 135, Width = 280, Height = 75 };
            gbQuick.Controls.Add(new Label { Text = "Bipar/PIN:", Left = 10, Top = 32, Width = 70 });
            txtQuick = new TextBox { Left = 90, Top = 30, Width = 170, PasswordChar = '*' };
            txtQuick.KeyDown += TxtQuick_KeyDown;
            gbQuick.Controls.Add(txtQuick);

            lblMessage = new Label { Left = 30, Top = 230, Width = 280, Height = 40, ForeColor = Color.Red };

            this.Controls.Add(lblUser);
            this.Controls.Add(txtLogin);
            this.Controls.Add(lblPass);
            this.Controls.Add(txtSenha);
            this.Controls.Add(btnEntrar);
            this.Controls.Add(btnSair);
            this.Controls.Add(gbQuick);
            this.Controls.Add(lblMessage);
            
            this.AcceptButton = btnEntrar;
        }

        private void TxtQuick_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                RealizarLoginRapido(txtQuick.Text);
            }
        }

        private void RealizarLoginRapido(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return;

            using (var conn = DatabaseHelper.GetLocalConnection())
            {
                var usuario = conn.QueryFirstOrDefault<Usuario>("SELECT * FROM Usuarios WHERE CodigoAcesso = @Codigo AND Ativo = 1", new { Codigo = code });

                if (usuario != null)
                {
                    Program.UsuarioLogado = usuario;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblMessage.Text = "PIN ou Código de Crachá inválido/inativo.";
                    txtQuick.SelectAll();
                }
            }
        }

        private void BtnEntrar_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text;
            string senhaOriginal = txtSenha.Text;
            string senhaHash = SecurityHelper.GetSha256Hash(senhaOriginal);

            using (var conn = DatabaseHelper.GetLocalConnection())
            {
                var usuario = conn.QueryFirstOrDefault<Usuario>("SELECT * FROM Usuarios WHERE Login = @Login", new { Login = login });

                if (usuario != null && usuario.Ativo)
                {
                    if (usuario.SenhaHash == senhaHash || usuario.SenhaHash == senhaOriginal)
                    {
                        if (usuario.SenhaHash == senhaOriginal) 
                        {
                            conn.Execute("UPDATE Usuarios SET SenhaHash = @Hash WHERE Id = @Id", new { Hash = senhaHash, Id = usuario.Id });
                            usuario.SenhaHash = senhaHash;
                        }

                        Program.UsuarioLogado = usuario;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return;
                    }
                }
                
                lblMessage.Text = "Usuário ou senha inválidos, ou usuário inativo.";
            }
        }
    }
}
