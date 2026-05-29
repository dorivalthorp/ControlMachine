using System;
using System.Windows.Forms;
using ControlMachine.Data;
using ControlMachine.Forms;
using ControlMachine.Services;
using System.Linq;
using System.Text;
using Dapper;

namespace ControlMachine
{
    static class Program
    {
        public static Models.Usuario UsuarioLogado { get; set; }
        public static System.Drawing.Icon AppIcon { get; private set; }

        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0] == "--test")
            {
                RunAutomatedTests();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            
            DatabaseHelper.InitializeLocalDatabase();
            DatabaseHelper.InitializeRemoteDatabase();
            SyncService.Start();

            
            string pngPath = @"c:\01Job\ControlMachine\docs\robô.png";
            string icoPath = @"c:\01Job\ControlMachine\docs\robo.ico";
            try
            {
                if (!System.IO.File.Exists(icoPath) && System.IO.File.Exists(pngPath))
                {
                    using (var bmp = new System.Drawing.Bitmap(pngPath))
                    {
                        using (var fs = new System.IO.FileStream(icoPath, System.IO.FileMode.Create))
                        {
                            System.Drawing.Icon.FromHandle(bmp.GetHicon()).Save(fs);
                        }
                    }
                }
                if (System.IO.File.Exists(icoPath))
                {
                    AppIcon = new System.Drawing.Icon(icoPath);
                }
            }
            catch { }

            
            var splash = new FrmSplash();
            if (AppIcon != null) splash.Icon = AppIcon;
            splash.ShowDialog();

            
            var loginForm = new FrmLogin();
            if (AppIcon != null) loginForm.Icon = AppIcon;

            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                var principal = new FrmPrincipal();
                if (AppIcon != null) principal.Icon = AppIcon;
                Application.Run(principal);
            }

            SyncService.Stop();
        }

        private static void RunAutomatedTests()
        {
            var log = new StringBuilder();
            log.AppendLine("==================================================");
            log.AppendLine("  INICIANDO TESTES AUTOMATIZADOS - CONTROLMACHINE");
            log.AppendLine($"  Data/Hora: {DateTime.Now}");
            log.AppendLine("==================================================");

            int passed = 0;
            int failed = 0;

            
            try
            {
                string raw = "123";
                string hash = Helpers.SecurityHelper.GetSha256Hash(raw);
                if (hash == "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3")
                {
                    log.AppendLine("[PASS] Teste 1: Hash SHA-256 gerado corretamente.");
                    passed++;
                }
                else
                {
                    log.AppendLine("[FAIL] Teste 1: Hash SHA-256 incorreto.");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                log.AppendLine("[FAIL] Teste 1: Erro no hash: " + ex.Message);
                failed++;
            }

            
            try
            {
                DatabaseHelper.InitializeLocalDatabase();
                using (var conn = DatabaseHelper.GetLocalConnection())
                {
                    conn.Open();
                    var count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Usuarios");
                    log.AppendLine($"[PASS] Teste 2: Conexão SQLite estabelecida. Total usuários cadastrados: {count}");
                    passed++;
                }
            }
            catch (Exception ex)
            {
                log.AppendLine("[FAIL] Teste 2: Erro na conexão local: " + ex.Message);
                failed++;
            }

            
            try
            {
                string csvPath = @"c:\01Job\ControlMachine\docs\importar_brincos_exemplo.csv";
                if (System.IO.File.Exists(csvPath))
                {
                    string res = Helpers.CsvImporter.ImportBrincos(csvPath);
                    log.AppendLine("[PASS] Teste 3: Importação de brincos processada. Relatório:");
                    log.AppendLine(res);
                    passed++;
                }
                else
                {
                    log.AppendLine("[FAIL] Teste 3: Arquivo de brincos exemplo não encontrado.");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                log.AppendLine("[FAIL] Teste 3: Erro ao importar brincos: " + ex.Message);
                failed++;
            }

            
            try
            {
                string csvPath = @"c:\01Job\ControlMachine\docs\importar_pedidos_exemplo.csv";
                if (System.IO.File.Exists(csvPath))
                {
                    string res = Helpers.CsvImporter.ImportPedidos(csvPath);
                    log.AppendLine("[PASS] Teste 4: Importação de pedidos processada. Relatório:");
                    log.AppendLine(res);
                    passed++;
                }
                else
                {
                    log.AppendLine("[FAIL] Teste 4: Arquivo de pedidos exemplo não encontrado.");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                log.AppendLine("[FAIL] Teste 4: Erro ao importar pedidos: " + ex.Message);
                failed++;
            }

            
            try
            {
                string csvPath = @"c:\01Job\ControlMachine\docs\importar_usuarios_exemplo.csv";
                if (System.IO.File.Exists(csvPath))
                {
                    string res = Helpers.CsvImporter.ImportUsuarios(csvPath, "mudar123");
                    log.AppendLine("[PASS] Teste 5: Importação de usuários processada. Relatório:");
                    log.AppendLine(res);
                    passed++;
                }
                else
                {
                    log.AppendLine("[FAIL] Teste 5: Arquivo de usuários exemplo não encontrado.");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                log.AppendLine("[FAIL] Teste 5: Erro ao importar usuários: " + ex.Message);
                failed++;
            }

            log.AppendLine("==================================================");
            log.AppendLine($"  FIM DOS TESTES - PASSOU: {passed} | FALHOU: {failed}");
            log.AppendLine("==================================================");

            try
            {
                string logPath = @"c:\01Job\ControlMachine\docs\resultado_testes.txt";
                System.IO.File.WriteAllText(logPath, log.ToString());
            }
            catch { }
        }
    }
}
