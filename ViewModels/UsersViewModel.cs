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
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
    public class UsersViewModel : BaseViewModel
    {
        private readonly ApiClient _apiClient;
        private ObservableCollection<UserDatabase> _users;
        private List<UserDatabase> _allUsers;
        private string _searchTerm;
        private bool _isLoading;
        private string _errorMessage;
        private UserDatabase _selectedUser;
        private readonly IDialogService _dialogService;

        #region Encapsulamentos

        public ObservableCollection<UserDatabase> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                OnPropertyChanged(nameof(Users));
            }
        }

        public List<UserDatabase> AllUsers
        {
            get => _allUsers;
            set { _allUsers = value; }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged(nameof(SearchTerm));
                FiltrarUsuarios(_searchTerm);
            }
        }

        public UserDatabase SelectedUser 
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
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

        public ICommand LoadUsersCommand { get; }        
        public ICommand DeleteUserCommand { get; }
        public ICommand OpenCreateUserCommand { get; }
        public ICommand OpenEditUserCommand { get; }



        public UsersViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            _dialogService = new DialogService();
            Users = new ObservableCollection<UserDatabase>();

            LoadUsersCommand = new AsyncRelayCommand(LoadUsers);
            
            OpenEditUserCommand = new RelayCommandWithParameter(EditUser);
            OpenCreateUserCommand = new RelayCommand(OpenCreateUserPopup);
            DeleteUserCommand = new AsyncRelayCommandWithParameter<UserDatabase>(DeleteUserAsync);

            _ = LoadUsers();
        }

        private void EditUser(object parameter)
        {
            if (parameter is UserDatabase userDb)
            {
                // Converte UserDatabase em User para edição/cadastro
                var user = new User
                {
                    Id = userDb.Id,
                    NomeUsuario = userDb.Name,
                    Setor = userDb.Setor,
                    Cargo = userDb.Cargo,
                    Email = userDb.Email,
                    Telefone = userDb.Telefone, 
                    IdPerfilUsuario = new PerfilUsuario
                    {
                        Id = userDb.PerfilId,
                        NivelAcesso = userDb.NivelAcesso,
                        Descricao = userDb.PerfilDescricao,
                        Nome = userDb.PerfilNome
                    }
                };
                var vm = new UserProfileViewModel(_apiClient, user);
                vm.OnDialogClose = result => { if (result == true) _ = LoadUsers(); };
                _dialogService.ShowDialog(vm);
            }
        }

        private void OpenCreateUserPopup()
        {
            var vm = new UserProfileViewModel(_apiClient);
            vm.OnDialogClose = result =>
            {
                if (result == true)
                    _ = LoadUsers();
            };
            _dialogService.ShowDialog(vm);
        }

        private async Task LoadUsers()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var list = await _apiClient.GetUsersAsync();

                _allUsers = list.ToList();
                Users = new ObservableCollection<UserDatabase>(_allUsers);
            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "Sessão expirada. Faça login novamente.";
                MessageBox.Show(ErrorMessage, "Erro de Autenticação", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao carregar usuários: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteUserAsync(UserDatabase userDB)
        {
            if (userDB == null) return;

            var confirm = MessageBox.Show(
                $"Tem certeza que deseja deletar o usuário \"{userDB.Name}\"?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            // Sugestão de InputBox para motivo
            string motivo = PromptForMotivo();

            try
            {
                IsLoading = true;
                string quemDeletou = App.LoggedUser?.NomeUsuario ?? "admin";
                var sucesso = await _apiClient.DeleteUserAsync(userDB.Id, motivo);

                if (sucesso)
                {
                    MessageBox.Show($"Usuário \"{userDB.Name}\" deletado com sucesso!");
                    await LoadUsers();
                }
                else
                {
                    MessageBox.Show("Erro ao deletar usuário.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao deletar usuário: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FiltrarUsuarios(string termo)
        {
            if (_allUsers == null) return;

            if (string.IsNullOrWhiteSpace(termo))
            {
                Users = new ObservableCollection<UserDatabase>(_allUsers);
            }
            else
            {
                var lower = termo.ToLowerInvariant();
                var filtrados = _allUsers
                    .Where(u =>
                        (u.Name?.ToLowerInvariant().Contains(lower) ?? false) ||
                        (u.Email?.ToLowerInvariant().Contains(lower) ?? false) ||
                        (u.Matricula.ToString().Contains(lower)) ||
                        (u.Setor?.ToLowerInvariant().Contains(lower) ?? false) ||
                        (u.Cargo?.ToLowerInvariant().Contains(lower) ?? false)
                    );
                Users = new ObservableCollection<UserDatabase>(filtrados);
            }
        }

        private string PromptForMotivo()
        {
            var window = new MotivoInputWindow();
            bool? result = window.ShowDialog();
            return result == true ? window.Motivo : null;
        }

    }
}
