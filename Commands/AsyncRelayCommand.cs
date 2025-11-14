using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace sistecDesktop.Commands
{
    public class AsyncRelayCommand : ICommand
    {
        // Delegate para guardar o método a ser executado (deve retornar Task para ser assíncrono)
        private readonly Func<Task> _execute;
        // Delegate opcional que define se o comando pode ou não ser executado no momento
        private readonly Func<bool> _canExecute;
        // Flag interna que controla se o comando está executando no momento
        private bool _isExecuting;

        // Construtor
        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Permite que o WPF atualize automaticamente o estado dos botões e outras interfaces conectadas ao comando
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute());
        }

        public async void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    RaiseCanExecuteChanged(); // Atualiza a UI (desativa botão)

                    await _execute();
                }
                finally
                {
                    _isExecuting = false;
                    RaiseCanExecuteChanged(); // Atualiza a UI (reativa botão)
                }
            }
        }

        // Revalidar se os comandos podem ser executados, disparando o evento de atualização na UI
        private void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
