namespace PDFMergeDesktop
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Threading.Tasks;

    using iTextSharp.text;
    using iTextSharp.text.pdf;
    
    /// <summary>
    ///  Merges a given collection of PDF files.
    /// </summary>
    public class PdfMerger : INotifyPropertyChanged
    {
        /// <summary>
        ///  The path to store the merged PDF file.
        /// </summary>
        private string outputPath;

        /// <summary>
        ///  Collection of paths to PDF files to merge.
        /// </summary>
        private ObservableCollection<StringItem> inputPaths = new ObservableCollection<StringItem>();

        /// <summary>
        ///  The writer object for this task.
        /// </summary>
        private PdfWriter writer = null;

        /// <summary>
        ///  The output document for this task.
        /// </summary>
        private Document document = null;

        /// <summary>
        ///  The number of files to process.
        /// </summary>
        private int fileCount = 0;

        /// <summary>
        ///  The number of files already merged.
        /// </summary>
        private int completedCount = 0;

        /// <summary>
        ///  Reporter for progress updates.
        /// </summary>
        private IProgress<int> progress;

        /// <summary>
        ///  Raised when bind-able properties change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        ///  Gets or sets the path to store the output file.
        /// </summary>
        public string OutputPath
        {
            get
            {
                return outputPath;
            }

            set
            {
                if (!StringComparer.Ordinal.Equals(outputPath, value))
                {
                    outputPath = value;
                    OnPropertyChanged("OutputPath");
                }
            }
        }

        /// <summary>
        ///  Gets a collection of paths to PDF files to merge.
        /// </summary>
        public ObservableCollection<StringItem> InputPaths
        {
            get
            {
                return inputPaths;
            }
        }

        /// <summary>
        ///  Gets or sets the report target for progress updates.
        /// </summary>
        public IProgress<int> Progress
        {
            get
            {
                return progress;
            }

            set
            {
                if (progress != value)
                {
                    progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }

        /// <summary>
        ///  Gets a value indicating whether a merge can be performed.
        /// </summary>
        public bool CanMerge
        {
            get
            {
                try
                {
                    outputPath = Path.GetFullPath(outputPath);
                }
                catch
                {
                    return false;
                }

                return inputPaths.Count > 1;
            }
        }

        /// <summary>
        ///  Merge the given files and store the output in the given destination.
        /// </summary>
        /// <returns>An await-able task.</returns>
        public async Task MergeAsync()
        {
            // Only one merge at a time, please.
            if (document != null || writer != null)
            {
                return;
            }

            await CreateDocument();
            await MergeFiles();
            CloseDocument();
        }

        /// <summary>
        ///  Raise the PropertyChanged event.
        /// </summary>
        /// <param name="name">The name of the changed property.</param>
        private void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        ///  Create the output document.
        /// </summary>
        /// <returns>An await-able task.</returns>
        private async Task CreateDocument()
        {
             await Task.Run(() =>
                {
                    var outputStream = new FileStream(outputPath, FileMode.Create);
                    document = new Document();
                    writer = PdfWriter.GetInstance(document, outputStream);
                    document.Open();
                });
        }

        /// <summary>
        ///  Merge the input files.
        /// </summary>
        /// <returns>An await-able task.</returns>
        private async Task MergeFiles()
        {
            if (inputPaths.Count == 0)
            {
                return;
            }

            fileCount = inputPaths.Count;
            completedCount = 0;
            progress.Report(0);
            
            foreach (var path in inputPaths)
            {
                await AddPdf(path.Text);
                progress.Report(++completedCount * 100 / fileCount);
            }
        }

        /// <summary>
        ///  Add the PDF file found at the given path to the output.
        /// </summary>
        /// <param name="path">The path to a PDF file.</param>
        /// <returns>An await-able task.</returns>
        private async Task AddPdf(string path)
        {
            var content = writer.DirectContent;

            PdfReader reader = null;
            await Task.Run(() =>
                 {
                     try
                     {
                         reader = new PdfReader(path);
                     }
                     catch (IOException)
                     {
                     }
                 });

            // Opening the PDF failed.
            if (reader == null)
            {
                return;
            }

            int pageCount = reader.NumberOfPages;
            for (int currentPage = 1; currentPage <= pageCount; currentPage++)
            {
                document.SetPageSize(reader.GetPageSize(currentPage));
                document.NewPage();
                var page = await Task.Run<PdfImportedPage>(() =>
                    writer.GetImportedPage(reader, currentPage));
                var rotation = reader.GetPageRotation(currentPage);
                if (rotation == 90 || rotation == 270)
                {
                    // Add the page with a transform matrix that rotates 90 degrees.
                    await Task.Run(() => content.AddTemplate(page, 0f, -1f, 1f, 0f, 0f, page.Height));
                }
                else
                {
                    // Add the page with an identity transform.
                    await Task.Run(() => content.AddTemplate(page, 1f, 0f, 0f, 1f, 0f, 0f));
                }
            }
        }

        /// <summary>
        ///  Close the document.
        /// </summary>
        private void CloseDocument()
        {
            document.Close();
            writer.Close();
            document = null;
            writer = null;
        }
    }
}
