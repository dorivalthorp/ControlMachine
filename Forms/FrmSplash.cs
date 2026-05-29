using System;
using System.Drawing;
using System.Windows.Forms;

namespace ControlMachine.Forms
{
    public class FrmSplash : Form
    {
        private Timer timer;

        public FrmSplash()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(500, 360);
            this.BackColor = Color.FromArgb(24, 24, 27); 

            
            this.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(63, 63, 70), 2))
                {
                    e.Graphics.DrawRectangle(pen, 1, 1, this.Width - 2, this.Height - 2);
                }
            };

            try
            {
                string pngPath = @"c:\01Job\ControlMachine\docs\robô.png";
                if (System.IO.File.Exists(pngPath))
                {
                    var pb = new PictureBox
                    {
                        Image = Image.FromFile(pngPath),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Location = new Point(150, 30),
                        Size = new Size(200, 160)
                    };
                    this.Controls.Add(pb);
                }
            }
            catch { }

            var lblSystemName = new Label
            {
                Text = "ControlMachine",
                Location = new Point(0, 205),
                Width = 500,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.White
            };
            this.Controls.Add(lblSystemName);

            var lblSubtitle = new Label
            {
                Text = "SISTEMA DE PRODUÇÃO LASER",
                Location = new Point(0, 255),
                Width = 500,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(14, 165, 233) 
            };
            this.Controls.Add(lblSubtitle);

            var lblLoading = new Label
            {
                Text = "Carregando sistema...",
                Location = new Point(0, 310),
                Width = 500,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(161, 161, 170)
            };
            this.Controls.Add(lblLoading);

            timer = new Timer();
            timer.Interval = 3000; 
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Load += (s, e) => timer.Start();
        }
    }
}
