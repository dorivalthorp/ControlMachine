using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using ControlMachine.Data;
using ControlMachine.Models;

namespace ControlMachine.Forms
{
    public class FrmNovaProducao : Form
    {
        private TextBox txtPedido;
        private TextBox txtCliente;
        private TextBox txtQuantidade;
        private ComboBox cmbMaquina;
        private ComboBox cmbFichaTecnica;
        
        public Producao NovaProducao { get; private set; }

        public FrmNovaProducao()
        {
            this.Text = "Nova Produção";
            this.Size = new Size(400, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            if (Program.AppIcon != null) this.Icon = Program.AppIcon;

            int top = 20;

            this.Controls.Add(new Label { Text = "Pedido:", Left = 20, Top = top, Width = 100 });
            txtPedido = new TextBox { Left = 130, Top = top - 2, Width = 220, Text = "PED-" + new Random().Next(1000, 9999) };
            this.Controls.Add(txtPedido);

            top += 35;
            this.Controls.Add(new Label { Text = "Cliente:", Left = 20, Top = top, Width = 100 });
            txtCliente = new TextBox { Left = 130, Top = top - 2, Width = 220 };
            this.Controls.Add(txtCliente);

            top += 35;
            this.Controls.Add(new Label { Text = "Quantidade:", Left = 20, Top = top, Width = 100 });
            txtQuantidade = new TextBox { Left = 130, Top = top - 2, Width = 100, Text = "100" };
            this.Controls.Add(txtQuantidade);

            top += 35;
            this.Controls.Add(new Label { Text = "Máquina Laser:", Left = 20, Top = top, Width = 100 });
            cmbMaquina = new ComboBox { Left = 130, Top = top - 2, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbMaquina);

            top += 35;
            this.Controls.Add(new Label { Text = "Ficha Técnica:", Left = 20, Top = top, Width = 100 });
            cmbFichaTecnica = new ComboBox { Left = 130, Top = top - 2, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbFichaTecnica);

            top += 45;
            var btnSalvar = new Button { Text = "Salvar", Left = 130, Top = top, Width = 100, DialogResult = DialogResult.OK };
            btnSalvar.Click += BtnSalvar_Click;
            this.Controls.Add(btnSalvar);

            var btnCancelar = new Button { Text = "Cancelar", Left = 250, Top = top, Width = 100, DialogResult = DialogResult.Cancel };
            this.Controls.Add(btnCancelar);

            this.AcceptButton = btnSalvar;

            CarregarCombos();
        }

        private void CarregarCombos()
        {
            try
            {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    var maquinas = conn.Query<MaquinaLaser>("SELECT * FROM MaquinasLaser WHERE Ativa = 1").ToList();
                    cmbMaquina.DataSource = maquinas;
                    cmbMaquina.DisplayMember = "Nome";
                    cmbMaquina.ValueMember = "Id";

                    var fichas = conn.Query<FichaTecnica>("SELECT * FROM FichasTecnicas WHERE Ativa = 1").ToList();
                    cmbFichaTecnica.DataSource = fichas;
                    cmbFichaTecnica.DisplayMember = "Nome";
                    cmbFichaTecnica.ValueMember = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar configurações: " + ex.Message);
            }
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPedido.Text) || string.IsNullOrWhiteSpace(txtCliente.Text))
            {
                MessageBox.Show("Pedido e Cliente são obrigatórios!");
                this.DialogResult = DialogResult.None;
                return;
            }

            if (!int.TryParse(txtQuantidade.Text, out int qtd) || qtd <= 0)
            {
                MessageBox.Show("Quantidade deve ser um número inteiro válido e maior que zero!");
                this.DialogResult = DialogResult.None;
                return;
            }

            if (cmbMaquina.SelectedValue == null)
            {
                MessageBox.Show("Selecione uma máquina laser!");
                this.DialogResult = DialogResult.None;
                return;
            }

            NovaProducao = new Producao
            {
                Pedido = txtPedido.Text.Trim(),
                Cliente = txtCliente.Text.Trim(),
                NumeroProducao = "PROD-" + new Random().Next(100, 999),
                Status = "Aguardando",
                Quantidade = qtd,
                DataProducao = DateTime.Now,
                UsuarioId = Program.UsuarioLogado?.Id ?? 1,
                MaquinaId = (int)cmbMaquina.SelectedValue,
                FichaTecnicaId = cmbFichaTecnica.SelectedValue != null ? (int?)cmbFichaTecnica.SelectedValue : null
            };
        }
    }
}
