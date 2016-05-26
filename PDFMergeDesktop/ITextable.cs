/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace PDFMergeDesktop
{
    /// <summary>
    ///  Interface for objects that have readable and writable text properties.
    /// </summary>
    public interface ITextable
    {
        /// <summary>
        ///  Gets or sets the text property of the object.
        /// </summary>
        string Text { get; set; }
    }
}
