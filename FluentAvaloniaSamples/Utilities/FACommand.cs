using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FluentAvaloniaSamples.Utilities
{
    public class FACommand : ICommand
    {
        public FACommand(Action<object> executeMethod)
        {
            _executeMethod = executeMethod;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            _executeMethod.Invoke(parameter);
        }

        private Action<object> _executeMethod;
    }
}
