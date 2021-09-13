using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace PDF_Merger
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

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            do
            {
                MergePdfs();

                if (System.Windows.Forms.MessageBox.Show("Merge more PDFs?", "Next", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                    System.Windows.Forms.DialogResult.Yes)
                {
                    Application.Current.Shutdown();
                    break;
                }

            } while (true);
        }

        private static void MergePdfs()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var dialogResult = dialog.ShowDialog();
                if (dialogResult == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    MergePdfsInFolder(dialog.SelectedPath);
                }
            }
        }

        private static void MergePdfsInFolder(string sourcePath)
        {
            var files = Directory.GetFiles(sourcePath, "*.pdf");

            if (!files.Any())
            {
                MessageBox.Show("No PDFs found in this folder", "No PDFs", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // Open the output document
            var outputDocument = new PdfDocument();

            foreach (string file in files.OrderBy(f => new FileInfo(f).Name))
            {
                PdfDocument inputDocument = null;
                try
                {
                    inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }

                int count = inputDocument.PageCount;
                for (int idx = 0; idx < count; idx++)
                {
                    var page = inputDocument.Pages[idx];
                    outputDocument.AddPage(page);
                }
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "PDF|*.pdf";
                dialog.Title = "Save Merged PDF";

                var sourceFolderInfo = new DirectoryInfo(sourcePath);

                dialog.FileName = $"{sourceFolderInfo.Name}_Merged";

                var dialogResult = dialog.ShowDialog();

                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    outputDocument.Save(dialog.FileName);
                }
            }
        }
    }
}
