using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using ControlMachine.Models;
using ControlMachine.Data;

namespace ControlMachine.Forms
{
    public class FrmAuditoria : Form
    {
        private DataGridView grid;

        public FrmAuditoria()
        {
            this.Text = "Log de Auditoria (Apenas Consulta)";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DataHora", HeaderText = "Data/Hora", Width = 150 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UsuarioId", HeaderText = "ID Usuário", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Acao", HeaderText = "Ação", Width = 150 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Detalhes", HeaderText = "Detalhes", Width = 400 });

            this.Controls.Add(grid);
            this.Load += (s, e) => CarregarDados();
        }

        private void CarregarDados()
        {
            try {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    
                    var logs = conn.Query<Auditoria>("SELECT * FROM Auditoria ORDER BY Id DESC LIMIT 500").ToList();
                    grid.DataSource = logs;
                }
            } catch { }
        }
    }
}
