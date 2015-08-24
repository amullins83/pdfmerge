namespace PDFMergeDesktop
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///  Boolean to visibility converter with invert option.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        ///  Convert the value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type to which the value should be converted (ignored).</param>
        /// <param name="parameter">The conversion parameter, used to determine whether to invert.</param>
        /// <param name="culture">The target culture (ignored).</param>
        /// <returns>
        ///   <c>Visibility.Visible</c> if the value is true (or false with the "Invert" option),
        ///   otherwise <c>Visibility.Collapsed</c>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isTrue = value as bool?;
            var isInverting = parameter != null &&
                StringComparer.Ordinal.Equals(parameter.ToString(), "Invert");
            if ((isTrue == true && !isInverting) ||
                (isTrue == false && isInverting))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        ///  Convert a visibility back to a boolean.
        /// </summary>
        /// <param name="value">The visibility to convert.</param>
        /// <param name="targetType">The type from which the value should be converted (ignored).</param>
        /// <param name="parameter">The converter parameter, used to determine whether to invert.</param>
        /// <param name="culture">The target culture (ignored).</param>
        /// <returns>
        ///  True if the input is visible (or invisible with the Invert option),
        ///  otherwise false.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = value as Visibility?;
            var isInverting = parameter != null &&
                StringComparer.Ordinal.Equals(parameter.ToString(), "Invert");
            if (visibility == Visibility.Visible)
            {
                return !isInverting;
            }
            else
            {
                return isInverting;
            }
        }
    }
}
