using sistecDesktop.Commands;
using sistecDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
    public class MotivoResolucaoViewModel : BasePopupViewModel
    {
        private string _motivo;

        public int IdChamado { get; }
        public string Categoria { get; }
        public string Problema { get; }


        public string Motivo
        {
            get => _motivo;
            set { _motivo = value; OnPropertyChanged(nameof(Motivo)); }
        }

        public ICommand SaveCommand { get; }
        public new ICommand CancelCommand { get; }

        // 
        public Action<bool, string> OnClose { get; set; }

        public MotivoResolucaoViewModel(int idChamado, string categoria, string problema)
        {
            PopupTitle = "Relatório da Resolução";
            PopupWidth = 800;
            PopupHeight = 500;

            IdChamado = idChamado;
            Categoria = categoria;
            Problema = problema;

            SaveCommand = new RelayCommand(OnOk, CanSave);
            CancelCommand = new RelayCommand(OnCancel);
        }

        private bool CanSave() => !string.IsNullOrWhiteSpace(Motivo) && Motivo.Length >= 20;

        protected override void OnOk()
        {
            OnClose?.Invoke(true, Motivo);
        }

        protected override void OnCancel()
        {
            OnClose?.Invoke(false, null);
            OnDialogClose(true);
        }
    }
}
