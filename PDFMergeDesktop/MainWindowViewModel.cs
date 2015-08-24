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
        ///  Command to browse for output files.
        /// </summary>
        private RelayCommand browseOutputCommand;

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
        ///  Initializes a new instance of the
        ///  <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel()
        {
            merger = new PdfMerger();
            merger.Progress = this;
            browseInputCommand = new RelayCommand(
                new Action<object>(BrowseInputExecute));
            browseOutputCommand = new RelayCommand(
                new Action<object>(BrowseOutputExecute));
            addCommand = new RelayCommand(
                new Action<object>(AddExecute),
                new Predicate<object>(AddCanExecute));
            removeCommand = new RelayCommand(
                new Action<object>(RemoveExecute),
                new Predicate<object>(RemoveCanExecute));
            mergeCommand = new RelayCommand(
                new Action<object>(MergeExecute),
                new Predicate<object>(MergeCanExecute));
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
        ///  Gets the command to browse for an output path.
        /// </summary>
        public ICommand BrowseOutputCommand
        {
            get { return browseOutputCommand; }
        }

        /// <summary>
        ///  Gets the command to merge the files.
        /// </summary>
        public ICommand MergeCommand
        {
            get { return mergeCommand; }
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
            InputPath = BrowseFile.Open(
                Resources.MainWindowStrings.AddInput,
                Resources.MainWindowStrings.FileFilter,
                App.Current.MainWindow);
        }

        /// <summary>
        ///  Execute the browse output command.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        private void BrowseOutputExecute(object parameter)
        {
            OutputPath = BrowseFile.Save(
                Resources.MainWindowStrings.SetOutput,
                Resources.MainWindowStrings.FileFilter,
                App.Current.MainWindow);
        }

        /// <summary>
        ///  Execute the add command.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        private void AddExecute(object parameter)
        {
            var originalCanMerge = MergeCanExecute(null);
            merger.InputPaths.Add(inputPath);
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
            merger.MergeAsync().ContinueWith(new Action<Task>((task) => IsProcessing = false));
            IsProcessing = true;
        }

        /// <summary>
        ///  Determine whether the merge command can execute.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        /// <returns>A value indicating whether the merge command can be executed.</returns>
        private bool MergeCanExecute(object parameter)
        {
            return merger.CanMerge;
        }
    }
}
