namespace PDFMergeDesktop
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Threading;

    /// <summary>
    ///  The view model for the main page.
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged, IProgress<int>
    {
        /// <summary>
        ///  The current path to add to the collection.
        /// </summary>
        private string inputPath = string.Empty;

        /// <summary>
        ///  The model for performing the merge task.
        /// </summary>
        private PdfMerger merger;

        /// <summary>
        ///  Command to browse for input files.
        /// </summary>
        private RelayCommand browseInputCommand;

        /// <summary>
        ///  Command to add an input path to the collection.
        /// </summary>
        private RelayCommand addCommand;

        /// <summary>
        ///  Command to remove an input path from the collection.
        /// </summary>
        private RelayCommand removeCommand;

        /// <summary>
        ///  Command to perform the merge task.
        /// </summary>
        private RelayCommand mergeCommand;

        /// <summary>
        ///  Command to move the selected item up.
        /// </summary>
        private RelayCommand moveUpCommand;

        /// <summary>
        ///  Command to move the selected item down.
        /// </summary>
        private RelayCommand downCommand;

        /// <summary>
        ///  The progress for the current operation, if any.
        /// </summary>
        private int percentComplete = 0;

        /// <summary>
        ///  The dispatcher for the GUI thread.
        /// </summary>
        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        /// <summary>
        ///  A value indicating whether a merge is in progress.
        /// </summary>
        private bool isProcessing = false;

        /// <summary>
        ///  The index of the selected item in the list.
        /// </summary>
        private int selectedIndex = 0;

        /// <summary>
        ///  Initializes a new instance of the
        ///  <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel()
        {
            merger = new PdfMerger();
            merger.Progress = this;
            browseInputCommand = new RelayCommand(
                new Action<object>(BrowseInputExecute));
            addCommand = new RelayCommand(
                new Action<object>(AddExecute),
                new Predicate<object>(AddCanExecute));
            removeCommand = new RelayCommand(
                new Action<object>(RemoveExecute),
                new Predicate<object>(RemoveCanExecute));
            mergeCommand = new RelayCommand(
                new Action<object>(MergeExecute),
                new Predicate<object>(MergeCanExecute));
            moveUpCommand = new RelayCommand(
                new Action<object>(UpExecute),
                new Predicate<object>(UpCanExecute));
            downCommand = new RelayCommand(
                new Action<object>(DownExecute),
                new Predicate<object>(DownCanExecute));
        }

        /// <summary>
        ///  Raised when an observable property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        ///  Gets or sets the destination path for the merged PDF.
        /// </summary>
        public string OutputPath
        {
            get
            {
                return merger.OutputPath;
            }

            set
            {
                if (OutputPath != value)
                {
                    var originalMergeCanExecute = MergeCanExecute(null);
                    merger.OutputPath = value;
                    OnPropertyChanged("OutputPath");
                    if (originalMergeCanExecute != MergeCanExecute(null))
                    {
                        mergeCommand.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        /// <summary>
        ///  Gets or sets the currently active input path.
        /// </summary>
        public string InputPath
        {
            get
            {
                return inputPath;
            }

            set
            {
                if (value != inputPath)
                {
                    var originalAddCanExecute = AddCanExecute(null);
                    var originalRemoveCanExecute = RemoveCanExecute(null);
                    inputPath = value;
                    OnPropertyChanged("InputPath");
                    if (originalAddCanExecute != AddCanExecute(null))
                    {
                        addCommand.RaiseCanExecuteChanged();
                    }

                    if (originalRemoveCanExecute != RemoveCanExecute(null))
                    {
                        removeCommand.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        /// <summary>
        ///  Gets the command to add new input paths.
        /// </summary>
        public ICommand AddCommand
        {
            get { return addCommand; }
        }

        /// <summary>
        ///  Gets the command to remove the selected input path.
        /// </summary>
        public ICommand RemoveCommand
        {
            get { return removeCommand; }
        }

        /// <summary>
        ///  Gets the command to browse for an input path.
        /// </summary>
        public ICommand BrowseInputCommand
        {
            get { return browseInputCommand; }
        }

        /// <summary>
        ///  Gets the command to merge the files.
        /// </summary>
        public ICommand MergeCommand
        {
            get { return mergeCommand; }
        }

        /// <summary>
        ///  Gets the command to move an item up the list.
        /// </summary>
        public ICommand UpCommand
        {
            get { return moveUpCommand; }
        }

        /// <summary>
        ///  Gets the command to move an item down the list.
        /// </summary>
        public ICommand DownCommand
        {
            get { return downCommand; }
        }

        /// <summary>
        ///  Gets a value indicating whether a merge is in progress.
        /// </summary>
        public bool IsProcessing
        {
            get
            {
                return isProcessing;
            }

            private set
            {
                if (isProcessing != value)
                {
                    isProcessing = value;
                    dispatcher.BeginInvoke(
                        new Action<string>(OnPropertyChanged),
                        "IsProcessing");
                }
            }
        }

        /// <summary>
        ///  Gets the percentage completion of the current operation, if any.
        /// </summary>
        public int PercentComplete
        {
            get
            {
                return percentComplete;
            }

            private set
            {
                if (percentComplete != value)
                {
                    percentComplete = value;
                    dispatcher.BeginInvoke(
                        new Action<string>(OnPropertyChanged),
                        "PercentComplete");
                }
            }
        }

        /// <summary>
        ///  Gets the collection of input paths to merge.
        /// </summary>
        public ObservableCollection<string> InputPaths
        {
            get { return merger.InputPaths; }
        }

        /// <summary>
        ///  Gets or sets the selected index into the list of input paths.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }

            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                    moveUpCommand.RaiseCanExecuteChanged();
                    downCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        ///  Update the percent completion.
        /// </summary>
        /// <param name="progress">The new completion percentage.</param>
        public void Report(int progress)
        {
            PercentComplete = progress;
            if (PercentComplete >= 100)
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        ///  Raise the PropertyChanged event.
        /// </summary>
        /// <param name="name">The property changed.</param>
        private void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        ///  Execute the browse input command.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        private void BrowseInputExecute(object parameter)
        {
            var paths = BrowseFile.Open(
                Resources.MainWindowStrings.AddInput,
                Resources.MainWindowStrings.FileFilter,
                App.Current.MainWindow);
            InputPath = string.Join(";", paths);
        }

        /// <summary>
        ///  Execute the add command.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        private void AddExecute(object parameter)
        {
            var originalCanMerge = MergeCanExecute(null);
            var paths = inputPath.Split(';');
            foreach (var path in paths)
            {
                merger.InputPaths.Add(path);
            }

            InputPath = string.Empty;
            if (originalCanMerge != MergeCanExecute(null))
            {
                mergeCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        ///  Determine whether the add command can be executed.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        /// <returns>A value indicating whether the add command can be executed.</returns>
        private bool AddCanExecute(object parameter)
        {
            return inputPath != null && inputPath != string.Empty;
        }

        /// <summary>
        ///  Execute the remove command.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        private void RemoveExecute(object parameter)
        {
            merger.InputPaths.Remove(inputPath);
            InputPath = string.Empty;
        }

        /// <summary>
        ///  Determine whether the remove command can be executed.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        /// <returns>A value indicating whether the remove command can be executed.</returns>
        private bool RemoveCanExecute(object parameter)
        {
            return merger.InputPaths.Any(path =>
                StringComparer.CurrentCultureIgnoreCase.Equals(path, inputPath));
        }

        /// <summary>
        ///  Execute the merge command.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        private void MergeExecute(object parameter)
        {
            OutputPath = BrowseFile.Save(
                Resources.MainWindowStrings.SetOutput,
                Resources.MainWindowStrings.FileFilter,
                App.Current.MainWindow);
            if (merger.CanMerge)
            {
                merger.MergeAsync().ContinueWith(new Action<Task>((task) => IsProcessing = false));
                IsProcessing = true;
            }
            else
            {
                System.Windows.MessageBox.Show(Resources.MainWindowStrings.CouldNotMerge);
            }
        }

        /// <summary>
        ///  Determine whether the merge command can execute.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        /// <returns>A value indicating whether the merge command can be executed.</returns>
        private bool MergeCanExecute(object parameter)
        {
            return InputPaths.Count > 1;
        }

        /// <summary>
        ///  Execute the "up" command.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored)</param>
        private void UpExecute(object parameter)
        {
            InputPaths.Move(selectedIndex, selectedIndex - 1);
        }

        /// <summary>
        ///  Determine whether the up command can execute.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored)</param>
        /// <returns>A value indicating whether the up command can be executed.</returns>
        private bool UpCanExecute(object parameter)
        {
            return selectedIndex > 0;
        }

        /// <summary>
        ///  Execute the "down" command.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored)</param>
        private void DownExecute(object parameter)
        {
            InputPaths.Move(selectedIndex, selectedIndex + 1);
        }

        /// <summary>
        ///  Determine whether the down command can execute.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored)</param>
        /// <returns>A value indicating whether the down command can be executed.</returns>
        private bool DownCanExecute(object parameter)
        {
            return selectedIndex >= 0 && selectedIndex < InputPaths.Count - 1;
        }
    }
}
