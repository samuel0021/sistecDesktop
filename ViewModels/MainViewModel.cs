using sistecDesktop.Commands;
using sistecDesktop.Views;
using sistecDesktop.Services;  
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _selectedViewModel;
        
        // Tornar público para facilitar acesso
        public ApiClient ApiClient { get; private set; }  


        public BaseViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set
            {
                _selectedViewModel = value;
                OnPropertyChanged(nameof(SelectedViewModel));
            }
        }

        public ICommand UpdateViewCommand { get; set; }

        public MainViewModel()
        {
            // Criar o ApiClient UMA VEZ aqui
            ApiClient = new ApiClient(); 

            UpdateViewCommand = new UpdateViewCommand(this);

            // Inicia com a tela de Login passando a referência e o apiClient
            SelectedViewModel = new LoginViewModel(this, ApiClient);  
        }

        // Método auxiliar para quando outras ViewModels precisarem do ApiClient
        public ApiClient GetApiClient() 
        {
            return ApiClient;
        }
    }
}