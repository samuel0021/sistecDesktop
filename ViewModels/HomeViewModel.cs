using sistecDesktop.Commands;
using sistecDesktop.Views.Pages;
using sistecDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly ApiClient _apiClient;
        private string _paginaSelecionada;
        private UserControl _currentContent;

        public ICommand LogoutCommand { get; }
        public ICommand SelecionarPaginaCommand { get; }

        public UserControl CurrentContent
        {
            get { return _currentContent; }
            set
            {
                if (_currentContent != value)
                {
                    _currentContent = value;
                    OnPropertyChanged(nameof(CurrentContent));
                }
            }
        }

        public string PaginaSelecionada
        {
            get => _paginaSelecionada;
            set
            {
                if (_paginaSelecionada != value)
                {
                    _paginaSelecionada = value;
                    OnPropertyChanged(nameof(PaginaSelecionada));
                    OnPropertyChanged(nameof(TagHome));
                    OnPropertyChanged(nameof(TagDashboard));
                    OnPropertyChanged(nameof(TagChamados));
                    OnPropertyChanged(nameof(TagUsuarios));
                    LoadContent(value);
                }
            }
        }

        public string TagHome => PaginaSelecionada == "Home" ? "Selected" : null;
        public string TagDashboard => PaginaSelecionada == "Dashboard" ? "Selected" : null;
        public string TagChamados => PaginaSelecionada == "Chamados" ? "Selected" : null;
        public string TagUsuarios => PaginaSelecionada == "Usuarios" ? "Selected" : null;

        public HomeViewModel(MainViewModel mainViewModel, ApiClient apiClient)  // ← MODIFICAR
        {
            _mainViewModel = mainViewModel;
            _apiClient = apiClient;

            LogoutCommand = new LogoutCommand(this);

            SelecionarPaginaCommand = new RelayCommandWithParameter(
                parameter => PaginaSelecionada = parameter?.ToString()
            );

            PaginaSelecionada = "Home";
        }

        private void LoadContent(string nomePagina)
        {
            switch (nomePagina)
            {
                case "Home":
                    CurrentContent = new Home();
                    break;
                case "Dashboard":
                    CurrentContent = new Dashboard();
                    break;
                /*case "Chamados":
                    var ticketsViewModel = new TicketsViewModel(_apiClient);
                    var ticketsPage = new Tickets { DataContext = ticketsViewModel };
                    CurrentContent = ticketsPage;
                    break;*/
                case "Usuarios":
                    var usersViewModel = new UsersViewModel(_apiClient);
                    var usersPage = new Users { DataContext = usersViewModel };
                    CurrentContent = usersPage;
                    break;
                default:
                    CurrentContent = new Home();
                    break;
            }
        }

        public async void ExecutarLogout()  // ← MODIFICAR para async
        {
            try
            {
                // Fazer logout na API
                await _apiClient.LogoutAsync();  // ← ADICIONAR

                // Limpar cookies locais
                _apiClient.Logout();  // ← ADICIONAR

                // Voltar para tela de login
                _mainViewModel.SelectedViewModel = new LoginViewModel(_mainViewModel, _apiClient);  // ← MODIFICAR
            }
            catch (Exception ex)
            {
                // Log do erro (opcional)
                System.Diagnostics.Debug.WriteLine($"Erro no logout: {ex.Message}");

                // Mesmo com erro, volta pra tela de login
                _mainViewModel.SelectedViewModel = new LoginViewModel(_mainViewModel, _apiClient);
            }
        }
    }
}