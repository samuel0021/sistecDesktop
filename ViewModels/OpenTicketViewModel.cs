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
    public class OpenTicketViewModel : BasePopupViewModel
    {
        private readonly ApiClient _apiClient;

        // Propriedades da tela
        private string _title;
        private string _description;
        private string _errorMessage;
        private bool _isLoading;
        private int _priority;
        private string _category;
        private string _problemaSelecionado;
        private ObservableCollection<ProblemaItem> _problemasDisponiveis;

        #region Encapsulamentos

        public string ProblemaSelecionado
        {
            get => _problemaSelecionado;
            set
            {
                _problemaSelecionado = value;
                OnPropertyChanged(nameof(ProblemaSelecionado));
            }
        }

        public ObservableCollection<ProblemaItem> ProblemasDisponiveis
        {
            get => _problemasDisponiveis;
            set
            {
                _problemasDisponiveis = value;
                OnPropertyChanged(nameof(ProblemasDisponiveis));
            }
        }

        public string Title
        {
            get { return _title; }
            set 
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
        
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
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

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public int Priority
        {
            get { return _priority; }
            set
            {
                _priority = value;
                OnPropertyChanged(nameof(Priority));
            }
        }

        public string CategoriaSelecionada
        {
            get { return _category; }
            set 
            {
                if (_category != value)
                {
                    _category = value;
                    OnPropertyChanged(nameof(CategoriaSelecionada));
                    Console.WriteLine($"DEBUG: Categoria alterada para: {_category}");

                    // Atualizar problemas automaticamente
                    AtualizarProblemas(value);

                    // Limpar problema selecionado
                    ProblemaSelecionado = null;
                }
            }
        }
        #endregion

        // Comandos
        public ICommand CreateTicketCommand { get; }

        // Construtor
        public OpenTicketViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;

            PopupTitle = "Abrir Chamado";
            PopupWidth = 500;
            PopupHeight = 700;

            ProblemasDisponiveis = new ObservableCollection<ProblemaItem>();
            CreateTicketCommand = new AsyncRelayCommand(CreateTicket);
        }

        public Action OnChamadoCriado { get; set; }

        private async Task CreateTicket()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Por favor, informe o título.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Por favor, informe a descrição.";
                return;
            }

            if (Priority == 0)
            {
                ErrorMessage = "Por favor, selecione a prioridade.";
                return;
            }

            if (string.IsNullOrEmpty(CategoriaSelecionada))
            {
                ErrorMessage = "Por favor, selecione a categoria.";
                return;
            }

            if (string.IsNullOrEmpty(ProblemaSelecionado))
            {
                ErrorMessage = "Por favor, selecione o problema.";
                return;
            }

            var confirm = MessageBox.Show(
                $"Tem certeza que deseja abrir o chamado?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            IsLoading = true;

            var categoria = CategoriaSelecionada?.ToLower();

            try
            {

                var request = new CreateChamadoRequest
                {
                    Title = Title,
                    Description = Description,
                    UserId = App.LoggedUser.IdPerfilUsuario.Id,
                    Priority = Priority,
                    Category = categoria, // campo original que você vinha usando
                    DescricaoCategoria = categoria, // compatibilidade com site (descricao_categoria)
                    DescricaoCategoriaChamado = categoria, // compatibilidade com outros lugares
                    Problem = ProblemaSelecionado,

                    DescricaoDetalhada = $"Título: {Title}\n\nDescrição: {Description}"
                };

                var chamado = await _apiClient.CreateChamadoAsync(request);

                MessageBox.Show(
                    $"Chamado #{chamado.Id} criado com sucesso!",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                OnChamadoCriado?.Invoke();


                OnDialogClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao criar chamado: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private List<CategoriaProblema> ObterCategoriasProblemas()
        {
            return new List<CategoriaProblema>
    {
        new CategoriaProblema
        {
            Categoria = "hardware",
            Label = "Hardware",
            Problemas = new List<ProblemaItem>
            {
                new ProblemaItem { Value = "computador-nao-liga", Label = "Computador não liga" },
                new ProblemaItem { Value = "tela-preta", Label = "Tela preta" },
                new ProblemaItem { Value = "travamento-frequente", Label = "Travamento frequente" },
                new ProblemaItem { Value = "lentidao-equipamento", Label = "Lentidão no equipamento" },
                new ProblemaItem { Value = "problema-teclado-mouse", Label = "Problema com teclado/mouse" },
                new ProblemaItem { Value = "outros-hardware", Label = "Outros problemas de hardware" }
            }
        },
        new CategoriaProblema
        {
            Categoria = "software",
            Label = "Software",
            Problemas = new List<ProblemaItem>
            {
                new ProblemaItem { Value = "erro-sistema", Label = "Erro no sistema" },
                new ProblemaItem { Value = "aplicativo-nao-abre", Label = "Aplicativo não abre" },
                new ProblemaItem { Value = "lentidao-sistema", Label = "Lentidão no sistema" },
                new ProblemaItem { Value = "perda-dados", Label = "Perda de dados" },
                new ProblemaItem { Value = "atualizacao-software", Label = "Problema com atualização" },
                new ProblemaItem { Value = "outros-software", Label = "Outros problemas de software" }
            }
        },
        new CategoriaProblema
        {
            Categoria = "rede",
            Label = "Rede e Conectividade",
            Problemas = new List<ProblemaItem>
            {
                new ProblemaItem { Value = "sem-internet", Label = "Sem acesso à internet" },
                new ProblemaItem { Value = "wifi-nao-conecta", Label = "Wi-Fi não conecta" },
                new ProblemaItem { Value = "lentidao-rede", Label = "Lentidão na rede" },
                new ProblemaItem { Value = "acesso-compartilhado", Label = "Problema com acesso compartilhado" },
                new ProblemaItem { Value = "outros-rede", Label = "Outros problemas de rede" }
            }
        },
        new CategoriaProblema
        {
            Categoria = "acesso",
            Label = "Acesso e Permissões",
            Problemas = new List<ProblemaItem>
            {
                new ProblemaItem { Value = "esqueci-senha", Label = "Esqueci minha senha" },
                new ProblemaItem { Value = "acesso-negado", Label = "Acesso negado ao sistema" },
                new ProblemaItem { Value = "criar-usuario", Label = "Criar novo usuário" },
                new ProblemaItem { Value = "alterar-permissoes", Label = "Alterar permissões" },
                new ProblemaItem { Value = "outros-acesso", Label = "Outros problemas de acesso" }
            }
        },
        new CategoriaProblema
        {
            Categoria = "email",
            Label = "Email",
            Problemas = new List<ProblemaItem>
            {
                new ProblemaItem { Value = "nao-recebe-email", Label = "Não está recebendo emails" },
                new ProblemaItem { Value = "nao-envia-email", Label = "Não consegue enviar emails" },
                new ProblemaItem { Value = "configurar-email", Label = "Configurar cliente de email" },
                new ProblemaItem { Value = "problema-anexo", Label = "Problema com anexos" },
                new ProblemaItem { Value = "outros-email", Label = "Outros problemas de email" }
            }
        },
        new CategoriaProblema
        {
            Categoria = "outros",
            Label = "Outros",
            Problemas = new List<ProblemaItem>
            {
                new ProblemaItem { Value = "solicitacao-equipamento", Label = "Solicitação de equipamento" },
                new ProblemaItem { Value = "treinamento", Label = "Solicitação de treinamento" },
                new ProblemaItem { Value = "sugestao-melhoria", Label = "Sugestão de melhoria" },
                new ProblemaItem { Value = "outros-geral", Label = "Outros" }
            }
        }
    };
        }

        // Atualizar problemas quando categoria mudar
        private void AtualizarProblemas(string categoria)
        {
            ProblemasDisponiveis.Clear();

            if (string.IsNullOrEmpty(categoria))
                return;

            var categoriaEncontrada = ObterCategoriasProblemas()
                .FirstOrDefault(c => c.Categoria == categoria);

            if (categoriaEncontrada != null)
            {
                foreach (var problema in categoriaEncontrada.Problemas)
                {
                    ProblemasDisponiveis.Add(problema);
                }
            }
        }


        //protected override async void OnOk()
        //{
        //    if (string.IsNullOrWhiteSpace(Titulo))
        //    {
        //        MessageBox.Show("Preencha o título");
        //        return;
        //    }

        //    try
        //    {
        //        await _apiClient.CriarChamadoAsync(new { Titulo, Descricao, Categoria });
        //        base.OnOk();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Erro: {ex.Message}");
        //    }
        //}
    }
}
