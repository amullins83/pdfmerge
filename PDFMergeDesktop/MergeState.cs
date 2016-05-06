namespace PDFMergeDesktop
{
    using System.Collections.Generic;

    /// <summary>
    ///  The serializable representation of window state.
    /// </summary>
    public class MergeState
    {
        /// <summary>
        ///  The path to which the merged PDF should be saved.
        /// </summary>
        private string outputPath = string.Empty;

        /// <summary>
        ///  The ordered paths to PDF's to be combined.
        /// </summary>
        private List<string> inputPaths = new List<string>();

        /// <summary>
        ///  Gets or sets the output path.
        /// </summary>
        public string OutputPath
        {
            get { return outputPath; }
            set { outputPath = value; }
        }

        /// <summary>
        ///  Gets or sets the input paths.
        /// </summary>
        public List<string> InputPaths
        {
            get { return inputPaths; }
            set { inputPaths = value; }
        }
    }
}
