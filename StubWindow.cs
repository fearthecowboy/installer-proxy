//-----------------------------------------------------------------------
// <copyright company="CoApp Project">
//     Copyright (c) 2010 Garrett Serack. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CoApp.Installer {
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Text;
    using System.Windows.Forms;
    using Microsoft.Win32;

    public partial class StubWindow : Form {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint SearchPath(string lpPath, string lpFileName, string lpExtension, int nBufferLength,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpBuffer, out IntPtr lpFilePart);

        private static string FindInPath(string filename) {
            try {
                var s = new StringBuilder(260); // MAX_PATH
                var p = new IntPtr();

                SearchPath(null, filename, null, s.Capacity, s, out p);

                return s.ToString();
            }
            catch {
            }

            return string.Empty;
        }

        [STAThread]
        private static void Main(string[] args) {
            if (args.Length > 0 ) {
                if( args.First() == "update" ) {
                    // try to run coapp-update first. 
                    // 
                }

                var msiPath = string.Join(" ", args);
                msiPath = Path.GetFullPath(msiPath);

                if (msiPath.EndsWith(".msi", StringComparison.CurrentCultureIgnoreCase) && File.Exists(msiPath)) {
                    
                    var path = SearchForCoApp();
                    if (File.Exists(path)) {
                        RunInstaller(path, msiPath);
                        return;
                    }
                    MessageBox.Show("You've attempted to install a CoApp package, but the actual package installer can not be located.  You will have to select the EXE to run for CoApp packages and retry the installation.", "CoApp Installer", MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                }
            }

            // didn't work (or ran in interactive mode). Show Dialog.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StubWindow());
        }

        public static string SearchForCoApp() {
            var thisAssemblyVersion = Version(Assembly.GetEntryAssembly().Location);

            var path = CoAppInstallerPath;
            if (!string.IsNullOrEmpty(path) && File.Exists(path)) {
                return path;
            }

            // try to find coapp.exe as the installer 

            // search PATH
            // search in c:\apps\bin\...
            // search in c:\apps\.installed\outercurve foundation\...
            try {
                path = FindInPath("coapp.exe");
                if (!string.IsNullOrEmpty(path) && File.Exists(path) && Version(path) >= thisAssemblyVersion) {
                    // remember this location, it was found in a good spot.
                    CoAppInstallerPath = path;
                    return path;
                }
            }
            catch {
            }

            try {
                var possibilities = Directory.EnumerateFiles(@"c:\apps\bin", "coapp.exe", SearchOption.AllDirectories);
                foreach (var each in possibilities.Where(each => Version(each) >= thisAssemblyVersion)) {
                    // remember this location, it was found in a good spot.
                    CoAppInstallerPath = each;
                    return each;
                }
            }
            catch {
            }

            try {
                var possibilities = Directory.EnumerateFiles(@"c:\apps\.installed\Outercurve Foundation\", "coapp.exe",
                    SearchOption.AllDirectories);
                foreach (var each in possibilities.Where(each => Version(each) >= thisAssemblyVersion)) {
                    return each;
                }
            }
            catch {
            }

            return string.Empty;
        }

        public static int PositionOfFirstCharacterNotIn(string str, char[] characters) {
            var p = 0;
            while (p < str.Length) {
                if (!characters.Contains(str[p])) {
                    return p;
                }
                p++;
            }
            return p;
        }

        public static ulong Version(string path) {
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
            string fv = info.FileVersion;
            if (!string.IsNullOrEmpty(fv)) {
                fv = fv.Substring(0, PositionOfFirstCharacterNotIn(fv, "0123456789.".ToCharArray()));
            }

            if (string.IsNullOrEmpty(fv)) {
                return 0;
            }

            var vers = fv.Split('.');
            var major = vers.Length > 0 ? ToInt32(vers[0], 0) : 0;
            var minor = vers.Length > 1 ? ToInt32(vers[1], 0) : 0;
            var build = vers.Length > 2 ? ToInt32(vers[2], 0) : 0;
            var revision = vers.Length > 3 ? ToInt32(vers[3], 0) : 0;

            return (((UInt64) major) << 48) + (((UInt64) minor) << 32) + (((UInt64) build) << 16) + (UInt64) revision;
        }

        public static int ToInt32(string str, int defaultValue) {
            var i = defaultValue;
            Int32.TryParse(str, out i);
            return i;
        }

        public static bool IsAdmin {
            get {
                return new WindowsPrincipal( WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static void RunInstaller(string path, string msi) {
            var processInfo = new ProcessStartInfo {
                FileName = path,
                Arguments = "\"" + msi + "\""
            };

            if( !IsAdmin ) {
                processInfo.Verb = "runas";
            }

            Process.Start(processInfo);
        }

        public StubWindow() {
            InitializeComponent();
            textBox1.Text = CoAppInstallerPath;
            button2.Enabled = false;
        }

        private const string CoAppRegRoot = @"Software\CoApp";
        private const string CoAppInstallerKey = @"Installer";

        private static string CoAppInstallerPath {
            get {
                RegistryKey regkey = null;
                string result = null;

                try {
                    regkey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64).CreateSubKey(CoAppRegRoot);

                    if (null != regkey) {
                        result = regkey.GetValue(CoAppInstallerKey, null) as string;
                    }
                }
                catch {
                }
                finally {
                    if (null != regkey) {
                        regkey.Close();
                    }
                }

                return result;
            }

            set {
                RegistryKey regkey = null;
                try {
                    regkey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64).CreateSubKey(CoAppRegRoot);

                    if (null == regkey) {
                        return;
                    }

                    regkey.SetValue(CoAppInstallerKey, value);
                }
                catch {
                }
                finally {
                    if (null != regkey) {
                        regkey.Close();
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.AutoUpgradeEnabled = true;
            openFileDialog1.Filter = "Program files (*.exe)|*.exe";
            openFileDialog1.Title = "Select Target EXE";
            openFileDialog1.DefaultExt = ".exe";
            openFileDialog1.ShowDialog();

            if (File.Exists(openFileDialog1.FileName)) {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            textBox1.Text = textBox1.Text.Trim();

            if (!string.IsNullOrEmpty(textBox1.Text) && !File.Exists(textBox1.Text)) {
                if (
                    MessageBox.Show("Target program [" + textBox1.Text + "] does not exist. Save Anyway?", "Warning",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) {
                    return;
                }
            }
            CoAppInstallerPath = textBox1.Text;
            button2.Enabled = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            button2.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e) {
            textBox1.Text = CoAppInstallerPath;
            button2.Enabled = false;
        }
    }
}