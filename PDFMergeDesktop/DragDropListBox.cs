namespace PDFMergeDesktop
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    ///  A list box class that allows reordering of items via drag and drop.
    /// </summary>
    /// <typeparam name="T">The type of item displayed in the list.</typeparam>
    public class DragDropListBox<T> : ListBox
        where T : class
    {
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
            AllowDrop = true;
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
            if (e.LeftButton == MouseButtonState.Pressed && InBounds(dragDistance))
            {
                var item = FindVisualAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
                if (item != null)
                {
                    DragDrop.DoDragDrop(item, item.DataContext, DragDropEffects.Move);
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
            if (sender is ListBoxItem)
            {
                var source = e.Data.GetData(typeof(T)) as T;
                var target = ((ListBoxItem)sender).DataContext as T;
                int sourceIndex = Items.IndexOf(source);
                int targetIndex = Items.IndexOf(target);
                Move(sourceIndex, targetIndex);
            }
        }

        /// <summary>
        ///  Determine whether the drag distance is within accepted bounds.
        /// </summary>
        /// <param name="dragDistance">A vector showing the horizontal and vertical distance of the drag action.</param>
        /// <returns>A value indicating whether the drag distance is within accepted bounds.</returns>
        private bool InBounds(Vector dragDistance)
        {
            return Math.Abs(dragDistance.X) > SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(dragDistance.Y) > SystemParameters.MinimumVerticalDragDistance;
        }

        /// <summary>
        ///  Move an item in the data context collection from the given source index to the given target index.
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

            var source = items[sourceIndex];
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

            items.Insert(finalTarget, source);
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
    }
}
