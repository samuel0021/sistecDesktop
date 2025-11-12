using sistecDesktop.Models;
using sistecDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace sistecDesktop.ViewModels
{
    public class MyTicketsViewModel : TicketsViewModel
    {

        public MyTicketsViewModel(ApiClient apiClient) : base(apiClient)
        {
            PopupTitle = "Meus Chamados";
            PopupWidth = 1150;
            PopupHeight = 500;
        }

        //sobrescrita pra mostrar só os chamados do usuário logado
        public override async Task LoadTickets()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var all = await _apiClient.GetChamadosAsync();

                Console.WriteLine($"App.LoggedUser.Id: {App.LoggedUser.Id}");
                foreach (var chamado in all)
                    Console.WriteLine($"Chamado: {chamado.Id}, UserId: {chamado.UserId}");

                var myTickets = all.Where(c => c.UserId == App.LoggedUser.Id).ToList();
                Tickets.Clear();
                foreach (var ticket in myTickets)
                    Tickets.Add(ticket);

                Console.WriteLine($"Meus chamados carregados: {Tickets.Count}");
                foreach (var chamado in all)
                    Console.WriteLine($"Chamado: {chamado.Id}, UserId: {chamado.UserId}");
            }
            // Mesmos catch/finally igual ao TicketsViewModel
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "Sessão expirada. Faça login novamente.";
                MessageBox.Show(ErrorMessage, "Erro de Autenticação", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao carregar chamados: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
