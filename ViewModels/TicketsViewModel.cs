using sistecDesktop.Commands;
using sistecDesktop.Models;
using sistecDesktop.Services;
using sistecDesktop.Views.Pages;
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
    public class TicketsViewModel : BaseViewModel
    {
        private readonly ApiClient _apiClient;
        private ObservableCollection<Chamado> _tickets;  // ← Inglês
        private bool _isLoading;
        private string _errorMessage;
        private Chamado _selectedTicket;

        #region Encapsulamentos
        public ObservableCollection<Chamado> Tickets  // ← MUDAR para inglês
        {
            get { return _tickets; }
            set
            {
                _tickets = value;
                OnPropertyChanged(nameof(Tickets));  // ← MUDAR
            }
        }

        public Chamado SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;
                OnPropertyChanged(nameof(SelectedTicket));
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        #endregion

        public ICommand LoadTicketsCommand { get; }

        public TicketsViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            Tickets = new ObservableCollection<Chamado>();  // ← MUDAR
            LoadTicketsCommand = new AsyncRelayCommand(LoadTickets);
            _ = LoadTickets();
        }

        private async Task LoadTickets()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var list = await _apiClient.GetChamadosAsync();
                Tickets.Clear();  // ← MUDAR

                foreach (var ticket in list)
                {
                    Tickets.Add(ticket);  // ← MUDAR
                }
            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "Sessão expirada. Faça login novamente.";
                MessageBox.Show(ErrorMessage, "Erro de Autenticação", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao carregar chamados: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
