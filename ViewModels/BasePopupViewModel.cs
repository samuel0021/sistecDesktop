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
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public BasePopupViewModel() 
        {
            OkCommand = new RelayCommand(OnOk);
            CancelCommand = new RelayCommand(OnCancel);
        }

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
