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
    public class UsersViewModel : BaseViewModel
    {
        private readonly ApiClient _apiClient;
        private ObservableCollection<User> _users;
        private bool _isLoading;
        private string _errorMessage;
        private User _selectedUser;

        #region Encapsulamentos
        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                OnPropertyChanged(nameof(Users));
            }
        }

        public User SelectedUser 
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
        public ICommand ViewUserCommand { get; }

        public UsersViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            Users = new ObservableCollection<User>();

            LoadUsersCommand = new AsyncRelayCommand(LoadUsers);
            ViewUserCommand = new RelayCommandWithParameter(ViewUser);

            _ = LoadUsers();
        }

        private async Task LoadUsers()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var list = await _apiClient.GetUsersAsync();

                Users.Clear();
                foreach (var user in list)
                {
                    Users.Add(user);
                }
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

        private void ViewUser(object parameter)
        {
            if (parameter is User user)
            {
                MessageBox.Show(
                    $"ID: {user.Id}\n" +
                    $"Nome: {user.Name}\n" +
                    $"Email: {user.Email}\n" +
                    $"Telefone: {user.Telefone}\n" +
                    $"Cargo: {user.Cargo}\n" +
                    $"Setor: {user.Setor}\n" +
                    $"Matrícula: {user.Matricula}\n",
                    "Detalhes do Chamado",
                    MessageBoxButton.OK ,
                    MessageBoxImage.Information);
            }
        }
    }
}
