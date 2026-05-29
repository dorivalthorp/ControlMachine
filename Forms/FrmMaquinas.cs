using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using ControlMachine.Models;
using ControlMachine.Data;

namespace ControlMachine.Forms
{
    public class FrmMaquinas : Form
    {
        private DataGridView grid;
        private TextBox txtNome;
        private TextBox txtDescricao;
        private CheckBox chkAtiva;
        private Button btnSalvar;
        private Button btnLimpar;
        private int idSelecionado = 0;

        public FrmMaquinas()
        {
            this.Text = "Gerenciar Máquinas Laser (Local)";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 120 };

            pnlTop.Controls.Add(new Label { Text = "Nome:", Left = 10, Top = 10, Width = 80 });
            txtNome = new TextBox { Left = 100, Top = 8, Width = 200 };
            pnlTop.Controls.Add(txtNome);

            pnlTop.Controls.Add(new Label { Text = "Descrição:", Left = 10, Top = 40, Width = 80 });
            txtDescricao = new TextBox { Left = 100, Top = 38, Width = 300 };
            pnlTop.Controls.Add(txtDescricao);

            chkAtiva = new CheckBox { Text = "Ativa", Left = 100, Top = 70, Checked = true };
            pnlTop.Controls.Add(chkAtiva);

            btnSalvar = new Button { Text = "Salvar", Left = 420, Top = 36, Width = 100 };
            btnSalvar.Click += BtnSalvar_Click;
            pnlTop.Controls.Add(btnSalvar);

            btnLimpar = new Button { Text = "Limpar", Left = 530, Top = 36, Width = 100 };
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
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nome", HeaderText = "Nome", Width = 150 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Descricao", HeaderText = "Descrição", Width = 250 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "Ativa", HeaderText = "Ativa", Width = 50 });

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
                    var maquinas = conn.Query<MaquinaLaser>("SELECT * FROM MaquinasLaser").ToList();
                    grid.DataSource = maquinas;
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
                txtDescricao.Text = row.Cells[2].Value?.ToString();
                chkAtiva.Checked = Convert.ToBoolean(row.Cells[3].Value);
            }
        }

        private void LimparForm()
        {
            idSelecionado = 0;
            txtNome.Text = "";
            txtDescricao.Text = "";
            chkAtiva.Checked = true;
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Nome é obrigatório.");
                return;
            }

            try {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    if (idSelecionado == 0)
                    {
                        string sql = "INSERT INTO MaquinasLaser (Id, Nome, Descricao, Ativa) VALUES ((SELECT IFNULL(MAX(Id),0)+1 FROM MaquinasLaser m), @Nome, @Descricao, @Ativa)";
                        conn.Execute(sql, new { Nome = txtNome.Text, Descricao = txtDescricao.Text, Ativa = chkAtiva.Checked });
                    }
                    else
                    {
                        string sql = "UPDATE MaquinasLaser SET Nome = @Nome, Descricao = @Descricao, Ativa = @Ativa WHERE Id = @Id";
                        conn.Execute(sql, new { Nome = txtNome.Text, Descricao = txtDescricao.Text, Ativa = chkAtiva.Checked, Id = idSelecionado });
                    }
                }
                
                CarregarDados();
                LimparForm();
                MessageBox.Show("Máquina salva localmente!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar: " + ex.Message);
            }
        }
    }
}
