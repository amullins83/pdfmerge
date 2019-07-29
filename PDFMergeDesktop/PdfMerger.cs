/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace PDFMergeDesktop
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;

    using iText.Kernel.Pdf;
    
    /// <summary>
    ///  Merges a given collection of PDF files.
    /// </summary>
    public class PdfMerger : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        ///  The path to store the merged PDF file.
        /// </summary>
        private string outputPath;

        /// <summary>
        ///  The writer object for this task.
        /// </summary>
        private PdfWriter writer = null;

        /// <summary>
        ///  The output document for this task.
        /// </summary>
        private PdfDocument document = null;

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
                    outputPath = System.IO.Path.GetFullPath(outputPath);
                }
                catch (IOException)
                {
                    return false;
                }

                return InputPaths.Count > 1;
            }
        }

        /// <summary>
        /// Gets a collection of paths to files to merge
        /// </summary>
        public ObservableCollection<StringItem> InputPaths { get; } = new ObservableCollection<StringItem>();

        /// <summary>
        /// Gets a value indicating whether the merger created a PDF document.
        /// </summary>
        public bool DidCreatePdf { get; private set; } = false;

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

            DidCreatePdf = false;
            await CreateDocument().ConfigureAwait(false);
            await MergeFiles().ConfigureAwait(false);
            CloseDocument();
            DidCreatePdf = true;
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
                    writer = new PdfWriter(outputStream);
                    document = new PdfDocument(writer);
                }).ConfigureAwait(false);
        }

        /// <summary>
        ///  Merge the input files.
        /// </summary>
        /// <returns>An await-able task.</returns>
        private async Task MergeFiles()
        {
            if (InputPaths.Count == 0)
            {
                return;
            }

            fileCount = InputPaths.Count;
            completedCount = 0;
            progress.Report(0);
            
            foreach (var path in InputPaths)
            {
                await AddPdf(path.Text).ConfigureAwait(false);
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
            using (PdfReader reader = await GetReaderFor(path).ConfigureAwait(false))
            {
                // Opening the PDF failed.
                if (reader == null)
                {
                    return;
                }

                // We cannot copy PDFs with owner passwords
                if (!reader.IsOpenedWithFullPermission())
                {
                    MessageBox.Show(
                        string.Format(CultureInfo.CurrentCulture, Resources.MergerStrings.OwnerPasswordError, path),
                        Resources.MergerStrings.OwnerPasswordErrorCaption,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                GetPages(reader);
            }
        }

        /// <summary>
        ///  Create a <see cref="PdfReader"/> object from the file at the given path.
        /// </summary>
        /// <param name="path">The path to a PDF file.</param>
        /// <returns>An await-able task that eventually contains a <see cref="PdfReader"/> result if successful.</returns>
        private static async Task<PdfReader> GetReaderFor(string path)
        {
            PdfReader reader = null;

            await Task.Run(() =>
            {
                try
                {
                    reader = new PdfReader(path);
                }
                catch (IOException e)
                {
                    MessageBox.Show(
                        string.Format(CultureInfo.CurrentCulture, Resources.MergerStrings.OpenError, path, e.Message),
                        Resources.MergerStrings.OpenErrorCaption,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }).ConfigureAwait(false);

            return reader;
        }

        /// <summary>
        ///  Copy pages from the given reader into the given content.
        /// </summary>
        /// <param name="reader">
        ///  The reader to collect data from a PDF document that has been opened with full permissions
        /// </param>
        private void GetPages(PdfReader reader)
        {
            using (PdfDocument inputDoc = new PdfDocument(reader))
            {
                int pageCount = inputDoc.GetNumberOfPages();
                for (int pageNumber = 1; pageNumber <= pageCount; pageNumber++)
                {
                    document.AddPage(inputDoc.GetPage(pageNumber).CopyTo(document));
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            if (disposing)
            {
                writer?.Dispose();
                document?.Close();
            }

            disposedValue = true;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
