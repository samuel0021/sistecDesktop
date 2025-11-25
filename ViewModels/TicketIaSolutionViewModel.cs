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
    public class TicketIaSolutionViewModel : BasePopupViewModel
    {
        private readonly ApiClient _apiClient;
        private readonly Chamado _chamado;

        private RespostaIA _solucao;
        private bool _isLoading;
        private bool _isSendingFeedback;

        #region Encapsulamentos
        public RespostaIA Solucao
        {
            get => _solucao;
            set { _solucao = value; OnPropertyChanged(nameof(Solucao)); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public bool IsSendingFeedback
        {
            get => _isSendingFeedback;
            set { _isSendingFeedback = value; OnPropertyChanged(nameof(IsSendingFeedback)); }
        }
        #endregion

        // Condição para poder aplicar feedback da IA
        public bool CanFeedback => App.LoggedUser != null && _chamado != null && App.LoggedUser.Id == _chamado.UserId && string.IsNullOrEmpty(Solucao?.FeedbackUsuario);

        // Comandos
        public ICommand FeedbackOkCommand { get; }
        public ICommand FeedbackErrorCommand { get; }

        // Construtor
        public TicketIaSolutionViewModel(ApiClient apiClient, Chamado chamado)
        {
            _apiClient = apiClient;
            _chamado = chamado;

            PopupTitle = $"Solução IA - Chamado #{chamado.Id}";
            PopupWidth = 700;
            PopupHeight = 600;

            FeedbackOkCommand = new AsyncRelayCommand(() => EnviarFeedback("DEU_CERTO"));
            FeedbackErrorCommand = new AsyncRelayCommand(() => EnviarFeedback("DEU_ERRADO"));

            _ = LoadSolucao();
        }

        private async Task LoadSolucao()
        {
            try
            {
                IsLoading = true;
                Solucao = await _apiClient.GetSolucaoIaAsync(_chamado.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar solução da IA: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EnviarFeedback(string feedback)
        {
            if (Solucao == null) return;

            try
            {
                IsSendingFeedback = true;
                await _apiClient.EnviarFeedbackIaAsync(Solucao.ChamadoId, feedback);

                if (feedback == "DEU_CERTO")
                    MessageBox.Show("Ótimo! Seu chamado foi marcado como resolvido.", "Sucesso",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Seu chamado foi encaminhado para um analista humano.", "Informação",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao enviar feedback: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSendingFeedback = false;
            }
        }
    }
}
