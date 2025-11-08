using sistecDesktop.Commands;
using sistecDesktop.Models;
using sistecDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        private readonly ApiClient _apiClient;
        private readonly IDialogService _dialogService;

        public TicketsViewModel TicketsViewModel { get; }

        private ObservableCollection<Chamado> _tickets;
        private bool _isLoading;
        private string _errorMessage;

        #region Encapsulamentos
        public ObservableCollection<Chamado> Tickets
        {
            get => _tickets;
            set
            {
                _tickets = value;
                OnPropertyChanged(nameof(Tickets));
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
#endregion

        public ICommand LoadTicketsCommand { get; }
        public ICommand OpenTicketCommand { get; }
        public ICommand MyTicketsCommand { get; }
        public ICommand ApproveTicketsCommand { get; }

        public HomePageViewModel(ApiClient apiClient, TicketsViewModel ticketsViewModel)
        {
            _apiClient = apiClient;
            TicketsViewModel = ticketsViewModel;
            _dialogService = new DialogService();

            Tickets = new ObservableCollection<Chamado>();
            LoadTicketsCommand = new AsyncRelayCommand(LoadTickets);
            OpenTicketCommand = new RelayCommand(OpenTicket);
            MyTicketsCommand = new RelayCommand(MyTickets);
            ApproveTicketsCommand = new RelayCommand(OpenApproveTickets);
  
            _ = LoadTickets();

        }

        private async Task LoadTickets()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var list = await _apiClient.GetChamadosAsync();
                Tickets.Clear();

                // Pega apenas os últimos 10 chamados
                // var ultimosChamados = list.OrderByDescending(c => c.CreatedAt).Take(10);

                foreach (var ticket in list)
                {
                    Tickets.Add(ticket);
                }

                // Força o refresh da view, se necessário
                // CollectionViewSource.GetDefaultView(Tickets).Refresh();

            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "Sessão expirada. Faça login novamente.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao carregar chamados: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenTicket()
        {
            var vm = new OpenTicketViewModel(_apiClient);

            //callback para atualizar a lista
            vm.OnChamadoCriado = () => TicketsViewModel.LoadTicketsCommand.Execute(null);

            _dialogService.ShowDialog(vm);

        }

        private void MyTickets()
        {
            var vm = new MyTicketsViewModel(_apiClient);

            _dialogService.ShowDialog(vm);
        }

        private void OpenApproveTickets()
        {
            // verifica o nível de acesso
            var perfil = App.PerfisAcesso.FirstOrDefault(p => p.Id == App.LoggedUser.IdPerfilUsuario);

            if (App.LoggedUser?.IdPerfilUsuario < 3)
            {
                Console.WriteLine($"IdPerfilUsuario: {App.LoggedUser.MatriculaAprovador}");
                MessageBox.Show(
                    "Apenas gestores e administradores podem aprovar chamados.",
                    "Acesso Negado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var vm = new ApproveTicketsViewModel(_apiClient);
            _dialogService.ShowDialog(vm);
        }
    }
}
