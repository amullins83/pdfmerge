namespace PDFMergeDesktop
{
    using System.Windows;
    using static Resources.AboutStrings;

    /// <summary>
    /// Interaction logic for the About window.
    /// </summary>
    public partial class AboutPdfMerge : Window
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="AboutPdfMerge"/> class.
        /// </summary>
        public AboutPdfMerge()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        ///  Gets the text to display in the author area.
        /// </summary>
        public static string AuthorText
        {
            get { return string.Format(AuthorFormat, Author); }
        }

        /// <summary>
        ///  Gets the version description text.
        /// </summary>
        public static string VersionText
        {
            get { return string.Format(VersionFormat, typeof(AboutPdfMerge).Assembly.GetName().Version); }
        }
    }
}
