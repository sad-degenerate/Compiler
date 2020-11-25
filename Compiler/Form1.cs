using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Compiler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            var filePath = @"D:\helloWorld.txt";
            var fileName = "test.exe";

            try
            {
                var langCode = new NewLangCode(filePath);

                MessageBox.Show(langCode.Code);

                var errors = langCode.Compile(fileName);

                if (string.IsNullOrWhiteSpace(errors))
                    Process.Start($"{Application.StartupPath}/{fileName}");
                else
                    MessageBox.Show(errors);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
