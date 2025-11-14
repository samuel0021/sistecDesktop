using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace sistecDesktop.Commands
{
    public class AsyncRelayCommandWithParameter<T> : ICommand
    {
        // Delegate para armazenar o método assíncrono a ser executado. Recebe um parâmetro do tipo T
        private readonly Func<T, Task> _execute;
        // Delegate opcional para determinar se o comando pode ser executado. Também recebe T
        private readonly Func<T, bool> _canExecute;
        // Flag usada para evitar execuções concorrentes; evita duplo clique ou múltiplas execuções
        private bool _isExecuting;

        // Construtor recebe o método pra executar e a regra opcional de habilitação
        public AsyncRelayCommandWithParameter(Func<T, Task> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Evento usado pelo WPF para atualizar o estado do botão na interface automaticamente
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Define se o comando pode ser executado
        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute((T)parameter));
        }

        // Executa o comando de fato, de forma assíncrona
        // Desabilita o botão durante a execução, e habilita novamente no final
        public async void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    RaiseCanExecuteChanged(); // Atualiza a UI (desabilita botão)
                    // Executa o método principal
                    await _execute((T)parameter);
                }
                finally
                {
                    _isExecuting = false;
                    RaiseCanExecuteChanged(); // Atualiza a UI (reativa botão)
                }
            }
        }

        // Força atualização do estado de habilitado/desabilitado dos comandos na UI WPF.
        private void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
