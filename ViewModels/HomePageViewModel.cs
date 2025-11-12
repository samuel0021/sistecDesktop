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

        public bool canApprove => App.LoggedUser != null && App.LoggedUser.IdPerfilUsuario.NivelAcesso >= 4;
        public bool CanOpenScaledTickets => App.LoggedUser.IdPerfilUsuario.Id >= 4;


        public TicketsViewModel TicketsViewModel { get; }

        private ObservableCollection<Chamado> _tickets;
        public ObservableCollection<Chamado> HomeTickets { get; set; } = new ObservableCollection<Chamado>();

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
        public ICommand ViewScaledTicketsCommand { get; }


        public HomePageViewModel(ApiClient apiClient, TicketsViewModel ticketsViewModel)
        {
            _apiClient = apiClient;
            TicketsViewModel = ticketsViewModel;
            _dialogService = new DialogService();

            Tickets = new ObservableCollection<Chamado>();
            LoadTicketsCommand = new AsyncRelayCommand(LoadTickets);
            OpenTicketCommand = new RelayCommand(OpenTicket);
            MyTicketsCommand = new AsyncRelayCommand(MyTickets);
            ApproveTicketsCommand = new RelayCommand(OpenApproveTickets);

            ViewScaledTicketsCommand = new RelayCommand(ViewScaledTickets);

            _ = LoadTickets();

            Console.WriteLine($"TicketsViewModel: {ticketsViewModel?.GetType().Name}");


            Console.WriteLine($"UserId: {App.LoggedUser?.Id}");
        }

        private async Task LoadTickets()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var list = await _apiClient.GetChamadosAsync();
                Tickets.Clear();

               

                foreach (var ticket in list)
                {
                    Tickets.Add(ticket);
                }
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

        private async Task MyTickets()
        {
            var vm = new MyTicketsViewModel(_apiClient);
            await vm.LoadTickets();
            _dialogService.ShowDialog(vm);
        }

        private void OpenApproveTickets()
        {
            // verifica o nível de acesso            

            if (App.LoggedUser.IdPerfilUsuario != null && App.LoggedUser.IdPerfilUsuario.Id < 3)
            {
                Console.WriteLine($"IdPerfilUsuario: {App.LoggedUser.IdPerfilUsuario}");
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

        // Método pra abrir o popup dos chamados escalados
        private void ViewScaledTickets()
        {
            var vm = new TicketsScaleViewModel(_apiClient);
            _dialogService.ShowDialog(vm);
        }
    }
}
