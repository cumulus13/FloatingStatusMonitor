using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using LibreHardwareMonitor.Hardware;

using Timer = System.Windows.Forms.Timer;

namespace FloatingStatusMonitor
{
    public class Config
    {
        public string FontName { get; set; } = "Consolas";
        public float FontSize { get; set; } = 10;
        public string TextColor { get; set; } = "#FFFFFF";
        public string BackColor { get; set; } = "#000000";
        public double Opacity { get; set; } = 0.8;
        public int WindowWidth { get; set; } = 220;
        public int WindowHeight { get; set; } = 100;
        public int WindowX { get; set; } = 20;
        public int WindowY { get; set; } = 20;
        
        // CPU Color Settings
        public string CpuHighBgColor { get; set; } = "#0000FF"; // Blue for >95-99%
        public string CpuCriticalBgColor { get; set; } = "#FF0000"; // Red for >=99%
        public string CpuHighTextColor { get; set; } = "#FFFFFF"; // White
        public string CpuCriticalTextColor { get; set; } = "#FFFFFF"; // White
        
        // RAM/TEMP Color Settings
        public string RamTempColor0 { get; set; } = "#00FF00"; // Green for >=0%
        public string RamTempColor30 { get; set; } = "#00FFFF"; // Cyan for >=30%
        public string RamTempColor50 { get; set; } = "#FFA500"; // Orange for >=50%
        public string RamTempColor70 { get; set; } = "#FFFF00"; // Yellow for >=70%
        public string RamTempColor80 { get; set; } = "#FFFF00"; // Yellow for >=80%
        public string RamTempColor90 { get; set; } = "#FF00FF"; // Magenta for >=90%
        public string RamTempColor98 { get; set; } = "#FFFFFF"; // White for >=98%
        public string RamTempColor99 { get; set; } = "#FFFFFF"; // White for >=99%
        public string RamTempBgColor98 { get; set; } = "#0000FF"; // Blue bg for >=98%
        public string RamTempBgColor99 { get; set; } = "#FF0000"; // Red bg for >=99%
    }

    public class Form1 : Form
    {
        [DllImport("user32.dll")] public static extern bool ReleaseCapture();
        [DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private Label cpuLabel, ram1Label, ram2Label, tempLabel;
        private Timer updateTimer;
        private Computer computer;
        private Config config;
        private FileSystemWatcher watcher;
        private NotifyIcon trayIcon;
        // private const string ConfigPath = "config.json";
        private static string ConfigPath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            "config.json"
        );


        public Form1()
        {
            this.Load += (s, e) => ApplyConfig();

            config = LoadConfigFromDisk();

            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.BackColor = Color.Black;
            this.TransparencyKey = Color.Magenta;
            this.MouseDown += Form1_MouseDown;
            this.KeyDown += Form1_KeyDown;

            CreateLabels();

            computer = new Computer { IsCpuEnabled = true };
            computer.Open();

            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "Floating Status Monitor"
            };
            var menu = new ContextMenuStrip();
            menu.Items.Add("Save Geometry", null, (s, e) => SaveCurrentGeometry());
            menu.Items.Add("Exit", null, (s, e) => 
            {
                SaveCurrentGeometry();
                Application.Exit();
            });
            trayIcon.ContextMenuStrip = menu;

            updateTimer = new Timer { Interval = 1000 };
            updateTimer.Tick += UpdateStats;
            updateTimer.Start();

            watcher = new FileSystemWatcher(".", ConfigPath)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            watcher.Changed += (s, e) =>
            {
                Task.Delay(300).ContinueWith(_ =>
                {
                    try
                    {
                        config = LoadConfigFromDisk();
                        ApplyConfig();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Reload config error: " + ex.Message);
                    }
                });
            };
        }

