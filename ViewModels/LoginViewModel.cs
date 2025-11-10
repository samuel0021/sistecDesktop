using Newtonsoft.Json;
using sistecDesktop.Commands;
using sistecDesktop.Models; 
using sistecDesktop.Services;
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
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                MensagemErro = "Email ou senha inválidos.";
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

                // Chama a API
                var resultado = await _apiClient.LoginAsync(loginRequest);

                if (resultado != null && resultado.Success && resultado.Data != null && resultado.Data.User != null)
                {
                    // Login OK
                    App.LoggedUser = resultado.Data.User;

                    Console.WriteLine($"Usuário logado após login: {App.LoggedUser.NomeUsuario}");
                    Console.WriteLine($"Nome do Perfil: {App.LoggedUser.IdPerfilUsuario?.Nome}");
                    Console.WriteLine($"Nivel de acesso: {App.LoggedUser.IdPerfilUsuario?.NivelAcesso}");

                    Console.WriteLine(JsonConvert.SerializeObject(App.LoggedUser, Formatting.Indented));
                    _mainViewModel.SelectedViewModel = new HomeViewModel(_mainViewModel, _apiClient);
                    ((HomeViewModel)_mainViewModel.SelectedViewModel).NameCurrentUser = App.LoggedUser?.NomeUsuario ?? "";

                }
                else
                {
                    // Login inválido
                    MensagemErro = "Email ou senha inválidos.";

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
