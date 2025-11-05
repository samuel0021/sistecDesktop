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
using sistecDesktop.Services;
using sistecDesktop.Models; 

namespace sistecDesktop.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly ApiClient _apiClient;

        // Propriedades para binding com a View
        private string _email;
        private string _senha;
        private string _mensagemErro;
        private bool _isLoading;

        #region Encapsulamentos
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string Senha
        {
            get => _senha;
            set
            {
                _senha = value;
                OnPropertyChanged(nameof(Senha));
            }
        }

        public string MensagemErro
        {
            get => _mensagemErro;
            set
            {
                _mensagemErro = value;
                OnPropertyChanged(nameof(MensagemErro));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
        #endregion

        public ICommand EsqueciSenhaCommand { get; }
        public ICommand LoginCommand { get; }

        public LoginViewModel(MainViewModel mainViewModel, ApiClient apiClient)
        {
            _mainViewModel = mainViewModel;
            _apiClient = apiClient;

            EsqueciSenhaCommand = new RelayCommand(() => ForgotPassWindow.Mostrar());
 
            LoginCommand = new AsyncRelayCommand(ExecutarLoginAsync);
        }

        // Método atualizado com chamada à API
        public async Task ExecutarLoginAsync()
        {
            // Limpar mensagem de erro anterior
            MensagemErro = string.Empty;

            // Validações básicas -- ARRUMAR DEPOIS
            if (string.IsNullOrWhiteSpace(Email))
            {
                MensagemErro = "Por favor, informe o email.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Senha))
            {
                MensagemErro = "Por favor, informe a senha.";
                return;
            }

            IsLoading = true;  // Mostrar loading na tela

            try
            {
                // Criar requisição de login
                var loginRequest = new LoginRequest
                {
                    Email = Email,
                    Password = Senha
                };

                // Chamar a API
                var resultado = await _apiClient.LoginAsync(loginRequest);

                if (resultado.Success)
                {
                    // Login bem-sucedido!

                    // Você pode salvar os dados do usuário na MainViewModel se precisar
                    // _mainViewModel.UsuarioLogado = resultado.Data.User;

                    // Limpar os campos
                    Email = string.Empty;
                    Senha = string.Empty;

                    // Armazenar usuário
                    App.LoggedUser = resultado.Data.User;

                    // Navegar para a Home
                    _mainViewModel.SelectedViewModel = new HomeViewModel(_mainViewModel, _apiClient);
                }              
            }
            catch (Exception ex)
            {
                // Erro de conexão ou outro erro
                MensagemErro = $"Erro ao conectar com o servidor: {ex.Message}";
            }
            finally 
            {
                IsLoading = false;  // Esconder loading
                Console.WriteLine($"Usuário logado após login: {App.LoggedUser.IdPerfilUsuario}");
            }
        }

        
        // Manter método antigo por compatibilidade (se precisar)
        public void ExecutarLogin()
        {
            // Chamar a versão async
            _ = ExecutarLoginAsync();
        }
    }
}
