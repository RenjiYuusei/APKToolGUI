using APKToolGUI.Languages;
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

            if (main.button_OBFUSCATE_BrowseTool == null)
                return;

            main.button_OBFUSCATE_BrowseTool.Click += button_OBFUSCATE_BrowseTool_Click;
            main.button_OBFUSCATE_BrowseInputFile.Click += button_OBFUSCATE_BrowseInputFile_Click;
            main.button_OBFUSCATE_BrowseOutputFile.Click += button_OBFUSCATE_BrowseOutputFile_Click;
            main.button_OBFUSCATE_Run.Click += button_OBFUSCATE_Run_Click;
        }

        private void button_OBFUSCATE_BrowseTool_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "dpt-shell (*.jar;*.exe;*.bat)|*.jar;*.exe;*.bat|All files (*.*)|*.*";
                if (!String.IsNullOrWhiteSpace(main.textBox_OBFUSCATE_ToolPath.Text))
                {
                    try
                    {
                        string path = main.textBox_OBFUSCATE_ToolPath.Text;
                        if (File.Exists(path) || Directory.Exists(Path.GetDirectoryName(path)))
                        {
                            ofd.InitialDirectory = Path.GetDirectoryName(path);
                            ofd.FileName = Path.GetFileName(path);
                        }
                    }
                    catch
                    {
                    }
                }

                if (ofd.ShowDialog() == DialogResult.OK)
                    main.textBox_OBFUSCATE_ToolPath.Text = ofd.FileName;
            }
        }

        private void button_OBFUSCATE_BrowseInputFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Android Package (*.apk)|*.apk|Split Package (*.apks;*.xapk;*.apkm)|*.apks;*.xapk;*.apkm|All files (*.*)|*.*";
                if (!String.IsNullOrWhiteSpace(main.textBox_OBFUSCATE_InputFile.Text))
                {
                    try
                    {
                        string file = main.textBox_OBFUSCATE_InputFile.Text;
                        if (File.Exists(file))
                        {
                            ofd.InitialDirectory = Path.GetDirectoryName(file);
                            ofd.FileName = Path.GetFileName(file);
                        }
                    }
                    catch
                    {
                    }
                }

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    main.textBox_OBFUSCATE_InputFile.Text = ofd.FileName;
                    if (String.IsNullOrWhiteSpace(main.textBox_OBFUSCATE_OutputFile.Text))
                    {
                        string directory = Path.GetDirectoryName(ofd.FileName) ?? String.Empty;
                        string fileName = Path.GetFileNameWithoutExtension(ofd.FileName) + "_obfuscated.apk";
                        main.textBox_OBFUSCATE_OutputFile.Text = Path.Combine(directory, fileName);
                    }
                }
            }
        }

        private void button_OBFUSCATE_BrowseOutputFile_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Android Package (*.apk)|*.apk|All files (*.*)|*.*";
                if (!String.IsNullOrWhiteSpace(main.textBox_OBFUSCATE_OutputFile.Text))
                {
                    try
                    {
                        string file = main.textBox_OBFUSCATE_OutputFile.Text;
                        if (!String.IsNullOrWhiteSpace(Path.GetDirectoryName(file)))
                            sfd.InitialDirectory = Path.GetDirectoryName(file);
                        sfd.FileName = Path.GetFileName(file);
                    }
                    catch
                    {
                    }
                }
                else if (!String.IsNullOrWhiteSpace(main.textBox_OBFUSCATE_InputFile.Text))
                {
                    string directory = Path.GetDirectoryName(main.textBox_OBFUSCATE_InputFile.Text) ?? String.Empty;
                    string fileName = Path.GetFileNameWithoutExtension(main.textBox_OBFUSCATE_InputFile.Text) + "_obfuscated.apk";
                    sfd.InitialDirectory = directory;
                    sfd.FileName = fileName;
                }

                if (sfd.ShowDialog() == DialogResult.OK)
                    main.textBox_OBFUSCATE_OutputFile.Text = sfd.FileName;
            }
        }

        private async void button_OBFUSCATE_Run_Click(object sender, EventArgs e)
        {
            string inputFile = main.textBox_OBFUSCATE_InputFile.Text;
            string outputFile = main.textBox_OBFUSCATE_OutputFile.Text;

            if (!File.Exists(inputFile))
            {
                MessageBox.Show(Language.WarningFileForObfuscationNotSelected, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await main.Obfuscate(inputFile, outputFile);
        }
    }
}
