using sistecDesktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sistecDesktop.Services
{
    public interface IDialogService
    {
        bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : BasePopupViewModel;
    }

    public class DialogService : IDialogService
    {
        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : BasePopupViewModel
        {
            var window = new PopupWindow { DataContext = viewModel };

            viewModel.OnDialogClose = (result) =>
            {
                window.DialogResult = result;
                window.Close();
            };

            return window.ShowDialog();
        }
    }
}
