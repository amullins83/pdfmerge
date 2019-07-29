namespace PDFMergeDesktop
{
    using System;
    using System.Threading.Tasks;

    public class RelayCommandAsync : RelayCommand
    {
        private readonly Func<object, Task> func;
        
        public RelayCommandAsync(Func<object, Task> f) : this(f, null)
        {}

        public RelayCommandAsync(Func<object, Task> f, Predicate<object> pred) : base(null, pred)
        {
            func = f;
        }

        /// <summary>
        ///  Execute the command.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        public override void Execute(object parameter)
        {
            func.Invoke(parameter).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