        private void CreateLabels()
        {
            int lineHeight = 20;
            
            cpuLabel = new Label
            {
                AutoSize = false,
                Size = new Size(200, lineHeight),
                Location = new Point(5, 5),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            cpuLabel.MouseDown += Form1_MouseDown;
            this.Controls.Add(cpuLabel);

            ram1Label = new Label
            {
                AutoSize = false,
                Size = new Size(200, lineHeight),
                Location = new Point(5, 25),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            ram1Label.MouseDown += Form1_MouseDown;
            this.Controls.Add(ram1Label);

            ram2Label = new Label
            {
                AutoSize = false,
                Size = new Size(200, lineHeight),
                Location = new Point(5, 45),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            ram2Label.MouseDown += Form1_MouseDown;
            this.Controls.Add(ram2Label);

            tempLabel = new Label
            {
                AutoSize = false,
                Size = new Size(200, lineHeight),
                Location = new Point(5, 65),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            tempLabel.MouseDown += Form1_MouseDown;
            this.Controls.Add(tempLabel);
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Q)
            {
                SaveCurrentGeometry();
                Application.Exit();
            }
            else if (e.KeyCode == Keys.S)
            {
                SaveCurrentGeometry();
            }
        }

        private Config LoadConfigFromDisk()
        {
            if (File.Exists(ConfigPath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<Config>(json) ?? new Config();
                }
                catch { }
            }
            return new Config();
        }

        private void ApplyConfig()
        {
            if (this.IsHandleCreated)
            {
                this.Invoke((MethodInvoker)(() => ApplyConfigValues()));
            }
            else
            {
                ApplyConfigValues();
            }
        }

        private void ApplyConfigValues()
        {
            var font = new Font(config.FontName, config.FontSize);
            this.Font = font;
            this.Opacity = config.Opacity;
            this.Size = new Size(config.WindowWidth, config.WindowHeight);
            this.Location = new Point(config.WindowX, config.WindowY);
            this.BackColor = ColorTranslator.FromHtml(config.BackColor);
            
            if (cpuLabel != null) cpuLabel.Font = font;
            if (ram1Label != null) ram1Label.Font = font;
            if (ram2Label != null) ram2Label.Font = font;
            if (tempLabel != null) tempLabel.Font = font;
        }

        private Color GetColorForRamOrTemp(float value)
        {
            if (value >= 99) return ColorTranslator.FromHtml(config.RamTempColor99);
            if (value >= 98) return ColorTranslator.FromHtml(config.RamTempColor98);
            if (value >= 90) return ColorTranslator.FromHtml(config.RamTempColor90);
            if (value >= 80) return ColorTranslator.FromHtml(config.RamTempColor80);
            if (value >= 70) return ColorTranslator.FromHtml(config.RamTempColor70);
            if (value >= 50) return ColorTranslator.FromHtml(config.RamTempColor50);
            if (value >= 30) return ColorTranslator.FromHtml(config.RamTempColor30);
            return ColorTranslator.FromHtml(config.RamTempColor0);
        }

        private Color GetBackgroundForRamOrTemp(float value)
        {
            if (value >= 99) return ColorTranslator.FromHtml(config.RamTempBgColor99);
            if (value >= 98) return ColorTranslator.FromHtml(config.RamTempBgColor98);
            return Color.Transparent;
        }

        private void UpdateStats(object? sender, EventArgs e)
        {
            float cpu = GetCpuUsage();
            float ram_counter = GetMemoryUsage();
            float ram = GetRamViaGlobalMemoryStatusEx();
            float temp = GetCpuTemperature();

            // CPU color logic - affects entire form background
            Color formBackgroundColor = ColorTranslator.FromHtml(config.BackColor);
            Color defaultTextColor = ColorTranslator.FromHtml(config.TextColor);
            
            if (cpu >= 99)
            {
                formBackgroundColor = ColorTranslator.FromHtml(config.CpuCriticalBgColor);
                defaultTextColor = ColorTranslator.FromHtml(config.CpuCriticalTextColor);
            }
            else if (cpu > 95 && cpu <= 99)
            {
                formBackgroundColor = ColorTranslator.FromHtml(config.CpuHighBgColor);
                defaultTextColor = ColorTranslator.FromHtml(config.CpuHighTextColor);
            }

            this.BackColor = formBackgroundColor;
            
            // Update CPU label
            cpuLabel.Text = $"CPU  : {cpu:F1}%";
            cpuLabel.ForeColor = defaultTextColor;
            cpuLabel.BackColor = Color.Transparent;
            
            // // Update RAM 1 label with individual coloring
            // ram1Label.Text = $"RAM 1: {ram:F1}%";
            // ram1Label.ForeColor = GetColorForRamOrTemp(ram);
            // ram1Label.BackColor = GetBackgroundForRamOrTemp(ram);
            
            // // Update RAM 2 label with individual coloring
            // ram2Label.Text = $"RAM 2: {ram_counter:F1}%";
            // ram2Label.ForeColor = GetColorForRamOrTemp(ram_counter);
            // ram2Label.BackColor = GetBackgroundForRamOrTemp(ram_counter);
            
            // // Update TEMP label with individual coloring
            // tempLabel.Text = $"TEMP : {temp:F1}°C";
            // tempLabel.ForeColor = GetColorForRamOrTemp(temp);
            // tempLabel.BackColor = GetBackgroundForRamOrTemp(temp);

            bool cpuInHighState = cpu >= 99 || (cpu > 95 && cpu <= 99);

            // Update RAM 1 label
            ram1Label.Text = $"RAM 1: {ram:F1}%";
            ram1Label.ForeColor = cpuInHighState ? defaultTextColor : GetColorForRamOrTemp(ram);
            ram1Label.BackColor = cpuInHighState ? Color.Transparent : GetBackgroundForRamOrTemp(ram);

            // Update RAM 2 label
            ram2Label.Text = $"RAM 2: {ram_counter:F1}%";
            ram2Label.ForeColor = cpuInHighState ? defaultTextColor : GetColorForRamOrTemp(ram_counter);
            ram2Label.BackColor = cpuInHighState ? Color.Transparent : GetBackgroundForRamOrTemp(ram_counter);

            // Update TEMP label
            tempLabel.Text = $"TEMP : {temp:F1}°C";
            tempLabel.ForeColor = cpuInHighState ? defaultTextColor : GetColorForRamOrTemp(temp);
            tempLabel.BackColor = cpuInHighState ? Color.Transparent : GetBackgroundForRamOrTemp(temp);


            trayIcon.Text = $"CPU {cpu:F1}% | RAM {ram:F1}% | TEMP {temp:F1}°C";
        }

        private float GetRamViaGlobalMemoryStatusEx()
        {
            try
            {
                MEMORYSTATUSEX mem = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(mem))
                {
                    float used = (float)(mem.ullTotalPhys - mem.ullAvailPhys) / mem.ullTotalPhys * 100;
                    return used;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GlobalMemoryStatusEx Error: " + ex.Message);
            }
            return 0;
        }

        private float GetCpuUsage()
        {
            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();
            System.Threading.Thread.Sleep(250);
            return cpuCounter.NextValue();
        }

        private float GetMemoryUsage()
        {
            using var memCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            return memCounter.NextValue();
        }

        private float GetCpuTemperature()
        {
            foreach (var hw in computer.Hardware)
            {
                if (hw.HardwareType == HardwareType.Cpu)
                {
                    hw.Update();
                    foreach (var sensor in hw.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                            return sensor.Value.Value;
                    }
                }
            }
            return 0;
        }

        private void SaveCurrentGeometry()
        {
            try
            {
                config.WindowX = this.Location.X;
                config.WindowY = this.Location.Y;
                config.WindowWidth = this.Size.Width;
                config.WindowHeight = this.Size.Height;
                
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
                
                Debug.WriteLine($"Geometry saved: X={config.WindowX}, Y={config.WindowY}, W={config.WindowWidth}, H={config.WindowHeight}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Save geometry error: " + ex.Message);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            SaveCurrentGeometry();
            trayIcon.Dispose();
            base.OnFormClosed(e);
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
    }
}