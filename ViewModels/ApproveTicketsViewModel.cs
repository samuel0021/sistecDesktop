using sistecDesktop.Commands;
using sistecDesktop.Models;
using sistecDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
    public class ApproveTicketsViewModel : TicketsViewModel
    {
        private bool _showRejectModal;
        private string _motivoRejeicao;

        public bool ShowRejectModal
        {
            get => _showRejectModal;
            set { _showRejectModal = value; OnPropertyChanged(nameof(ShowRejectModal)); }
        }

        public string MotivoRejeicao
        {
            get => _motivoRejeicao;
            set { _motivoRejeicao = value; OnPropertyChanged(nameof(MotivoRejeicao)); }
        }

        // Comandos
        public ICommand ApproveCommand { get; }
        public ICommand OpenRejectModalCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand CancelRejectCommand { get; }

        // Construtor
        public ApproveTicketsViewModel(ApiClient apiClient) : base(apiClient)
        {
            PopupTitle = "Aprovar Chamados";
            PopupWidth = 1000;
            PopupHeight = 450;

            ApproveCommand = new RelayCommandWithParameter(async (param) => await ApproveTicket(param));
            OpenRejectModalCommand = new RelayCommandWithParameter(OpenRejectModal);
            RejectCommand = new AsyncRelayCommand(RejectTicket);
            CancelRejectCommand = new RelayCommand(CancelReject);

            _ = LoadTickets();
        }

        // Sobrescrita pra trazer só os chamados pendentes
        public override async Task LoadTickets()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var lista = await _apiClient.GetPendingTickets();
                Tickets.Clear();
                foreach (var chamado in lista)
                    Tickets.Add(chamado);
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

        private async Task ApproveTicket(object parameter)
        {
            if (parameter is Chamado chamado)
            {
                var result = MessageBox.Show(
                    $"Tem certeza que deseja aprovar o chamado #{chamado.Id}?\nTítulo: {chamado.Title}",
                    "Confirmar Aprovação",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        IsLoading = true;
                        var success = await _apiClient.AprovarChamadoAsync(chamado.Id);
                        if (success)
                        {
                            MessageBox.Show("Chamado aprovado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadTickets();
                        }
                        else
                        {
                            MessageBox.Show("Erro ao aprovar chamado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally { IsLoading = false; }
                }
            }
        }

        // Abre a tela de motivo da rejeição
        private void OpenRejectModal(object parameter)
        {
            if (parameter is Chamado chamado)
            {
                SelectedTicket = chamado;
                MotivoRejeicao = string.Empty;
                ShowRejectModal = true;
            }
        }

        private async Task RejectTicket()
        {
            if (SelectedTicket == null) return;

            if (string.IsNullOrWhiteSpace(MotivoRejeicao) || MotivoRejeicao.Trim().Length < 10)
            {
                MessageBox.Show("Motivo da rejeição deve ter pelo menos 10 caracteres.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                ShowRejectModal = false;
                var success = await _apiClient.RejeitarChamadoAsync(SelectedTicket.Id, MotivoRejeicao.Trim());
                if (success)
                {
                    MessageBox.Show("Chamado rejeitado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    SelectedTicket = null;
                    MotivoRejeicao = string.Empty;
                    await LoadTickets();
                }
                else
                {
                    MessageBox.Show("Erro ao rejeitar chamado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        private void CancelReject()
        {
            ShowRejectModal = false;
            SelectedTicket = null;
            MotivoRejeicao = string.Empty;
        }
    }

}
