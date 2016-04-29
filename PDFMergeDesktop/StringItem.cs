namespace PDFMergeDesktop
{
    /// <summary>
    ///  A wrapper class around strings.
    /// </summary>
    public class StringItem : ITextable
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="StringItem"/> class.
        /// </summary>
        public StringItem()
        {
            Text = string.Empty;
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="StringItem"/> class.
        /// </summary>
        /// <param name="text">The initial text of the item.</param>
        public StringItem(string text)
        {
            Text = text;
        }

        /// <summary>
        ///  Gets or sets the text.
        /// </summary>
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        ///  Represent the given item as text.
        /// </summary>
        /// <returns>The <see cref="Text"/> property.</returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
