using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using ControlMachine.Models;
using ControlMachine.Data;

namespace ControlMachine.Forms
{
    public class FrmFichasTecnicas : Form
    {
        private DataGridView grid;
        private TextBox txtNome;
        private TextBox txtPotencia;
        private TextBox txtVelocidade;
        private TextBox txtFrequencia;
        private TextBox txtPassadas;
        private CheckBox chkAtiva;
        private Button btnSalvar;
        private Button btnLimpar;
        private int idSelecionado = 0;

        public FrmFichasTecnicas()
        {
            this.Text = "Fichas Técnicas / Parâmetros do Laser";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 150 };

            pnlTop.Controls.Add(new Label { Text = "Nome:", Left = 10, Top = 10, Width = 80 });
            txtNome = new TextBox { Left = 100, Top = 8, Width = 200 };
            pnlTop.Controls.Add(txtNome);

            pnlTop.Controls.Add(new Label { Text = "Potência (%):", Left = 10, Top = 40, Width = 80 });
            txtPotencia = new TextBox { Left = 100, Top = 38, Width = 80 };
            pnlTop.Controls.Add(txtPotencia);

            pnlTop.Controls.Add(new Label { Text = "Velocidade:", Left = 200, Top = 40, Width = 80 });
            txtVelocidade = new TextBox { Left = 280, Top = 38, Width = 80 };
            pnlTop.Controls.Add(txtVelocidade);

            pnlTop.Controls.Add(new Label { Text = "Frequência:", Left = 10, Top = 70, Width = 80 });
            txtFrequencia = new TextBox { Left = 100, Top = 68, Width = 80 };
            pnlTop.Controls.Add(txtFrequencia);

            pnlTop.Controls.Add(new Label { Text = "Passadas:", Left = 200, Top = 70, Width = 80 });
            txtPassadas = new TextBox { Left = 280, Top = 68, Width = 80 };
            pnlTop.Controls.Add(txtPassadas);

            chkAtiva = new CheckBox { Text = "Receita Ativa", Left = 100, Top = 105, Checked = true, Width = 150 };
            pnlTop.Controls.Add(chkAtiva);

            btnSalvar = new Button { Text = "Salvar", Left = 450, Top = 36, Width = 100 };
            btnSalvar.Click += BtnSalvar_Click;
            pnlTop.Controls.Add(btnSalvar);

            btnLimpar = new Button { Text = "Limpar", Left = 560, Top = 36, Width = 100 };
            btnLimpar.Click += (s, e) => LimparForm();
            pnlTop.Controls.Add(btnLimpar);

            this.Controls.Add(pnlTop);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nome", HeaderText = "Nome Receita", Width = 220 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Potencia", HeaderText = "Potência %", Width = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Velocidade", HeaderText = "Vel (mm/s)", Width = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Frequencia", HeaderText = "Freq (kHz)", Width = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Passadas", HeaderText = "Passadas", Width = 80 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "Ativa", HeaderText = "Ativa", Width = 60 });

            grid.CellDoubleClick += Grid_CellDoubleClick;

            this.Controls.Add(grid);
            grid.BringToFront();

            this.Load += (s, e) => CarregarDados();
        }

        private void CarregarDados()
        {
            try {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    var fichas = conn.Query<FichaTecnica>("SELECT * FROM FichasTecnicas").ToList();
                    grid.DataSource = fichas;
                }
            } catch { }
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = grid.Rows[e.RowIndex];
                idSelecionado = Convert.ToInt32(row.Cells[0].Value);
                txtNome.Text = row.Cells[1].Value?.ToString();
                txtPotencia.Text = row.Cells[2].Value?.ToString();
                txtVelocidade.Text = row.Cells[3].Value?.ToString();
                txtFrequencia.Text = row.Cells[4].Value?.ToString();
                txtPassadas.Text = row.Cells[5].Value?.ToString();
                chkAtiva.Checked = Convert.ToBoolean(row.Cells[6].Value);
            }
        }

        private void LimparForm()
        {
            idSelecionado = 0;
            txtNome.Text = "";
            txtPotencia.Text = "";
            txtVelocidade.Text = "";
            txtFrequencia.Text = "";
            txtPassadas.Text = "";
            chkAtiva.Checked = true;
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Nome da receita é obrigatório.");
                return;
            }

            if (!double.TryParse(txtPotencia.Text, out double potencia) ||
                !int.TryParse(txtVelocidade.Text, out int velocidade) ||
                !int.TryParse(txtFrequencia.Text, out int frequencia) ||
                !int.TryParse(txtPassadas.Text, out int passadas))
            {
                MessageBox.Show("Por favor, preencha os parâmetros numéricos corretamente (Potência, Velocidade, Frequência, Passadas).");
                return;
            }

            try {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    if (idSelecionado == 0)
                    {
                        string sql = "INSERT INTO FichasTecnicas (Id, Nome, Potencia, Velocidade, Frequencia, Passadas, Ativa) VALUES ((SELECT IFNULL(MAX(Id),0)+1 FROM FichasTecnicas f), @Nome, @Potencia, @Velocidade, @Frequencia, @Passadas, @Ativa)";
                        conn.Execute(sql, new { Nome = txtNome.Text, Potencia = potencia, Velocidade = velocidade, Frequencia = frequencia, Passadas = passadas, Ativa = chkAtiva.Checked ? 1 : 0 });
                    }
                    else
                    {
                        string sql = "UPDATE FichasTecnicas SET Nome = @Nome, Potencia = @Potencia, Velocidade = @Velocidade, Frequencia = @Frequencia, Passadas = @Passadas, Ativa = @Ativa WHERE Id = @Id";
                        conn.Execute(sql, new { Nome = txtNome.Text, Potencia = potencia, Velocidade = velocidade, Frequencia = frequencia, Passadas = passadas, Ativa = chkAtiva.Checked ? 1 : 0, Id = idSelecionado });
                    }
                }
                
                CarregarDados();
                LimparForm();
                MessageBox.Show("Ficha técnica salva localmente com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar: " + ex.Message);
            }
        }
    }
}
