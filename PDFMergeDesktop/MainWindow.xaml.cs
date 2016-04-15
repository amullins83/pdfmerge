namespace PDFMergeDesktop
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

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

        /// <summary>
        ///  Handle changes to the multiple item selection in the list box.
        /// </summary>
        /// <param name="sender">The list box that raised the event (ignored).</param>
        /// <param name="e">The event arguments.</param>
        private void StringItemDragDropListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectionList = new List<StringItem>();
            foreach (var item in InputsBox.SelectedItems)
            {
                if (item is StringItem)
                {
                    selectionList.Add((StringItem)item);
                }
            }

            viewModel.SelectedItems = selectionList;
        }
    }
}
