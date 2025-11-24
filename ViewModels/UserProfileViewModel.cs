using sistecDesktop.Commands;
using sistecDesktop.Models;
using sistecDesktop.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
   

    public class UserProfileViewModel : BasePopupViewModel
    {
        private readonly ApiClient _apiClient;
        private bool _isLoading;
        private string _errorMessage;
        private bool _modoEdicao;

        private int? _idUsuario;

        #region Campos do formulário
        private string _nome;
        private string _sobrenome;
        private string _email;
        private string _senha;
        private string _telefone;
        private string _ramal;
        private string _cargo;
        private string _setor;
        private PerfilUsuario _perfilSelecionado;
        private int _matriculaAprovador;
        #endregion


        #region Encapsulamentos
        public bool ModoEdicao
        {
            get => _modoEdicao;
            set
            {
                if (_modoEdicao != value)
                {
                    _modoEdicao = value;
                    OnPropertyChanged(nameof(ModoEdicao));
                    OnPropertyChanged(nameof(IsSenhaReadOnly));
                }
            }
        }
        public bool IsSenhaReadOnly => !ModoEdicao;


        public ObservableCollection<PerfilUsuario> PerfisAcesso { get; set; } = new ObservableCollection<PerfilUsuario>();

        public string Nome
        {
            get => _nome;
            set { _nome = value; OnPropertyChanged(nameof(Nome)); AtualizarEmailSenha(); }
        }
        public int MatriculaAprovador
        {
            get => _matriculaAprovador;
            set { _matriculaAprovador = value; OnPropertyChanged(nameof(MatriculaAprovador)); }
        }

        public string Sobrenome
        {
            get => _sobrenome;
            set { _sobrenome = value; OnPropertyChanged(nameof(Sobrenome)); AtualizarEmailSenha(); }
        }
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }
        public string Telefone
        {
            get => _telefone;
            set { _telefone = value; OnPropertyChanged(nameof(Telefone)); AtualizarEmailSenha(); }
        }
        public string Ramal
        {
            get => _ramal;
            set { _ramal = value; OnPropertyChanged(nameof(Ramal)); }
        }
        public string Cargo
        {
            get => _cargo;
            set { _cargo = value; OnPropertyChanged(nameof(Cargo)); }
        }
        public string Setor
        {
            get => _setor;
            set { _setor = value; OnPropertyChanged(nameof(Setor)); }
        }
        public PerfilUsuario PerfilSelecionado
        {
            get => _perfilSelecionado;
            set { _perfilSelecionado = value; OnPropertyChanged(nameof(PerfilSelecionado)); }
        }
        public string Senha
        {
            get => _senha;
            set { _senha = value; OnPropertyChanged(nameof(Senha)); }
        }
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }
        #endregion

        public string TituloForm => ModoEdicao ? "Editar Usuário" : "Cadastrar Usuário";
        public string TextoBotaoSalvar => ModoEdicao ? "Salvar" : "Cadastrar";

        // Comandos
        public ICommand EditUserCommand { get; }
        public new ICommand CancelCommand { get; }

        //Construtor
        #region Construtor
        public UserProfileViewModel(ApiClient apiClient, User usuarioExistente = null)
        {
            _apiClient = apiClient;

            PopupTitle = TituloForm;
            PopupWidth = 800;
            PopupHeight = 550;

            PerfisAcesso = new ObservableCollection<PerfilUsuario>
            {
                new PerfilUsuario { Id = 1, Nome = "Usuário", NivelAcesso = 1 },
                new PerfilUsuario { Id = 2, Nome = "Analista de Suporte", NivelAcesso = 2 },
                new PerfilUsuario { Id = 5, Nome = "Gestor de Chamados", NivelAcesso = 3 },
                new PerfilUsuario { Id = 3, Nome = "Gerente de Suporte", NivelAcesso = 4 },
                new PerfilUsuario { Id = 4, Nome = "Administrador", NivelAcesso = 5 }
            };

            AtualizarPerfisVisiveis();

            if (usuarioExistente != null)
            {
                ModoEdicao = true;
                _idUsuario = usuarioExistente.Id;
                var split = (usuarioExistente.NomeUsuario ?? "").Split(' '); // Separa nome/sobrenome do nome_usuario
                Nome = split.FirstOrDefault() ?? "";
                Sobrenome = string.Join(" ", split.Skip(1));
                Email = usuarioExistente.Email;
                Telefone = usuarioExistente.Telefone;
                Ramal = usuarioExistente.Ramal;
                Cargo = usuarioExistente.Cargo;
                Setor = usuarioExistente.Setor;
                PerfilSelecionado = PerfisAcesso.FirstOrDefault(p => p.Id == usuarioExistente.IdPerfilUsuario.NivelAcesso);
                Senha = usuarioExistente.Senha;
            }
            else
            {
                ModoEdicao = false;
                _idUsuario = null;
                Nome = Sobrenome = Email = Telefone = Ramal = Cargo = Setor = Senha = "";
                PerfilSelecionado = PerfisAcesso.FirstOrDefault();
            }

            EditUserCommand = new AsyncRelayCommand(ExecutarSalvarAsync);
            CancelCommand = new RelayCommand(ExecutarCancelar);
        }
        #endregion

        private void AtualizarEmailSenha()
        {
            if (!ModoEdicao)
            {
                Email = GerarEmail(Nome, Sobrenome);
                Senha = GerarSenhaPadrao(Nome, Telefone);
            }
        }

        #region Geradores
        private string GerarEmail(string nome, string sobrenome)
        {
            string RemoverAcentos(string s) =>
                new string(s.Normalize(System.Text.NormalizationForm.FormD)
                           .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark).ToArray());

            var n = RemoverAcentos(nome?.Trim().Split(' ')[0] ?? "").ToLowerInvariant();
            var sobrenomes = sobrenome?.Trim().Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() ?? Array.Empty<string>();
            var s_ = sobrenomes.Length > 0 ? RemoverAcentos(sobrenomes.Last()).ToLowerInvariant() : "";
            if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(s_)) return "";
            return $"{n}.{s_}@sistec.com.br";
        }

        private string GerarSenhaPadrao(string nome, string telefone)
        {
            string RemoverAcentos(string s) =>
                new string(s.Normalize(System.Text.NormalizationForm.FormD)
                           .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark).ToArray());
            var primeiroNome = RemoverAcentos(nome?.Trim().Split(' ')[0] ?? "");
            var apenasNumeros = new string((telefone ?? "").Where(char.IsDigit).ToArray());
            var ultimos4 = apenasNumeros.Length >= 4
                ? apenasNumeros.Substring(apenasNumeros.Length - 4)
                : apenasNumeros;
            if (string.IsNullOrWhiteSpace(primeiroNome) || string.IsNullOrWhiteSpace(ultimos4))
                return "";
            return $"{primeiroNome}{ultimos4}";
        }
        #endregion

        private async Task ExecutarSalvarAsync()
        {
            ErrorMessage = "";

            var confirm = MessageBox.Show(
                $"Tem certeza que deseja salvar as informações?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                #region Validações
                if (string.IsNullOrWhiteSpace(Nome))
                {
                    ErrorMessage = "Nome é obrigatório.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(Sobrenome))
                {
                    ErrorMessage = "Sobrenome é obrigatório.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
                {
                    ErrorMessage = "E-mail é obrigatório e deve ser válido.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(Telefone))
                {
                    ErrorMessage = "Telefone é obrigatório.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(Cargo))
                {
                    ErrorMessage = "Cargo é obrigatório.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(Setor))
                {
                    ErrorMessage = "Setor é obrigatório.";
                    return;
                }
                if (PerfilSelecionado == null)
                {
                    ErrorMessage = "Nível de acesso é obrigatório.";
                    return;
                }
                if (!ModoEdicao && !string.IsNullOrWhiteSpace(Senha) && Senha.Length < 6)
                {
                    ErrorMessage = "Senha deve ter pelo menos 6 caracteres.";
                    return;
                }
                #endregion

                var user = new UserDatabase
                {
                    Name = $"{Nome} {Sobrenome}".Trim(),
                    Setor = Setor,
                    Cargo = Cargo,
                    Email = Email,
                    Telefone = Telefone,
                    //Ramal = Ramal,
                    //Perfil = PerfilSelecionado,
                    Matricula = MatriculaAprovador,
                    Senha = string.IsNullOrWhiteSpace(Senha) ? GerarSenhaPadrao(Nome, Telefone) : Senha,
                    PerfilId = PerfilSelecionado.Id,
                    NivelAcesso = PerfilSelecionado.NivelAcesso,
                    PerfilDescricao = PerfilSelecionado.Descricao
                };
                
                if (ModoEdicao && _idUsuario != null)
                {
                    // Edição (PUT) 
                    var editResponse = await _apiClient.UpdateUserAsync(_idUsuario.Value, user);
                    if (editResponse != null && editResponse.Success)
                        OnDialogClose?.Invoke(true);
                    else
                        ErrorMessage = "Erro ao editar usuário: " + (editResponse?.Message ?? "");
                }
                else
                {
                    // Cadastro (POST) 
                    var createdUser = await _apiClient.CreateUserAsync(user);
                    if (createdUser != null && createdUser.PerfilId > 0)
                        OnDialogClose?.Invoke(true);
                    else
                        ErrorMessage = "Erro ao cadastrar usuário";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public ObservableCollection<PerfilUsuario> PerfisVisiveis { get; set; }

        private void AtualizarPerfisVisiveis()
        {
            if (App.LoggedUser != null && App.LoggedUser.IdPerfilUsuario.NivelAcesso >= 5)
            {
                PerfisVisiveis = new ObservableCollection<PerfilUsuario>(PerfisAcesso);  // Mostra todas as opções se o usuário logado for um admin
            }
            else
            {
                PerfisVisiveis = new ObservableCollection<PerfilUsuario>(PerfisAcesso.Where(p => p.NivelAcesso < 5));  // Oculta a opção "Administrador"
            }
            OnPropertyChanged(nameof(PerfisVisiveis));
        }

        private void ExecutarCancelar()
        {
            OnDialogClose?.Invoke(false);
        }
    }
}
