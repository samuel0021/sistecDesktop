using sistecDesktop.Commands;
using sistecDesktop.Models;
using sistecDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
    public class PerfilUsuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int Nivel { get; set; }
    }

    public class UserCreateViewModel : BasePopupViewModel
    {
        private readonly ApiClient _apiClient;
        private string _nome;
        private string _sobrenome;
        private string _email;
        private string _telefone;
        private string _ramal;
        private string _cargo;
        private string _setor;
        private PerfilUsuario _perfilSelecionado;
        private string _senha;
        private string _errorMessage;
        private bool _isLoading;

        public UserCreateViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            PerfisAcesso = new ObservableCollection<PerfilUsuario>
            {
                new PerfilUsuario { Id = 1, Nome = "Usuário", Nivel = 1 },
                new PerfilUsuario { Id = 2, Nome = "Analista de Suporte", Nivel = 2 },
                new PerfilUsuario { Id = 5, Nome = "Gestor de Chamados", Nivel = 3 },
                new PerfilUsuario { Id = 3, Nome = "Gerente de Suporte", Nivel = 4 },
                new PerfilUsuario { Id = 4, Nome = "Administrador", Nivel = 5 }
            };

            CreateUserCommand = new AsyncRelayCommand(CreateUserAsync);
        }

        public ObservableCollection<PerfilUsuario> PerfisAcesso { get; }

        public string Nome
        {
            get => _nome;
            set
            {
                _nome = value;
                OnPropertyChanged(nameof(Nome));
                AutoFillEmailSenha();
            }
        }
        public string Sobrenome
        {
            get => _sobrenome;
            set
            {
                _sobrenome = value;
                OnPropertyChanged(nameof(Sobrenome));
                AutoFillEmailSenha();
            }
        }
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }
        public string Telefone
        {
            get => _telefone;
            set
            {
                _telefone = value;
                OnPropertyChanged(nameof(Telefone));
                AutoFillEmailSenha();
            }
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

        public ICommand CreateUserCommand { get; }

        private void AutoFillEmailSenha()
        {
            if (!string.IsNullOrWhiteSpace(Nome) && !string.IsNullOrWhiteSpace(Sobrenome))
                Email = GerarEmail(Nome, Sobrenome);

            if (!string.IsNullOrWhiteSpace(Nome) && !string.IsNullOrWhiteSpace(Telefone))
                Senha = GerarSenhaPadrao(Nome, Telefone);
        }

        private string GerarEmail(string nome, string sobrenome)
        {
            string RemoverAcentos(string str) =>
                new string(str.Normalize(NormalizationForm.FormD)
                           .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                           .ToArray());

            var n = RemoverAcentos(nome?.Trim().Split(' ')[0] ?? "").ToLowerInvariant();
            var sobrenomes = sobrenome?.Trim().Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() ?? Array.Empty<string>();
            var s = sobrenomes.Length > 0 ? RemoverAcentos(sobrenomes.Last()).ToLowerInvariant() : "";
            if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(s)) return "";
            return $"{n}.{s}@sistec.com.br";
        }

        private string GerarSenhaPadrao(string nome, string telefone)
        {
            string RemoverAcentos(string s) =>
                new string(s.Normalize(NormalizationForm.FormD)
                           .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark).ToArray());
            var primeiroNome = RemoverAcentos(nome?.Trim().Split(' ')[0] ?? "");
            var ultimos4 = telefone.Length >= 4
                ? telefone.Substring(telefone.Length - 4)
                : telefone;
            return string.IsNullOrWhiteSpace(primeiroNome) || string.IsNullOrWhiteSpace(ultimos4)
                ? ""
                : $"{primeiroNome}{ultimos4}";
        }

        private async Task CreateUserAsync()
        {
            ErrorMessage = string.Empty;

            // Validação
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
            if (!string.IsNullOrWhiteSpace(Senha) && Senha.Length < 6)
            {
                ErrorMessage = "Senha deve ter pelo menos 6 caracteres.";
                return;
            }

            IsLoading = true;

            try
            {
                var usuario = new User
                {
                    NomeUsuario = $"{Nome} {Sobrenome}".Trim(),
                    Email = Email,
                    Telefone = Telefone,
                    Ramal = Ramal,
                    Cargo = Cargo,
                    Setor = Setor,
                    IdPerfilUsuario = PerfilSelecionado.Id,
                    Senha = Senha // já preenchida na geração automática!
                };

                var created = await _apiClient.CreateUserAsync(usuario);

                if (created != null)
                {
                    OnDialogClose?.Invoke(true);
                }
                else
                {
                    ErrorMessage = "Erro ao cadastrar usuário.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao cadastrar usuário: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected override void OnOk()
        {
            CreateUserCommand.Execute(null);
        }
    }
}
