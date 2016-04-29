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
