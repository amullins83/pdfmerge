namespace PDFMergeDesktop
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Microsoft.Win32;

    /// <summary>
    ///  Methods to assist when browsing files.
    /// </summary>
    public static class BrowseFile
    {
        /// <summary>
        ///  Show an open file browse dialog and return its result.
        /// </summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="filter">The file extension filter.</param>
        /// <returns>The selected path or null.</returns>
        public static IEnumerable<string> Open(string title, string filter)
        {
            return Open(title, filter, null);
        }

        /// <summary>
        ///  Show an open file browse dialog and return its result.
        /// </summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="filter">The file extension filter.</param>
        /// <param name="owner">The owning window for the browse dialog.</param>
        /// <returns>The selected path or null.</returns>
        public static IEnumerable<string> Open(string title, string filter, Window owner)
        {
            var open = new OpenFileDialog();
            open.Multiselect = true;
            return RunDialog(title, filter, owner, open);
        }

        /// <summary>
        ///  Show a save file browse dialog and return its result.
        /// </summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="filter">The file extension filter.</param>
        /// <returns>The selected path or null.</returns>
        public static string Save(string title, string filter)
        {
            return Save(title, filter, null);
        }

        /// <summary>
        ///  Show a save file browse dialog and return its result.
        /// </summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="filter">The file extension filter.</param>
        /// <param name="owner">The owning window for the browse dialog.</param>
        /// <returns>The selected path or null.</returns>
        public static string Save(string title, string filter, Window owner)
        {
            return RunDialog(title, filter, owner, new SaveFileDialog()).FirstOrDefault();
        }

        /// <summary>
        ///  Show the given dialog and return its result.
        /// </summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="filter">The file extension filter.</param>
        /// <param name="owner">The owning window for the browse dialog.</param>
        /// <param name="dialog">The dialog to show.</param>
        /// <returns>The selected path or null.</returns>
        private static IEnumerable<string> RunDialog(string title, string filter, Window owner, FileDialog dialog)
        {
            dialog.Title = title;
            dialog.Filter = filter;
            bool? fileCaptured = dialog.ShowDialog(owner);
            if (fileCaptured == true)
            {
                return dialog.FileNames;
            }

            return new List<string>();
        }
    }
}
