using sistecDesktop.Models;
using sistecDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sistecDesktop.ViewModels
{
    public class TicketDetailsViewModel : BasePopupViewModel
    {
        public Chamado Ticket { get; }

        public TicketDetailsViewModel(Chamado ticket)
        {
            Ticket = ticket;
            PopupTitle = "Detalhes do Chamado";
            PopupWidth = 500;
            PopupHeight = 600;
        }
    }
}
