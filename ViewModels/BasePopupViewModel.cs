using sistecDesktop.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace sistecDesktop.ViewModels
{
    // não pode ser instanciada, apenas herdada
    public abstract class BasePopupViewModel : BaseViewModel
    {
        private bool? _dialogResult;
        public Action<bool?> OnDialogClose { get; set; }

        // Propriedades do layout
        private string _popupTitle;
        private double _popupWidth;
        private double _popupHeight;

        //Propriedades encapsuladas
        #region Encapsulamentos
        public string PopupTitle
        {
            get => _popupTitle;
            set { _popupTitle = value; OnPropertyChanged(nameof(PopupTitle)); }
        }

        public double PopupWidth
        {
            get => _popupWidth;
            set { _popupWidth = value; OnPropertyChanged(nameof(PopupWidth)); }
        }

        public double PopupHeight
        {
            get => _popupHeight;
            set { _popupHeight = value; OnPropertyChanged(nameof(PopupHeight)); }
        }


        public bool? DialogResult 
        { 
            get => _dialogResult;
            set
            {
                if (_dialogResult != value)
                {
                    _dialogResult = value;
                    OnPropertyChanged(nameof(DialogResult));
                    OnDialogClose?.Invoke(value);
                }
            }
        }
        #endregion

        // Comandos
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public BasePopupViewModel() 
        {
            OkCommand = new RelayCommand(OnOk);
            CancelCommand = new RelayCommand(OnCancel);
        }

        // Métodos virtuais para sobrescrita
        protected virtual void OnOk()
        {
            DialogResult = true;
        }

        protected virtual void OnCancel()
        {
            DialogResult = false;
        }
    }
}
