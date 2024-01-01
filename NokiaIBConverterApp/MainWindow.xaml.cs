using NokiaIBConverter;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using MessageBoxOptions = System.Windows.MessageBoxOptions;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace NokiaIBConverterApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string SettingsFileName = "NokiaBIConverterSettings.config";
        private const char SettingsFileNameSeperator = ';';

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                var settingsFilePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + SettingsFileName;
                var settingsStr = File.Exists(settingsFilePath) ? File.ReadAllText(settingsFilePath).Split(SettingsFileNameSeperator) : null;

                if (settingsStr != null && settingsStr.Any())
                {
                    txtSourceFile.Text = settingsStr[0];
                    txtTargetFolder.Text = settingsStr[1];
                    cmbFormatType.SelectedIndex = Convert.ToInt32(settingsStr[2]);
                    cmbOutputType.SelectedIndex = Convert.ToInt32(settingsStr[3]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ארעה שגיאה. {ex}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settingsFilePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + SettingsFileName;

                var settingsStr = 
                    txtSourceFile.Text + SettingsFileNameSeperator + 
                    txtTargetFolder.Text + SettingsFileNameSeperator +
                    cmbFormatType.SelectedIndex.ToString() + SettingsFileNameSeperator +
                    cmbOutputType.SelectedIndex.ToString();

                if (File.Exists(settingsFilePath))
                {
                    File.Delete(settingsFilePath);
                }

                File.WriteAllText(settingsFilePath, settingsStr);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ארעה שגיאה. {ex}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
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

            SaveSettings();
            StartConversion();

        }

        private void StartConversion()
        {

            var writerType = cmbFormatType.SelectedItem == cmbVcfType ? WriterType.VCF : WriterType.CSV;
            var outpuType = cmbOutputType.SelectedItem == cmbOneFile ? OutputType.Single : OutputType.Multi;
            
            var factory = new WriterFactory();
            var targetFolderUniqueName = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss", new CultureInfo("en-US"));
            var targetFolderName = txtTargetFolder.Text + Path.DirectorySeparatorChar + targetFolderUniqueName;
            try
            {
                IWriter writer = outpuType == OutputType.Single ?
                    factory.CreateSingleFileWriter(writerType, targetFolderName, "contacts") :
                    factory.CreateMultiFileWriter(writerType, targetFolderName);

                var converter = new Converter(writer, txtSourceFile.Text);

                var resultsCount = converter.Convert();
                var messageStr = $"הסתיים בהצלחה.{resultsCount} רשומות הומרו";
                MessageBox.Show(messageStr, "הודעה", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                var errorStr = $"ארעה שגיאה בעיבוד הקובץ:\n" +
                                $"נתיב הקובץ:{targetFolderName}\n" +
                                $"פרטי שגיאה:{e}";

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

        private void mnuABout_Click(object sender, RoutedEventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var message = $"ממיר אנשי קשר נוקיה. גרסה {version.Major}.{version.Minor}.{version.Build}\n" +
                          $"השימוש בתוכנה באחריות המשתמש בלבד ואין אחריות על תקינות הנתונים.\n" +
                          $"התוכנה נכתבה לזיכוי הרבים ואין לקחת עליה או על השימוש בה תשלום.";

            MessageBox.Show(message, "אודות", MessageBoxButton.OK, MessageBoxImage.Information,MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }
    }
}
