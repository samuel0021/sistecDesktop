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
        private readonly TicketsViewModel _ticketsViewModel;
        private readonly ApiClient _apiClient;
        private string _paginaSelecionada;
        private UserControl _currentContent;
        private readonly IDialogService _dialogService;


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

        public HomeViewModel(MainViewModel mainViewModel, ApiClient apiClient)
        {
            _mainViewModel = mainViewModel;
            _apiClient = apiClient;

            _ticketsViewModel = new TicketsViewModel(_apiClient);
            _dialogService = new DialogService();

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
                    var homePageViewModel = new HomePageViewModel(_apiClient, _ticketsViewModel);
                    var homePage = new Home { DataContext = homePageViewModel };
                    CurrentContent = homePage;
                    break;
                case "Dashboard":
                    CurrentContent = new Dashboard();
                    break;
                case "Chamados":
                    var ticketsPage = new Tickets { ViewModel = _ticketsViewModel };
                    CurrentContent = ticketsPage;
                    break;
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

        public async void ExecutarLogout()
        {
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