using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using ControlMachine.Models;
using ControlMachine.Data;

namespace ControlMachine.Forms
{
    public class FrmParametros : Form
    {
        private DataGridView grid;
        private TextBox txtChave, txtValor, txtDescricao;
        private int idSelecionado = 0;

        public FrmParametros()
        {
            this.Text = "Parâmetros do Sistema (Local)";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 120 };
            
            pnlTop.Controls.Add(new Label { Text = "Chave:", Left = 10, Top = 10, Width = 50 });
            txtChave = new TextBox { Left = 60, Top = 8, Width = 200 };
            pnlTop.Controls.Add(txtChave);

            pnlTop.Controls.Add(new Label { Text = "Valor:", Left = 10, Top = 40, Width = 50 });
            txtValor = new TextBox { Left = 60, Top = 38, Width = 200 };
            pnlTop.Controls.Add(txtValor);

            pnlTop.Controls.Add(new Label { Text = "Descrição:", Left = 10, Top = 70, Width = 60 });
            txtDescricao = new TextBox { Left = 70, Top = 68, Width = 300 };
            pnlTop.Controls.Add(txtDescricao);

            Button btnSalvar = new Button { Text = "Salvar", Left = 450, Top = 20, Width = 80 };
            btnSalvar.Click += BtnSalvar_Click;
            pnlTop.Controls.Add(btnSalvar);

            Button btnNovo = new Button { Text = "Limpar", Left = 450, Top = 50, Width = 80 };
            btnNovo.Click += (s, e) => LimparForm();
            pnlTop.Controls.Add(btnNovo);

            this.Controls.Add(pnlTop);

            grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false };
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 40 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Chave", HeaderText = "Chave", Width = 150 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Valor", HeaderText = "Valor", Width = 150 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Descricao", HeaderText = "Descrição", Width = 250 });
            
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
                    var param = conn.Query<Parametro>("SELECT * FROM Parametros").ToList();
                    grid.DataSource = param;
                }
            } catch { }
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var p = grid.Rows[e.RowIndex].DataBoundItem as Parametro;
                if (p != null)
                {
                    idSelecionado = p.Id;
                    txtChave.Text = p.Chave;
                    txtValor.Text = p.Valor;
                    txtDescricao.Text = p.Descricao;
                }
            }
        }

        private void LimparForm()
        {
            idSelecionado = 0;
            txtChave.Text = "";
            txtValor.Text = "";
            txtDescricao.Text = "";
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtChave.Text))
            {
                MessageBox.Show("Chave é obrigatória!");
                return;
            }

            try {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    if (idSelecionado == 0)
                    {
                        string sql = "INSERT INTO Parametros (Id, Chave, Valor, Descricao) VALUES ((SELECT IFNULL(MAX(Id),0)+1 FROM Parametros p), @Chave, @Valor, @Descricao)";
                        conn.Execute(sql, new { Chave = txtChave.Text, Valor = txtValor.Text, Descricao = txtDescricao.Text });
                    }
                    else
                    {
                        string sql = "UPDATE Parametros SET Chave = @Chave, Valor = @Valor, Descricao = @Descricao WHERE Id = @Id";
                        conn.Execute(sql, new { Chave = txtChave.Text, Valor = txtValor.Text, Descricao = txtDescricao.Text, Id = idSelecionado });
                    }
                }
                
                CarregarDados();
                LimparForm();
                MessageBox.Show("Parâmetro salvo localmente!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }
    }
}
