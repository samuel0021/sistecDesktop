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
    public class RestoreUsersViewModel : BaseViewModel
    {
        private readonly ApiClient _apiClient;
        private ObservableCollection<DeletedUserBackup> _usuariosDeletados;
        private ObservableCollection<DeletedUserBackup> _usuariosFiltrados;
        private string _searchTerm;
        private bool _isLoading;
        private string _error;

        public ObservableCollection<DeletedUserBackup> UsuariosDeletados
        {
            get => _usuariosFiltrados ?? _usuariosDeletados;
            set { _usuariosFiltrados = value; OnPropertyChanged(nameof(UsuariosDeletados)); }
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

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public string Error
        {
            get => _error;
            set { _error = value; OnPropertyChanged(nameof(Error)); }
        }

        public ICommand ReloadCommand { get; }
        public ICommand RestoreCommand { get; }

        public RestoreUsersViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            ReloadCommand = new AsyncRelayCommand(FetchUsuariosDeletados);
            RestoreCommand = new AsyncRelayCommandWithParameter<DeletedUserBackup>(RestaurarUsuario);
            _ = FetchUsuariosDeletados();
        }

        public async Task FetchUsuariosDeletados()
        {
            IsLoading = true;
            Error = null;
            try
            {
                var backups = await _apiClient.GetDeletedUsersAsync();
                _usuariosDeletados = new ObservableCollection<DeletedUserBackup>(backups);
                FiltrarUsuarios(SearchTerm);
            }
            catch (Exception ex)
            {
                Error = $"Erro ao carregar usuários deletados: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FiltrarUsuarios(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
            {
                UsuariosDeletados = new ObservableCollection<DeletedUserBackup>(_usuariosDeletados);
            }
            else
            {
                var lower = termo.ToLowerInvariant();
                var filtrados = _usuariosDeletados
                    .Where(u =>
                        u.UsuarioOriginalId.ToString().Contains(lower) ||
                        (u.Name?.ToLowerInvariant().Contains(lower) ?? false) ||
                        (u.Email?.ToLowerInvariant().Contains(lower) ?? false) ||
                        (u.MotivoDelecao?.ToLowerInvariant().Contains(lower) ?? false) ||
                        (u.UsuarioQueDeletou?.ToLowerInvariant().Contains(lower) ?? false)
                    );
                UsuariosDeletados = new ObservableCollection<DeletedUserBackup>(filtrados);
            }
        }

        public async Task RestaurarUsuario(DeletedUserBackup selectedUser)
        {
            if (selectedUser == null || selectedUser.StatusBackup != "ATIVO") return;

            var confirm = MessageBox.Show(
                $"Tem certeza que deseja restaurar o usuário \"{selectedUser.Name}\"?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                var sucesso = await _apiClient.RestoreUserAsync(selectedUser.BackupId);
                if (sucesso)
                {
                    // Recarrega a lista
                    await FetchUsuariosDeletados();
                }
                else
                {
                    Error = "Erro ao restaurar usuário.";
                }
            }
            catch (Exception ex)
            {
                Error = $"Erro ao restaurar usuário: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
