namespace PDFMergeDesktop
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    ///  A list box class that allows reordering of items via drag and drop.
    /// </summary>
    /// <typeparam name="T">The type of item displayed in the list.</typeparam>
    public class DragDropListBox<T> : ListBox
        where T : class, ITextable, new()
    {
        /// <summary>
        ///  Dependency property to get and set a filter for accepting dropped files.
        /// </summary>
        public static readonly DependencyProperty FileFilterProperty =
            DependencyProperty.Register("FileFilter", typeof(string), typeof(DragDropListBox<T>), new PropertyMetadata("*.*"));

        /// <summary>
        ///  Captures the starting point of a drag operation.
        /// </summary>
        private Point dragStartPoint;
        
        /// <summary>
        ///  Initializes a new instance of the <see cref="DragDropListBox{T}"/> class.
        /// </summary>
        public DragDropListBox()
        {
            PreviewMouseMove += TrackDrag;
            ItemContainerStyle = new Style(typeof(ListBoxItem));
            ItemContainerStyle.Setters.Add(
                new EventSetter(
                    PreviewMouseLeftButtonDownEvent,
                    new MouseButtonEventHandler(StartDrag)));
            ItemContainerStyle.Setters.Add(
                new EventSetter(
                    DropEvent,
                    new DragEventHandler(DropItem)));
            ItemContainerStyle.Setters.Add(
                new EventSetter(
                    DragOverEvent,
                    new DragEventHandler(CheckItem)));

            Drop += DropItem;
            DragOver += CheckItem;
            AllowDrop = true;
        }

        /// <summary>
        ///  Gets or sets the filter to determine whether a file should be accepted.
        /// </summary>
        public string FileFilter
        {
            get { return (string)GetValue(FileFilterProperty); }
            set { SetValue(FileFilterProperty, value); }
        }

        /// <summary>
        ///  Use our custom list box item class.
        /// </summary>
        /// <returns>A new instance of the <see cref="DeferredSelectionListItem"/> class.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DeferredSelectionListItem();
        }

        /// <summary>
        ///  Respond to a preview mouse move event by tracking drag starts and current positions.
        /// </summary>
        /// <param name="sender">
        ///  The object that raised the event (ignored).
        /// </param>
        /// <param name="e">
        ///  The mouse event arguments.
        /// </param>
        private void TrackDrag(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(null);
            var dragDistance = point - dragStartPoint;
            if (e.LeftButton == MouseButtonState.Pressed && IsOverThreshold(dragDistance))
            {
                var item = sender as FrameworkElement;
                if (item != null)
                {
                    var selection = new List<T>();
                    foreach (var dataItem in SelectedItems)
                    {
                        if (dataItem is T)
                        {
                            selection.Add((T)dataItem);
                        }
                    }

                    DragDrop.DoDragDrop(item, selection, DragDropEffects.Move);
                }
            }
        }

        /// <summary>
        ///  Respond to a preview left click event by starting a new drag action.
        /// </summary>
        /// <param name="sender">The list box item that raised the event.</param>
        /// <param name="e">The mouse button event arguments.</param>
        private void StartDrag(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = e.GetPosition(null);
        }

        /// <summary>
        ///  Respond to a drop event by reordering the item collection.
        /// </summary>
        /// <param name="sender">The item that raised the event.</param>
        /// <param name="e">The drag event arguments.</param>
        private void DropItem(object sender, DragEventArgs e)
        {
            int targetIndex = Items.Count;
            if (sender is ListBoxItem)
            {
                // The drop has a specific target item
                var target = ((ListBoxItem)sender).DataContext as T;
                targetIndex = Items.IndexOf(target);
                var source = e.Data.GetData(typeof(List<T>)) as List<T>;
                if (source != null)
                {
                    // The source comes from our drag start
                    foreach (var dataItem in source)
                    {
                        var sourceIndex = Items.IndexOf(dataItem);
                        Move(sourceIndex, targetIndex);
                    }

                    return;
                }
            }
            
            // Allow dropping files from other windows
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var items = ItemsSource as IList<T>;
                if (items == null)
                {
                    return;
                }

                for (int i = 0, numDroppedFiles = 0; i < files.Length; i++)
                {
                    if (IsAcceptablePath(files[i]))
                    {
                        var item = new T();
                        item.Text = files[i];
                        items.Insert(targetIndex + numDroppedFiles, item);
                    }
                }
            }
        }

        /// <summary>
        ///  Check if any item in the given data collection matches our filter.
        /// </summary>
        /// <param name="sender">The object that raised the event (ignored, assumed this).</param>
        /// <param name="e">The event arguments, including the data.</param>
        private void CheckItem(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (!files.Any(f => IsAcceptablePath(f)))
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        ///  Determine if the given path can be added.
        /// </summary>
        /// <param name="path">The path to inspect.</param>
        /// <returns>A value indicating whether the path can be added.</returns>
        private bool IsAcceptablePath(string path)
        {
            return IsUnique(path) && IsFilterMatch(path);
        }

        /// <summary>
        ///  Determine if the given path matches our filter.
        /// </summary>
        /// <param name="path">The path to inspect.</param>
        /// <returns>A value indicating whether the given path matches our filter.</returns>
        private bool IsFilterMatch(string path)
        {
            return new WildcardPattern(FileFilter).IsMatch(Path.GetFileName(path));
        }

        /// <summary>
        ///  Determine if the given path is already in the list.
        /// </summary>
        /// <param name="path">The path to inspect.</param>
        /// <returns>A value indicating whether the given path is unique from our list.</returns>
        private bool IsUnique(string path)
        {
            var items = ItemsSource as IList<T>;
            if (items == null)
            {
                return true;
            }

            return !items.Any(item =>
                StringComparer.CurrentCultureIgnoreCase.Equals(item.Text, path));
        }

        /// <summary>
        ///  Determine whether the drag distance is above a minimum threshold.
        /// </summary>
        /// <param name="dragDistance">A vector showing the horizontal and vertical distance of the drag action.</param>
        /// <returns>A value indicating whether the drag distance is over a minimum threshold.</returns>
        private bool IsOverThreshold(Vector dragDistance)
        {
            return Math.Abs(dragDistance.X) > SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(dragDistance.Y) > SystemParameters.MinimumVerticalDragDistance;
        }

        /// <summary>
        ///  Move a group of items in the data context collection from the given source list to the given target index.
        /// </summary>
        /// <param name="sourceIndex">The index of the item to move.</param>
        /// <param name="targetIndex">The index to which the item should be moved.</param>
        private void Move(int sourceIndex, int targetIndex)
        {
            var items = ItemsSource as IList<T>;
            if (items == null)
            {
                return;
            }
            
            int removeIndex = sourceIndex;
            int finalTarget = targetIndex;
            if (sourceIndex < targetIndex)
            {
                finalTarget++;
            }
            else
            {
                removeIndex++;
            }
            
            items.Insert(finalTarget, items[sourceIndex]);
            items.RemoveAt(removeIndex);
        }

        /// <summary>
        ///  Get the visual ancestor of the given type for the given child object.
        /// </summary>
        /// <typeparam name="AncestorT">The type of the ancestor to find.</typeparam>
        /// <param name="child">The child object from which to climb the visual tree.</param>
        /// <returns>
        ///  The nearest visual tree ancestor of <see cref="child"/> of the given type,
        ///  or null if none found.
        /// </returns>
        private AncestorT FindVisualAncestor<AncestorT>(DependencyObject child)
            where AncestorT : DependencyObject
        {
            DependencyObject parentObject;
            while ((parentObject = VisualTreeHelper.GetParent(child)) != null)
            {
                AncestorT parent = parentObject as AncestorT;
                if (parent != null)
                {
                    return parent;
                }

                child = parentObject;
            }

            return null;
        }

        /// <summary>
        ///  ListBoxItem child class to defer selection behavior.
        /// </summary>
        private class DeferredSelectionListItem : ListBoxItem
        {
            /// <summary>
            ///  Indicates whether selection should be deferred in case of starting a drag.
            /// </summary>
            private bool shouldDeferSelection = false;

            /// <summary>
            ///  Defer selection behavior in case of drag start.
            /// </summary>
            /// <param name="e">The mouse event arguments, including a click count.</param>
            protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
            {
                if (e.ClickCount == 1 && IsSelected)
                {
                    shouldDeferSelection = true;
                }
                else
                {
                    base.OnMouseLeftButtonDown(e);
                }
            }

            /// <summary>
            ///  Complete click action (including the mouse down action if deferred).
            /// </summary>
            /// <param name="e">The event arguments (passed through).</param>
            protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
            {
                if (shouldDeferSelection)
                {
                    try
                    {
                        base.OnMouseLeftButtonDown(e);
                    }
                    finally
                    {
                        shouldDeferSelection = false;
                    }
                }

                base.OnMouseLeftButtonUp(e);
            }
        }
    }
}
