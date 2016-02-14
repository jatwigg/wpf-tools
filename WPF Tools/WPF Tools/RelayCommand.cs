using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPF_Tools
{
    /// <summary>
    /// A Command that can be bound to the UI.
    /// </summary>
    public abstract class RelayCommand : ICommand
    {
        public abstract event EventHandler CanExecuteChanged;

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);
    }

    /// <summary>
    /// Don't need to add this to VM's, use base class RelayCommand instead. Abstract base class that all Command's inherit from.
    /// </summary>
    /// <typeparam name="ActionType">Type of Action (or Func) that the method of work will be.</typeparam>
    public abstract class AbstractRelayCommand<ActionType> : RelayCommand
    {
        public override event EventHandler CanExecuteChanged = delegate { };

        protected ActionType _commandAction;
        private bool _canExecute = true;

        public AbstractRelayCommand(ActionType action)
        {
            _commandAction = action;
        }

        public override bool CanExecute(object parameter)
        {
            return CanExecuteProp;
        }

        public abstract void Append(ActionType action);

        public abstract void Prepend(ActionType action);

        /// <summary>
        /// Set's if the command can execute or not. Will disable/enable button if command is bound to button.
        /// </summary>
        public bool CanExecuteProp
        {
            get { return _canExecute; }
            set
            {
                _canExecute = value;
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// A class that implements ICommand in it's simplest form. Lamda/pass-in an Action into the constructor and that will be 
    /// executed when the button is pressed.
    /// The command passes itself into the action as a parameter so that it is possible to enable/disable the button if need be.
    /// </summary>
    public class SimpleRelayCommand : AbstractRelayCommand<Action<SimpleRelayCommand>>
    {
        public SimpleRelayCommand(Action<SimpleRelayCommand> action) : base(action) { }

        public override void Execute(object parameter)
        {
            _commandAction.Invoke(this);
        }

        public override void Append(Action<SimpleRelayCommand> action)
        {
            var oldAction = _commandAction;
            _commandAction = (cmd) => { oldAction.Invoke(this); action.Invoke(this); };
        }

        public override void Prepend(Action<SimpleRelayCommand> action)
        {
            var oldAction = _commandAction;
            _commandAction = (cmd) => { action.Invoke(this); oldAction.Invoke(this); };
        }
    }

    /// <summary>
    /// A class that implements ICommand in it's simplest form. Lamda/pass-in an Action into the constructor and that will 
    /// be executed when the button is pressed.
    /// The command passes itself into the action as a parameter so that it is possible to enable/disable the button if need be.
    /// This Type also passes in the bound CommandParameter and assumes it's of type 'T'.
    /// </summary>
    public class SimpleRelayCommand<T> : AbstractRelayCommand<Action<SimpleRelayCommand<T>, T>>
    {
        public SimpleRelayCommand(Action<SimpleRelayCommand<T>, T> action) : base(action) { }

        public override void Execute(object parameter)
        {
            _commandAction.Invoke(this, (T)parameter);
        }

        public override void Append(Action<SimpleRelayCommand<T>, T> action)
        {
            var oldAction = _commandAction;
            _commandAction = (cmd, p) => { oldAction.Invoke(this, (T)p); action.Invoke(this, (T)p); };
        }

        public override void Prepend(Action<SimpleRelayCommand<T>, T> action)
        {
            var oldAction = _commandAction;
            _commandAction = (cmd, p) => { action.Invoke(this, (T)p); oldAction.Invoke(this, (T)p); };
        }
    }

    /// <summary>
    /// This is a relay command that will autodisable the button and then enable it after the work has completed. 
    /// This will enable too early if you await a task! Use Async version instead for that functionality.
    /// </summary>
    public class AutoCanExecuteRelayCommand : AbstractRelayCommand<Action>
    {
        public AutoCanExecuteRelayCommand(Action action) : base(action) { }

        public override void Execute(object parameter)
        {
            CanExecuteProp = false;
            try
            {
                _commandAction.Invoke();
            }
            finally
            {
                CanExecuteProp = true;
            }
        }

        public override void Append(Action action)
        {
            var oldAction = _commandAction;
            _commandAction = () => { oldAction.Invoke(); action.Invoke(); };
        }

        public override void Prepend(Action action)
        {
            var oldAction = _commandAction;
            _commandAction = () => { action.Invoke(); oldAction.Invoke(); };
        }
    }

    /// <summary>
    /// This is a relay command that will autodisable the button and then enable it after the work has completed. 
    /// This will enable too early if you await a task! Use Async version instead for that functionality.
    /// CommandParameter binding of type 'T' will be passed to your action.
    /// </summary>
    public class AutoCanExecuteRelayCommand<T> : AbstractRelayCommand<Action<T>>
    {
        public AutoCanExecuteRelayCommand(Action<T> action) : base(action) { }

        public override void Execute(object parameter)
        {
            CanExecuteProp = false;
            try
            {
                _commandAction.Invoke((T)parameter);
            }
            finally
            {
                CanExecuteProp = true;
            }
        }
        public override void Append(Action<T> action)
        {
            var oldAction = _commandAction;
            _commandAction = (p) => { oldAction.Invoke((T)p); action.Invoke((T)p); };
        }

        public override void Prepend(Action<T> action)
        {
            var oldAction = _commandAction;
            _commandAction = (p) => { action.Invoke((T)p); oldAction.Invoke((T)p); };
        }
    }

    /// <summary>
    /// For use with any command that uses async await in the function.
    /// </summary>
    public class AutoCanExecuteRelayCommandAsync : AbstractRelayCommand<Func<Task>>
    {
        public AutoCanExecuteRelayCommandAsync(Func<Task> actionTask) : base(actionTask) { }

        public override async void Execute(object parameter)
        {
            CanExecuteProp = false;
            try
            {
                await _commandAction.Invoke();
            }
            finally // this is some black magic right here
            {
                CanExecuteProp = true;
            }
        }

        public override void Append(Func<Task> action)
        {
            var oldAction = _commandAction;
            _commandAction = (async () =>
            {
                await oldAction.Invoke();
                await action.Invoke();
            });
        }

        public override void Prepend(Func<Task> action)
        {
            var oldAction = _commandAction;
            _commandAction = (async () =>
            {
                await action.Invoke();
                await oldAction.Invoke();
            });
        }
    }

    /// <summary>
    /// For use with any command that uses async await in the function. 
    /// Takes an instance of Type 'T' that is passed into the Execute via a CommandParameter binding and passed into function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AutoCanExecuteRelayCommandAsync<T> : AbstractRelayCommand<Func<T, Task>>
    {
        public AutoCanExecuteRelayCommandAsync(Func<T, Task> actionTask) : base(actionTask) { }

        public override async void Execute(object parameter)
        {
            CanExecuteProp = false;
            try
            {
                await _commandAction.Invoke((T)parameter);
            }
            finally
            {
                CanExecuteProp = true;
            }
        }

        public override void Append(Func<T, Task> action)
        {
            var oldAction = _commandAction;
            _commandAction = (async (p) =>
            {
                await oldAction.Invoke((T)p);
                await action.Invoke((T)p);
            });
        }

        public override void Prepend(Func<T, Task> action)
        {
            var oldAction = _commandAction;
            _commandAction = (async (p) =>
            {
                await action.Invoke((T)p);
                await oldAction.Invoke((T)p);
            });
        }
    }
}
