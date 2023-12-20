using Microsoft.Win32;
using NokiaIBConverter;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace NokiaIBConverterApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSelectDestination_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    txtTargetFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnSelectSource_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".bi",
                Filter = "Nokia IB File (.ib)|*.ib"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                txtSourceFile.Text = openFileDialog.FileName;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate())
            {
                MessageBox.Show("אחד או יותר מהשדות ריקים או לא נכונים", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            StartConversion();

        }

        private void StartConversion()
        {
            var writerType = cmbFormatType.SelectedItem == cmbVcfType ? WriterType.VCF : WriterType.CSV;
            var outpuType = cmbOutputType.SelectedItem == cmbOneFile ? OutputType.Single : OutputType.Multi;
            var factory = new WriterFactory();

            IWriter writer = outpuType == OutputType.Single ?
                factory.CreateSingleFileWriter(writerType, txtTargetFolder.Text, "contacts") :
                factory.CreateMultiFileWriter(writerType, txtTargetFolder.Text);

            var converter = new Converter(writer, txtSourceFile.Text);
            try
            {
                var resultsCount = converter.Convert();
                var messageStr = $"הסתיים בהצלחה.{resultsCount} רשומות הומרו";
                MessageBox.Show(messageStr, "הודעה", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                var errorStr = $"ארעה שגיאה בעיבוד הקובץ. פרטי שגיאה : {e.Message}";
                MessageBox.Show(errorStr, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            
        }

        private bool Validate()
        {
            return
                !string.IsNullOrEmpty(txtSourceFile.Text) &&
                !string.IsNullOrEmpty(txtTargetFolder.Text) &&
                File.Exists(txtSourceFile.Text) &&
                Directory.Exists(txtTargetFolder.Text);
        }
    }
}
