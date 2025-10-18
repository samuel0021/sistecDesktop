using sistecDesktop.Commands;
using sistecDesktop.Views.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private string _paginaSelecionada;
        private UserControl _currentContent;

        public ICommand LogoutCommand { get; }
        public ICommand SelecionarPaginaCommand { get; } // ✅ NOVO


        public UserControl CurrentContent
        {
            get { return _currentContent; }
            set 
            {
                if (_currentContent != value) 
                { 
                    _currentContent = value;
                    OnPropertyChanged(nameof(CurrentContent));
                }
            }
        }

        // ✅ ADICIONE esta propriedade
        public string PaginaSelecionada
        {
            get => _paginaSelecionada;
            set
            {
                if (_paginaSelecionada != value)
                {
                    _paginaSelecionada = value;
                    OnPropertyChanged(nameof(PaginaSelecionada));
                    OnPropertyChanged(nameof(TagHome));
                    OnPropertyChanged(nameof(TagDashboard));
                    OnPropertyChanged(nameof(TagChamados));
                    OnPropertyChanged(nameof(TagUsuarios));

                    LoadContent(value);
                }
            }
        }

        // ✅ ADICIONE estas propriedades (Tag para cada botão)
        public string TagHome => PaginaSelecionada == "Home" ? "Selected" : null;
        public string TagDashboard => PaginaSelecionada == "Dashboard" ? "Selected" : null;
        public string TagChamados => PaginaSelecionada == "Chamados" ? "Selected" : null;
        public string TagUsuarios => PaginaSelecionada == "Usuarios" ? "Selected" : null;

        public HomeViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            LogoutCommand = new LogoutCommand(this);

            // ✅ ADICIONE esta linha
            SelecionarPaginaCommand = new RelayCommandWithParameter(
                parameter => PaginaSelecionada = parameter?.ToString()
            );

            // Opcional: inicia sem seleção
            // Se quiser iniciar com Home selecionado, descomente a linha abaixo:
            PaginaSelecionada = "Home";
        }

        private void LoadContent(string nomePagina)
        {
            switch (nomePagina)
            {
                case "Home":
                    CurrentContent = new Home();
                    break;
                case "Dashboard":
                    CurrentContent = new Dashboard();
                    break;
                case "Chamados":
                    CurrentContent = new Chamados();
                    break;
                case "Usuarios":
                    CurrentContent = new Usuarios();
                    break;
                default:
                    CurrentContent = new Home();
                    break;
            }
        }

        public void ExecutarLogout()
        {
            _mainViewModel.SelectedViewModel = new LoginViewModel(_mainViewModel);
        }
    }
}
