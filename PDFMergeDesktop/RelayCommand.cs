/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace PDFMergeDesktop
{
    using System;
    using System.Windows.Input;

    /// <summary>
    ///  An implementation of the <c>RelayCommand</c> pattern, with a method that allows
    ///  an owning view model to raise the <c>CanExecuteChanged</c> event without
    ///  calling <c>CommandManager.InvalidateRequerySuggested</c>.
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        ///  The action to be performed by the command.
        /// </summary>
        private Action<object> action;

        /// <summary>
        ///  The predicate determining whether the action can be performed.
        /// </summary>
        private Predicate<object> predicate;

        /// <summary>
        ///  Carry private delegate to avoid making user code call
        ///  <c>CommandManager.InvalidateRequerySuggested</c>.
        /// </summary>
        private EventHandler canExecuteChanged = delegate { };

        /// <summary>
        ///  Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="action">The action to perform when the command executes.</param>
        public RelayCommand(Action<object> action) : this(action, null)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="action">The action to perform when the command executes.</param>
        /// <param name="predicate">The predicate to determine whether the command can execute.</param>
        public RelayCommand(Action<object> action, Predicate<object> predicate)
        {
            this.action = action;
            this.predicate = predicate;
        }

        /// <summary>
        ///  Raised when the CanExecute property changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                canExecuteChanged += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                canExecuteChanged += value;
            }
        }

        /// <summary>
        ///  Determines whether the command can execute.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>A value indicating whether the command can be executed.</returns>
        public bool CanExecute(object parameter)
        {
            if (predicate == null)
            {
                return true;
            }

            return predicate.Invoke(parameter);
        }

        /// <summary>
        ///  Execute the command.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        public void Execute(object parameter)
        {
            action.Invoke(parameter);
        }

        /// <summary>
        ///  Raise the <c>CanExecuteChanged</c> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            canExecuteChanged(this, EventArgs.Empty);
        }
    }
}
