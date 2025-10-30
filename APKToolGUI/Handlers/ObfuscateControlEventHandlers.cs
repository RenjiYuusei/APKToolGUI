using APKToolGUI.Languages;
using APKToolGUI.Properties;
using Ookii.Dialogs.WinForms;
using System;
using System.IO;
using System.Windows.Forms;

namespace APKToolGUI.Handlers
{
    class ObfuscateControlEventHandlers
    {
        private static FormMain main;

        public ObfuscateControlEventHandlers(FormMain Main)
        {
            main = Main;

            main.button_OBF_BrowseInputFile.Click += button_OBF_BrowseInputFile_Click;
            main.button_OBF_BrowseOutputDir.Click += button_OBF_BrowseOutputDir_Click;
            main.button_OBF_BrowseRulesFile.Click += button_OBF_BrowseRulesFile_Click;
            main.button_OBF_Obfuscate.Click += button_OBF_Obfuscate_Click;
        }

        internal void button_OBF_BrowseInputFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Android packages|*.apk;*.aab;*.apks;*.apkm;*.zip";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    main.textBox_OBF_InputFile.Text = ofd.FileName;

                    if (String.IsNullOrEmpty(main.textBox_OBF_OutputDir.Text))
                        main.textBox_OBF_OutputDir.Text = Path.GetDirectoryName(ofd.FileName);
                }
            }
        }

        internal void button_OBF_BrowseOutputDir_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog();
            dlg.ShowNewFolderButton = true;

            if (dlg.ShowDialog() == DialogResult.OK)
                main.textBox_OBF_OutputDir.Text = dlg.SelectedPath;
        }

        internal void button_OBF_BrowseRulesFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Rules file|*.rules;*.txt|All files|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                    main.textBox_OBF_RulesFile.Text = ofd.FileName;
            }
        }

        internal async void button_OBF_Obfuscate_Click(object sender, EventArgs e)
        {
            try
            {
                main.Save();

                if (!File.Exists(Settings.Default.Obfuscate_InputFile))
                {
                    main.ShowMessage(Language.ObfuscateInputFileNotFound, MessageBoxIcon.Warning);
                    return;
                }

                if (!String.IsNullOrEmpty(Settings.Default.Obfuscate_RulesFile) && !File.Exists(Settings.Default.Obfuscate_RulesFile))
                {
                    main.ShowMessage(Language.ObfuscateRulesFileNotFound, MessageBoxIcon.Warning);
                    return;
                }

                await main.Obfuscate(Settings.Default.Obfuscate_InputFile);
            }
            catch (Exception ex)
            {
                main.ToLog(ApktoolEventType.Error, ex.Message);
            }
        }
    }
}
