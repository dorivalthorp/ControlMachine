using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using ControlMachine.Models;
using ControlMachine.Data;
using System.Collections.Generic;

namespace ControlMachine.Forms
{
    public class FrmBrincos : Form
    {
        private DataGridView grid;
        private TextBox txtPesquisa;

        public FrmBrincos()
        {
            this.Text = "Histórico de Brincos (Gerados)";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 60 };
            
            pnlTop.Controls.Add(new Label { Text = "Pesquisar Número:", Left = 10, Top = 20, Width = 110 });
            txtPesquisa = new TextBox { Left = 120, Top = 18, Width = 200 };
            pnlTop.Controls.Add(txtPesquisa);

            Button btnBuscar = new Button { Text = "Buscar", Left = 330, Top = 16, Width = 80 };
            btnBuscar.Click += (s, e) => CarregarDados();
            pnlTop.Controls.Add(btnBuscar);

            this.Controls.Add(pnlTop);

            grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false };
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Numero", HeaderText = "Número do Brinco", Width = 150 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DataGravacao", HeaderText = "Data da Geração", Width = 150 });
            
            this.Controls.Add(grid);
            grid.BringToFront();

            CarregarDados();
        }

        private void CarregarDados()
        {
            try {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    string sql = "SELECT * FROM Brincos ORDER BY DataGravacao DESC";
                    var brincos = conn.Query<Brinco>(sql).ToList();

                    if (!string.IsNullOrWhiteSpace(txtPesquisa.Text))
                    {
                        brincos = brincos.Where(b => b.Numero != null && b.Numero.Contains(txtPesquisa.Text)).ToList();
                    }

                    grid.DataSource = brincos;
                }
            } catch { }
        }
    }
}
