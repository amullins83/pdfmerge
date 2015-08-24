namespace PDFMergeDesktop
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for <c>MainWindow.xaml</c>.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        ///  The view model for the window.
        /// </summary>
        private MainWindowViewModel viewModel = new MainWindowViewModel();

        /// <summary>
        ///  Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
