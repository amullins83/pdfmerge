/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

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
        ///  Gets the input paths.
        /// </summary>
        public List<string> InputPaths
        {
            get { return inputPaths; }
        }
    }
}
