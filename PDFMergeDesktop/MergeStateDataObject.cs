/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace PDFMergeDesktop
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Xml.Serialization;

    /// <summary>
    ///  Handles loading and saving window data.
    /// </summary>
    internal class MergeStateDataObject
    {
        /// <summary>
        ///  The serializable state of the window.
        /// </summary>
        private MergeState state = new MergeState();

        /// <summary>
        ///  A value indicating whether the load was successful.
        /// </summary>
        private bool didLoad = false;

        /// <summary>
        ///  A value indicating whether the save was successful.
        /// </summary>
        private bool didSave = false;

        /// <summary>
        ///  Initializes a new instance of the <see cref="MergeStateDataObject"/> class.
        /// </summary>
        /// <param name="viewModel">The view model from which to extract data.</param>
        internal MergeStateDataObject(MainWindowViewModel viewModel)
        {
            state.InputPaths = viewModel.InputPaths.Select(item => item.Text).ToList();
            state.OutputPath = viewModel.OutputPath;
            didLoad = true;
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="MergeStateDataObject"/> class.
        /// </summary>
        /// <param name="path">The path to a file from which to retrieve data.</param>
        internal MergeStateDataObject(string path)
        {
            try
            {
                using (var file = new FileStream(path, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(MergeState));
                    var newState = serializer.Deserialize(file) as MergeState;
                    if (newState != null)
                    {
                        state = newState;
                    }
                }

                didLoad = true;
            }
            catch (IOException e)
            {
                MessageBox.Show(
                    string.Format(Resources.MainWindowStrings.ErrorOnLoad, e.Message),
                    Resources.MainWindowStrings.ErrorOnLoadCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        ///  Gets the list of input paths to merge.
        /// </summary>
        internal List<string> InputPaths
        {
            get { return state.InputPaths; }
        }

        /// <summary>
        ///  Gets or sets the path to which the merged PDF should be saved.
        /// </summary>
        internal string OutputPath
        {
            get
            {
                return state.OutputPath;
            }

            set
            {
                state.OutputPath = value;
            }
        }

        /// <summary>
        ///  Gets a value indicating whether the load was successful.
        /// </summary>
        internal bool DidLoad
        {
            get { return didLoad; }
        }

        /// <summary>
        ///  Gets a value indicating whether the save was successful.
        /// </summary>
        internal bool DidSave
        {
            get { return didSave; }
        }

        /// <summary>
        ///  Save the state to the given path.
        /// </summary>
        /// <param name="path">The path to which window state should be saved.</param>
        internal void Save(string path)
        {
            didSave = false;
            try
            {
                using (var file = new FileStream(path, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(MergeState));
                    serializer.Serialize(file, state);
                }

                didSave = true;
            }
            catch (IOException e)
            {
                MessageBox.Show(
                    string.Format(Resources.MainWindowStrings.ErrorOnSave, e.Message),
                    Resources.MainWindowStrings.ErrorOnSaveCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
