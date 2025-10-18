using sistecDesktop.Commands;
using sistecDesktop.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;

        public ICommand EsqueciSenhaCommand { get; }
        public ICommand LoginCommand { get; }

        public LoginViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            EsqueciSenhaCommand = new RelayCommand(() => ForgotPassWindow.Mostrar());
            LoginCommand = new LoginCommand(this);
        }

        // Método público para ser chamado pelo LoginCommand
        public void ExecutarLogin()
        {
            // Aqui você pode adicionar validações depois
            // Por enquanto, apenas navega para a Home
            _mainViewModel.SelectedViewModel = new HomeViewModel(_mainViewModel);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
