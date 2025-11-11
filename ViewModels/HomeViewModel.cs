using sistecDesktop.Commands;
using sistecDesktop.Services;
using sistecDesktop.Views.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly TicketsViewModel _ticketsViewModel;
        private readonly ApiClient _apiClient;
        private string _paginaSelecionada;
        private bool _menuUsuariosExpandido;
        private UserControl _currentContent;
        private readonly IDialogService _dialogService;

        public bool canAccessDashboard => App.LoggedUser != null && App.LoggedUser.IdPerfilUsuario.NivelAcesso >= 4; 
        public bool canAccessUsers => App.LoggedUser != null && App.LoggedUser.IdPerfilUsuario.NivelAcesso >= 3;

        private string _nameCurrentUser;
        public string NameCurrentUser
        {
            get => _nameCurrentUser;
            set { _nameCurrentUser = value; OnPropertyChanged(nameof(NameCurrentUser)); }
        }


        public ICommand UsersMenuCommand { get; }

        public ICommand LogoutCommand { get; }
        public ICommand SelecionarPaginaCommand { get; }

        public bool MenuUsuariosExpandido
        {
            get => _menuUsuariosExpandido;
            set { _menuUsuariosExpandido = value; OnPropertyChanged(nameof(MenuUsuariosExpandido)); }
        }

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
                    OnPropertyChanged(nameof(TagUsuariosAtivos));
                    OnPropertyChanged(nameof(TagUsuariosDeletados));
                    LoadContent(value);
                }
            }
        }

        public string TagHome => PaginaSelecionada == "Home" ? "Selected" : null;
        public string TagDashboard => PaginaSelecionada == "Dashboard" ? "Selected" : null;
        public string TagChamados => PaginaSelecionada == "Chamados" ? "Selected" : null;
        public string TagUsuariosAtivos => PaginaSelecionada == "UsuariosAtivos" ? "Selected" : null;
        public string TagUsuariosDeletados => PaginaSelecionada == "UsuariosDeletados" ? "Selected" : null;

        public HomeViewModel(MainViewModel mainViewModel, ApiClient apiClient)
        {
            _mainViewModel = mainViewModel;
            _apiClient = apiClient;

            _ticketsViewModel = new TicketsViewModel(_apiClient);
            _dialogService = new DialogService();

            LogoutCommand = new LogoutCommand(this);
            UsersMenuCommand = new RelayCommand(UsersMenu);

            SelecionarPaginaCommand = new RelayCommandWithParameter(
                parameter => PaginaSelecionada = parameter?.ToString()
            );

            PaginaSelecionada = "Home";
        }

        private void UsersMenu()
        {
            MenuUsuariosExpandido = !MenuUsuariosExpandido;
        }


        private void LoadContent(string nomePagina)
        {
            switch (nomePagina)
            {
                case "Home":
                    var homePageViewModel = new HomePageViewModel(_apiClient, _ticketsViewModel);
                    var homePage = new Home { DataContext = homePageViewModel };
                    CurrentContent = homePage;
                    break;
                case "Dashboard":
                    var dashboardViewModel = new DashboardViewModel(_apiClient);
                    var dashboardPage = new Dashboard { DataContext = dashboardViewModel};
                    CurrentContent = dashboardPage;
                    break;
                case "Chamados":
                    var ticketsPage = new Tickets { ViewModel = _ticketsViewModel };
                    CurrentContent = ticketsPage;
                    break;
                case "UsuariosAtivos":
                    var usersViewModel = new UsersViewModel(_apiClient);
                    var activeUsersPage = new Users { DataContext = usersViewModel };
                    CurrentContent = activeUsersPage;
                    break;
                case "UsuariosDeletados":
                    var restoreViewModel = new RestoreUsersViewModel(_apiClient);
                    var restoreUsersPage = new RestoreUsersView { DataContext = restoreViewModel };
                    CurrentContent = restoreUsersPage;
                    break;
                default:
                    CurrentContent = new Home();
                    break;
            }
        }

        public async void ExecutarLogout()
        {
            var confirm = MessageBox.Show(
                $"Tem certeza que deseja sair?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                // Fazer logout na API
                await _apiClient.LogoutAsync();

                // Limpar cookies locais
                _apiClient.Logout();

                // Voltar para tela de login
                _mainViewModel.SelectedViewModel = new LoginViewModel(_mainViewModel, _apiClient);
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