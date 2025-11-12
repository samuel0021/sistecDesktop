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
    public class TicketsScaleViewModel : TicketsViewModel
    {
        private ChamadoEscalado _selectedEscalado;
        public ChamadoEscalado SelectedEscalado
        {
            get => _selectedEscalado;
            set
            {
                _selectedEscalado = value;
                OnPropertyChanged(nameof(SelectedEscalado));
            }
        }

        public ObservableCollection<ChamadoEscalado> ChamadosEscalados { get; } = new ObservableCollection<ChamadoEscalado>();
        public ICommand ResolveScaledTicketCommand { get; }

        // Construtor
        public TicketsScaleViewModel(ApiClient apiClient) : base(apiClient)
        {
            PopupTitle = "Chamados Escalados";
            PopupWidth = 1010;
            

            ResolveScaledTicketCommand = new AsyncRelayCommandWithParameter<ChamadoEscalado>(ResolverChamadoEscalado);

            _ = CarregarChamadosEscalados();
        }
        public async Task CarregarChamadosEscalados()
        {
            ChamadosEscalados.Clear();
            var lista = await _apiClient.GetChamadosEscaladosAsync();
            foreach (var chamado in lista) ChamadosEscalados.Add(chamado);

            foreach (var chamado in lista) Console.WriteLine(chamado);
        }
        private async Task ResolverChamadoEscalado(ChamadoEscalado chamado)
        {
            if (chamado == null)
                return;

            var confirm = await Task.Run(() =>
                MessageBox.Show(
                    $"Tem certeza que deseja resolver o chamado?",
                    "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning)
            );

            if (confirm != MessageBoxResult.Yes) return;

            var vmMotivo = new MotivoResolucaoViewModel(chamado.IdChamado, chamado.DescricaoCategoriaChamado, chamado.DescricaoProblemaChamado);
            vmMotivo.OnClose = async (salvou, motivo) =>
            {
                if (salvou && !string.IsNullOrWhiteSpace(motivo) && motivo.Length >= 20)
                {
                    try
                    {
                        await _apiClient.ResolverChamadoEscaladoAsync(chamado.IdChamado, motivo);
                        MessageBox.Show("Chamado resolvido com sucesso!");
                        await CarregarChamadosEscalados();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao resolver chamado escalado: {ex.Message}");
                    }
                }
            };
            _dialogService.ShowDialog(vmMotivo);
        }
    }
}
