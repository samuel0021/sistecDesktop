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
        private ObservableCollection<Chamado> _tickets;
        private bool _isLoading;
        private string _errorMessage;
        private Chamado _selectedTicket;

        #region Encapsulamentos
        public ObservableCollection<Chamado> Tickets
        {
            get { return _tickets; }
            set
            {
                _tickets = value;
                OnPropertyChanged(nameof(Tickets));
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
        public ICommand ViewTicketCommand { get; }

        public TicketsViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            Tickets = new ObservableCollection<Chamado>(); 
            LoadTicketsCommand = new AsyncRelayCommand(LoadTickets);
            ViewTicketCommand = new RelayCommandWithParameter(ViewTicket);

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

                foreach (var ticket in list)
                {
                    Tickets.Add(ticket);
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

        private async void ViewTicket(object parameter)
        {
            if (parameter is Chamado ticket)
            {
                try
                {
                    // Busca o chamado atualizado pelo ID
                    var updatedTicket = await _apiClient.GetChamadoByIdAsync(ticket.Id);

                    MessageBox.Show(
                        $"ID: {updatedTicket.Id}\n" +
                        $"Título: {updatedTicket.Title}\n" +
                        $"Descrição: {updatedTicket.Description}\n" +
                        $"Status: {updatedTicket.Status}\n" +
                        $"Usuário: {updatedTicket.UsuarioAbertura}\n" +
                        $"Abertura: {updatedTicket.CreatedAt:dd/MM/yyyy HH:mm}",
                        "Detalhes do Chamado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Sessão expirada. Faça login novamente.", "Erro de Autenticação", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar detalhes do chamado: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
