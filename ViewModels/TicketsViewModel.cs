using sistecDesktop.Commands;
using sistecDesktop.Models;
using sistecDesktop.Services;
using sistecDesktop.Views.Pages;
using sistecDesktop.Views.Popups;
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
    public class TicketsViewModel : BasePopupViewModel
    {
        protected readonly ApiClient _apiClient;
        private List<ChamadoDatabase> _allTickets;
        private ObservableCollection<Chamado> _tickets;

        private bool _isLoading;
        private string _errorMessage;
        private Chamado _selectedTicket;
        protected readonly IDialogService _dialogService;

        // Variável pra filtro de busca
        private string _searchTerm;

        #region Encapsulamentos
        public List<ChamadoDatabase> AllTickets
        {
            get => _allTickets;
            set
            {
                _allTickets = value;
                FiltrarChamados(_searchTerm); // Atualiza filtro ao receber nova lista
            }
        }

        public ObservableCollection<Chamado> Tickets
        {
            get => _tickets;
            set
            {
                _tickets = value;
                OnPropertyChanged(nameof(Tickets));
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    OnPropertyChanged(nameof(SearchTerm));
                    FiltrarChamados(_searchTerm);
                }
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

        // Comandos
        #region Comandos
        public ICommand LoadTicketsCommand { get; }
        public ICommand ViewTicketCommand { get; }
        public ICommand ScaleTicketCommand { get; }
        public ICommand ResolveTicketCommand { get; }
        public ICommand ViewScaledTicketsCommand { get; }
        public ICommand ViewIaSolutionCommand { get; }
        #endregion

        public bool CanOpenScaledTickets => App.LoggedUser != null && App.LoggedUser.IdPerfilUsuario.Id >= 4;

        public TicketsViewModel(ApiClient apiClient, IDialogService dialogService = null)
        {
            _apiClient = apiClient;
            _dialogService = dialogService ?? new DialogService(); // Se não passar, usa uma nova

            PopupTitle = "";
            PopupWidth = 800;
            PopupHeight = 450;

            Tickets = new ObservableCollection<Chamado>();
            LoadTicketsCommand = new AsyncRelayCommand(LoadTickets);
            ViewTicketCommand = new RelayCommandWithParameter(ViewTicket);
            ScaleTicketCommand = new AsyncRelayCommand(ScaleTicket);
            ResolveTicketCommand = new RelayCommandWithParameter(AbrirPopupMotivo);
            ViewIaSolutionCommand = new RelayCommandWithParameter(ViewIaSolution);


            ViewScaledTicketsCommand = new RelayCommand(OpenScaledTicketsPopup);

            _ = LoadTickets();
        }

        private void ViewIaSolution(object parameter)
        {
            var chamado = parameter as Chamado ?? SelectedTicket;
            if (chamado == null) return;

            // Restrição opcional
            if (!string.Equals(chamado.Status, "Aguardando Resposta", StringComparison.InvariantCultureIgnoreCase))
            {
                MessageBox.Show("A solução da IA está disponível apenas quando o chamado está em 'Aguardando Resposta'.",
                    "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vmIa = new TicketIaSolutionViewModel(_apiClient, chamado);
            _dialogService.ShowDialog(vmIa);
        }

        //virtual pra poder alterar na MyTickets
        public virtual async Task LoadTickets()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var list = await _apiClient.GetChamadosAsync();
                _allTickets = list.Select(MapChamadoToChamadoDatabase).ToList(); // Converte Chamado pra ChamadoDatabase
                FiltrarChamados(SearchTerm);
                Tickets.Clear();
                foreach (var ticket in list)
                {
                    // LOG pra depuração
                    Console.WriteLine($"Chamado: {ticket.Id} Status: {ticket.Status}");
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

                    var detalhesVm = new TicketDetailsViewModel(updatedTicket);
                    _dialogService.ShowDialog(detalhesVm);                    
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

        private async Task ScaleTicket()
        {
            if (SelectedTicket == null)
            {
                MessageBox.Show("Nenhum chamado selecionado!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Tem certeza que deseja escalar o chamado?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            // Input do motivo
            string motivo = PromptForMotivo();
            if (string.IsNullOrWhiteSpace(motivo))
            {
                MessageBox.Show("Motivo obrigatório para escalar.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(motivo) || motivo.Length < 10)
            {
                MessageBox.Show("Informe um motivo válido (mínimo 10 caracteres).", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            try
            {
                await _apiClient.EscalarChamadoAsync(SelectedTicket.Id, motivo);
                MessageBox.Show("Chamado escalado para a gerência com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadTickets();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Sessão expirada. Faça login novamente.", "Erro de Autenticação", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao escalar chamado: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Método recebe o chamado e faz a lógica de decisão
        private async Task ResolveTicketAsync(Chamado chamado, string motivoResolucao)
        {
            if (chamado == null)
                return;

            if (string.IsNullOrWhiteSpace(motivoResolucao) || motivoResolucao.Length < 20)
            {
                MessageBox.Show("O motivo da resolução deve ter pelo menos 20 caracteres.");
                return;
            }

            var confirm = MessageBox.Show(
                $"Tem certeza que deseja resolver o chamado?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            IsLoading = true;

            if (chamado.Status == "Com Analista" && App.LoggedUser.IdPerfilUsuario.Id >= 2)
            {
                await _apiClient.ResolverChamadoAsync(chamado.Id, motivoResolucao);
                MessageBox.Show("Chamado resolvido com sucesso!");
            }
            else if (chamado.Status == "Escalado" && App.LoggedUser.IdPerfilUsuario.Id >= 4)
            {
                await _apiClient.ResolverChamadoEscaladoAsync(chamado.Id, motivoResolucao);
                MessageBox.Show("Chamado resolvido com sucesso!");
            }
            else
            {
                MessageBox.Show("Você não tem permissão para resolver esse chamado.");
                return;
            }

            await LoadTickets();
        }

        // Abre a tela de chamados escalados
        private void OpenScaledTicketsPopup()
        {
            var escalaVm = new TicketsScaleViewModel(_apiClient);
            _dialogService.ShowDialog(escalaVm);
        }

        // Instanciando a MotivoInputWindow
        private string PromptForMotivo()
        {
            var window = new MotivoInputWindow();
            bool? result = window.ShowDialog();
            return result == true ? window.Motivo : null;
        }

        public void AbrirPopupMotivo(object param)
        {
            var chamado = param as Chamado ?? SelectedTicket;
            if (chamado == null) return;

            var vmMotivo = new MotivoResolucaoViewModel(chamado.Id, chamado.Categoria, chamado.Problema);
            vmMotivo.OnClose = async (salvou, motivo) =>
            {
                if (salvou && !string.IsNullOrWhiteSpace(motivo) && motivo.Length >= 20)
                    await ResolveTicketAsync(chamado, motivo);
            };
            _dialogService.ShowDialog(vmMotivo);
        }

        // Filtro de busca
        private void FiltrarChamados(string termo)
        {
            if (_allTickets == null)
            {
                Tickets = new ObservableCollection<Chamado>();
                return;
            }

            IEnumerable<ChamadoDatabase> baseQuery;

            if (string.IsNullOrWhiteSpace(termo))
            {
                baseQuery = _allTickets;
            }
            else
            {
                var lower = termo.ToLowerInvariant();
                baseQuery = _allTickets
                    .Where(c =>
                        c.Id.ToString().Contains(lower) ||
                        (c.Title?.ToLowerInvariant().Contains(lower) ?? false) ||
                        (c.Categoria?.ToLowerInvariant().Contains(lower) ?? false) ||
                        (c.UsuarioAbertura?.ToLowerInvariant().Contains(lower) ?? false) ||
                        (c.Status?.ToLowerInvariant().Contains(lower) ?? false) ||
                        (c.CreatedAt.ToString("dd/MM/yyyy HH:mm").Contains(lower))
                    );
            }
            Tickets = new ObservableCollection<Chamado>(baseQuery.Select(MapDbToChamado));
        }

        private Chamado MapDbToChamado(ChamadoDatabase db)
        {
            if (db == null) return null;
            return new Chamado
            {
                Id = db.Id,
                Title = db.Title,
                Categoria = db.Categoria,
                UsuarioAbertura = db.UsuarioAbertura,
                Status = db.Status,
                CreatedAt = db.CreatedAt,
                Problema = db.Problema,
                Prioridade = db.Prioridade,
                EmailUsuario = db.EmailUsuario,
                Description = db.Description
            };
        }

        private ChamadoDatabase MapChamadoToChamadoDatabase(Chamado chamado)
        {
            if (chamado == null) return null;
            return new ChamadoDatabase
            {
                Id = chamado.Id,
                Title = chamado.Title,
                Description = chamado.Description,
                Status = chamado.Status,
                Prioridade = chamado.Prioridade,
                Categoria = chamado.Categoria,
                Problema = chamado.Problema,
                UsuarioAbertura = chamado.UsuarioAbertura,
                EmailUsuario = chamado.EmailUsuario,
                UsuarioResolucao = chamado.UsuarioResolucao,
                UserId = chamado.UserId,
                CreatedAt = chamado.CreatedAt,
                DataResolucao = chamado.DataResolucao
            };
        }
    }
}
