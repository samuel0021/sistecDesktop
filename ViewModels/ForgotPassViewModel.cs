using sistecDesktop.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
    public class ForgotPassViewModel : BaseViewModel
    {
        public ICommand CloseCommand { get; }

        public ForgotPassViewModel()
        {
            CloseCommand = new RelayCommand(CloseWindow);
        }

        private void CloseWindow()
        {
            // Encontra a janela ForgotPassWindow que está aberta
            var janela = Application.Current.Windows.OfType<ForgotPassWindow>()
                .FirstOrDefault(w => w.IsActive);

            janela?.Close();
        }
    }    
}
