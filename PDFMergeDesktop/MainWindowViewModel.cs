/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace PDFMergeDesktop
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
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
            InputPaths.CollectionChanged += RaiseCanMergeChanged;
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
                    inputPath = value;
                    OnPropertyChanged("InputPath");
                    addCommand.RaiseCanExecuteChanged();
                    removeCommand.RaiseCanExecuteChanged();
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
        public ObservableCollection<StringItem> InputPaths
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
                }
            }
        }

        /// <summary>
        ///  Gets or sets a collection of selected items.
        /// </summary>
        public IEnumerable<StringItem> SelectedItems
        {
            get;
            set;
        }

        /// <summary>
        ///  Remove the selected items from the list.
        /// </summary>
        public void Remove()
        {
            if (RemoveCanExecute(null))
            {
                RemoveExecute(null);
            }
        }

        /// <summary>
        ///  Reset the window to an empty state.
        /// </summary>
        public void Reset()
        {
            InputPaths.Clear();
            InputPath = string.Empty;
            OutputPath = string.Empty;
        }

        /// <summary>
        ///  Save the state of the window.
        /// </summary>
        public void Save()
        {
            var path = BrowseFile.Save(
                Resources.MainWindowStrings.Save,
                Resources.MainWindowStrings.ProjectFilter,
                Application.Current.MainWindow);

            if (!string.IsNullOrWhiteSpace(path))
            {
                SaveStateToFile(path);
            }
        }

        /// <summary>
        ///  Load a saved window state.
        /// </summary>
        public void Open()
        {
            var paths = BrowseFile.Open(
                Resources.MainWindowStrings.Open,
                Resources.MainWindowStrings.ProjectFilter,
                Application.Current.MainWindow);

            if (paths.Any(p => !string.IsNullOrWhiteSpace(p)))
            {
                LoadStateFromFile(paths.First(p => !string.IsNullOrWhiteSpace(p)));
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
                Application.Current.MainWindow);
            if (paths.Any())
            {
                AddPaths(paths);
            }
        }

        /// <summary>
        ///  Execute the add command.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        private void AddExecute(object parameter)
        {
            var paths = inputPath.Split(';');
            AddPaths(paths);
        }

        /// <summary>
        ///  Add the given paths to the input paths collection.
        /// </summary>
        /// <param name="paths">The collection of paths to add.</param>
        private void AddPaths(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                merger.InputPaths.Add(new StringItem(path));
            }

            InputPath = string.Empty;
        }

        /// <summary>
        ///  Determine whether the add command can be executed.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        /// <returns>A value indicating whether the add command can be executed.</returns>
        private bool AddCanExecute(object parameter)
        {
            return inputPath != null &&
                   NoneMatch(inputPath) &&
                   !Path.GetInvalidPathChars().Any(c => inputPath.Contains(c)) &&
                   StringComparer.CurrentCultureIgnoreCase.Equals(Path.GetExtension(inputPath), ".pdf") &&
                   File.Exists(inputPath);
        }

        /// <summary>
        ///  Determine whether any input paths in the list match the given path.
        /// </summary>
        /// <param name="path">The path to match against the list.</param>
        /// <returns>A value indicating whether any input paths in the list match the given path.</returns>
        private bool NoneMatch(string path)
        {
            return !InputPaths.Any(p => StringComparer.CurrentCultureIgnoreCase.Equals(p.Text, inputPath));
        }

        /// <summary>
        ///  Execute the remove command.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        private void RemoveExecute(object parameter)
        {
            // Copy to list to prevent changing during loop
            foreach (var item in SelectedItems.ToList())
            {
                InputPaths.Remove(InputPaths.FirstOrDefault(pathItem =>
                    StringComparer.CurrentCultureIgnoreCase.Equals(pathItem.Text, item.Text)));
            }

            SelectedItems = null;
            InputPath = string.Empty;
        }

        /// <summary>
        ///  Determine whether the remove command can be executed.
        /// </summary>
        /// <param name="parameter">The command parameter (ignored).</param>
        /// <returns>A value indicating whether the remove command can be executed.</returns>
        private bool RemoveCanExecute(object parameter)
        {
            return SelectedItems != null && SelectedItems.Any();
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
                Application.Current.MainWindow);

            if (string.IsNullOrWhiteSpace(OutputPath))
            {
                return;
            }

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
        ///  Update the merge command's CanExecute value.
        /// </summary>
        /// <param name="sender">The collection that raised the event (ignored).</param>
        /// <param name="e">The event arguments (ignored).</param>
        private void RaiseCanMergeChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            mergeCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        ///  Save window state to a file.
        /// </summary>
        /// <param name="path">The path to the file into which window state will be saved.</param>
        private void SaveStateToFile(string path)
        {
            var data = new MergeStateDataObject(this);
            if (data.DidLoad)
            {
                data.Save(path);
            }
        }

        /// <summary>
        ///  Load window state from a file.
        /// </summary>
        /// <param name="path">The path to the file from which to load window state.</param>
        private void LoadStateFromFile(string path)
        {
            var data = new MergeStateDataObject(path);
            if (!data.DidLoad)
            {
                return;
            }

            InputPaths.Clear();
            foreach (var inputPath in data.InputPaths)
            {
                InputPaths.Add(new StringItem(inputPath));
            }

            OutputPath = data.OutputPath;
        }
    }
}
