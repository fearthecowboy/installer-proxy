//-----------------------------------------------------------------------
// <copyright company="CoApp Project">
//     Copyright (c) 2010 Garrett Serack. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace coapp_stub {
    using System;
    using System.Windows.Forms;
    using System.IO;
    using Microsoft.Win32;

    public partial class StubWindow :Form {

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StubWindow());
        }

        public StubWindow() {
            InitializeComponent();
            textBox1.Text = CoAppInstallerPath;
            button2.Enabled = false;
        }

        private const string CoAppRegRoot = @"Software\CoApp";
        private const string CoAppRegRoot2 = @"Software\Wow6432Node\CoApp";

        private const string CoAppInstallerKey= @"CoAppInstaller";

        private static string CoAppInstallerPath {
            get {
                RegistryKey regkey = null;
                string result = null;

                try {
                    regkey = Registry.LocalMachine.CreateSubKey(CoAppRegRoot);

                    if(null != regkey)
                        result = regkey.GetValue(CoAppInstallerKey, null) as string;
                }
                catch {
                }
                finally {
                    if(null != regkey)
                        regkey.Close();
                }

                if(string.IsNullOrEmpty(result) && IntPtr.Size == 8) {
                    try { // x64 platform, check if the x86 key is set.
                        regkey = Registry.LocalMachine.CreateSubKey(CoAppRegRoot2);

                        if(null != regkey) {
                            result = regkey.GetValue(CoAppInstallerKey, null) as string;
                            CoAppInstallerPath = result; // make sure both copies are to this value.
                        }
                    }
                    catch {
                    }
                    finally {
                        if(null != regkey)
                            regkey.Close();
                    }
                    
                }

                return result;
            }

            set {
                RegistryKey regkey = null;
                try {
                    regkey = Registry.LocalMachine.CreateSubKey(CoAppRegRoot);

                    if(null == regkey)
                        return;

                    regkey.SetValue(CoAppInstallerKey, value);
                }
                catch {
                }
                finally {
                    if(null != regkey)
                        regkey.Close();
                }

                
                if(IntPtr.Size == 8) { // x64 platform, set the x86 key as well.
                    try {
                        regkey = Registry.LocalMachine.CreateSubKey(CoAppRegRoot2);

                        if(null == regkey)
                            return;

                        regkey.SetValue(CoAppInstallerKey, value);
                    }
                    catch {
                    }
                    finally {
                        if(null != regkey)
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

            if(File.Exists(openFileDialog1.FileName))
                textBox1.Text = openFileDialog1.FileName;

        }

        private void button2_Click(object sender, EventArgs e) {
            textBox1.Text = textBox1.Text.Trim();

            if(!string.IsNullOrEmpty(textBox1.Text) && !File.Exists(textBox1.Text)) {
                if(MessageBox.Show("Target program [" + textBox1.Text + "] does not exist. Save Anyway?","Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
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

