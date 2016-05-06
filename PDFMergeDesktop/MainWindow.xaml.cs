﻿namespace PDFMergeDesktop
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for <c>MainWindow.xaml</c>.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        ///  The exit command, which is somehow not a standard application command.
        /// </summary>
        private static RoutedUICommand exitCommand = new RoutedUICommand(
            PDFMergeDesktop.Resources.MainWindowStrings.Exit,
            "Exit",
            typeof(MainWindow));
     
        /// <summary>
        ///  The view model for the window.
        /// </summary>
        private MainWindowViewModel viewModel = new MainWindowViewModel();
        
        /// <summary>
        ///  Initializes static members of the <see cref="MainWindow"/> class by registering the exit command.
        /// </summary>
        static MainWindow()
        {
            var exitBinding = new CommandBinding(exitCommand);
            CommandManager.RegisterClassCommandBinding(typeof(MainWindow), exitBinding);
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        /// <summary>
        ///  Gets the exit command.
        /// </summary>
        public static RoutedUICommand ExitCommand
        {
            get { return exitCommand; }
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

        /// <summary>
        ///  Enable the given command when not processing.
        /// </summary>
        /// <param name="sender">The command that raised the event (ignored).</param>
        /// <param name="e">The event arguments, including a property to be set true.</param>
        private void CanExecuteUnlessProcessing(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !viewModel.IsProcessing;
        }

        /// <summary>
        ///  Determine whether the save command can be executed.
        /// </summary>
        /// <param name="sender">The command that raised the event (ignored).</param>
        /// <param name="e">The event arguments, including a property to set.</param>
        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !viewModel.IsProcessing && viewModel.InputPaths.Count > 0;
        }
        
        /// <summary>
        ///  Determine whether the delete command can be executed.
        /// </summary>
        /// <param name="sender">The command that raised the event (ignored).</param>
        /// <param name="e">The event arguments, including a property to set.</param>
        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !viewModel.IsProcessing && viewModel.InputPaths.Count > 0 &&
                            viewModel.SelectedItems.Count() > 0;
        }

        /// <summary>
        ///  Execute the new command.
        /// </summary>
        /// <param name="sender">The command that raised the event (ignored).</param>
        /// <param name="e">The event arguments (ignored).</param>
        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            viewModel.Reset();
        }

        /// <summary>
        ///  Execute the open command.
        /// </summary>
        /// <param name="sender">The command that raised the event (ignored).</param>
        /// <param name="e">The event arguments (ignored).</param>
        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            viewModel.Open();
        }

        /// <summary>
        ///  Execute the save command.
        /// </summary>
        /// <param name="sender">The command that raised the event (ignored).</param>
        /// <param name="e">The event arguments (ignored).</param>
        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            viewModel.Save();
        }

        /// <summary>
        ///  Execute the delete command.
        /// </summary>
        /// <param name="sender">The command that raised the event (ignored).</param>
        /// <param name="e">The event arguments (ignored).</param>
        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            viewModel.Remove();
        }

        /// <summary>
        ///  Execute the exit command.
        /// </summary>
        /// <param name="sender">The command that raised the event (ignored).</param>
        /// <param name="e">The event arguments (ignored).</param>
        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}
