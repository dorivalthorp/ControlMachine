using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using Dapper;
using ControlMachine.Models;
using ControlMachine.Data;

namespace ControlMachine.Forms
{
    public class FrmDashboard : Form
    {
        private Panel pnlChartStatus;
        private Panel pnlChartUsuarios;
        private Panel pnlChartMotivos;
        private List<Producao> producoes;
        private List<MaquinaLaser> maquinas;
        private List<Usuario> usuarios;
        private List<Brinco> brincos;

        public FrmDashboard()
        {
            this.Text = "Dashboard e Indicadores";
            this.Size = new Size(1100, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            
            var lblTitle = new Label { Text = "Indicadores de Produção", Font = new Font("Arial", 16, FontStyle.Bold), Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleCenter };
            this.Controls.Add(lblTitle);

            
            var pnlContainer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            pnlContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnlContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnlContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            this.Controls.Add(pnlContainer);

            
            var gbStatus = new GroupBox { Text = "Qtd de Pedidos por Status", Dock = DockStyle.Fill, Margin = new Padding(10) };
            pnlChartStatus = new Panel { Dock = DockStyle.Fill };
            pnlChartStatus.Paint += PnlChartStatus_Paint;
            gbStatus.Controls.Add(pnlChartStatus);
            pnlContainer.Controls.Add(gbStatus, 0, 0);

            
            var gbUsuarios = new GroupBox { Text = "Qtd de Pedidos por Usuário", Dock = DockStyle.Fill, Margin = new Padding(10) };
            pnlChartUsuarios = new Panel { Dock = DockStyle.Fill };
            pnlChartUsuarios.Paint += PnlChartUsuarios_Paint;
            gbUsuarios.Controls.Add(pnlChartUsuarios);
            pnlContainer.Controls.Add(gbUsuarios, 1, 0);

            
            var gbMotivos = new GroupBox { Text = "Regravações por Motivo", Dock = DockStyle.Fill, Margin = new Padding(10) };
            pnlChartMotivos = new Panel { Dock = DockStyle.Fill };
            pnlChartMotivos.Paint += PnlChartMotivos_Paint;
            gbMotivos.Controls.Add(pnlChartMotivos);
            pnlContainer.Controls.Add(gbMotivos, 2, 0);

            this.Load += (s, e) => CarregarDados();
        }

        private void CarregarDados()
        {
            try {
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    producoes = conn.Query<Producao>("SELECT * FROM Producoes").ToList();
                    maquinas = conn.Query<MaquinaLaser>("SELECT * FROM MaquinasLaser").ToList();
                    usuarios = conn.Query<Usuario>("SELECT * FROM Usuarios").ToList();
                    brincos = conn.Query<Brinco>("SELECT * FROM Brincos WHERE MotivoRegravacao IS NOT NULL AND MotivoRegravacao <> ''").ToList();
                }
                pnlChartStatus.Invalidate();
                pnlChartUsuarios.Invalidate();
                pnlChartMotivos.Invalidate();
            } catch { }
        }

        private void PnlChartStatus_Paint(object sender, PaintEventArgs e)
        {
            if (producoes == null) return;

            var agrupado = producoes.GroupBy(p => p.Status)
                                    .Select(g => new { Status = g.Key, Total = g.Count() })
                                    .ToList();

            DesenharGraficoBarras(e.Graphics, pnlChartStatus.Width, pnlChartStatus.Height, agrupado.Select(x => x.Status).ToList(), agrupado.Select(x => x.Total).ToList(), Color.SteelBlue);
        }

        private void PnlChartUsuarios_Paint(object sender, PaintEventArgs e)
        {
            if (producoes == null || usuarios == null) return;

            var agrupado = producoes.GroupBy(p => p.UsuarioId)
                                    .Select(g => new { Usuario = usuarios.FirstOrDefault(u => u.Id == g.Key)?.Nome ?? "Desconhecido", Total = g.Count() })
                                    .ToList();

            DesenharGraficoBarras(e.Graphics, pnlChartUsuarios.Width, pnlChartUsuarios.Height, agrupado.Select(x => x.Usuario).ToList(), agrupado.Select(x => x.Total).ToList(), Color.SeaGreen);
        }

        private void PnlChartMotivos_Paint(object sender, PaintEventArgs e)
        {
            if (brincos == null) return;

            var agrupado = brincos.GroupBy(b => b.MotivoRegravacao)
                                  .Select(g => new { Motivo = g.Key, Total = g.Count() })
                                  .ToList();

            DesenharGraficoBarras(e.Graphics, pnlChartMotivos.Width, pnlChartMotivos.Height, agrupado.Select(x => x.Motivo).ToList(), agrupado.Select(x => x.Total).ToList(), Color.Tomato);
        }

        private void DesenharGraficoBarras(Graphics g, int width, int height, List<string> labels, List<int> values, Color color)
        {
            g.Clear(Color.White);
            if (values.Count == 0)
            {
                g.DrawString("Sem dados", new Font("Arial", 12), Brushes.Gray, 10, 10);
                return;
            }

            int maxVal = values.Max();
            if (maxVal == 0) maxVal = 1; 

            int barWidth = (width / values.Count) - 20;
            if (barWidth < 20) barWidth = 20;
            if (barWidth > 100) barWidth = 100;

            int startX = 20;
            int bottomY = height - 40;
            double scale = (double)(height - 80) / maxVal;

            for (int i = 0; i < values.Count; i++)
            {
                int barHeight = (int)(values[i] * scale);
                Rectangle rect = new Rectangle(startX, bottomY - barHeight, barWidth, barHeight);
                
                using (Brush b = new SolidBrush(color))
                {
                    g.FillRectangle(b, rect);
                }
                g.DrawRectangle(Pens.Black, rect);

                
                g.DrawString(values[i].ToString(), new Font("Arial", 10, FontStyle.Bold), Brushes.Black, startX + (barWidth/2) - 10, bottomY - barHeight - 20);

                
                string label = labels[i].Length > 10 ? labels[i].Substring(0, 10) + "..." : labels[i];
                g.DrawString(label, new Font("Arial", 8), Brushes.Black, startX, bottomY + 5);

                startX += barWidth + 20;
            }
        }
    }
}
