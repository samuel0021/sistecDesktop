using sistecDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace sistecDesktop.ViewModels
{
    public class OpenTicketViewModel : BasePopupViewModel
    {
        private readonly ApiClient _apiClient;

        public OpenTicketViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        //protected override async void OnOk()
        //{
        //    if (string.IsNullOrWhiteSpace(Titulo))
        //    {
        //        MessageBox.Show("Preencha o título");
        //        return;
        //    }

        //    try
        //    {
        //        await _apiClient.CriarChamadoAsync(new { Titulo, Descricao, Categoria });
        //        base.OnOk();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Erro: {ex.Message}");
        //    }
        //}
    }
}
