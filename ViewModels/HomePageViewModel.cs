using sistecDesktop.Commands;
using sistecDesktop.Models;
using sistecDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        private readonly ApiClient _apiClient;
        private ObservableCollection<Chamado> _tickets;
        private bool _isLoading;
        private string _errorMessage;

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

        public ICommand LoadTicketsCommand { get; }

        public HomePageViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            Tickets = new ObservableCollection<Chamado>();
            LoadTicketsCommand = new AsyncRelayCommand(LoadTickets);

            // Carregar automaticamente os últimos chamados
            _ = LoadTickets();
        }

        private async Task LoadTickets()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var list = await _apiClient.GetChamadosAsync();

                // Pegar apenas os últimos 10 chamados
                var ultimosChamados = list.OrderByDescending(c => c.CreatedAt).Take(10);

                Tickets.Clear();
                foreach (var ticket in ultimosChamados)
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
    }
}
