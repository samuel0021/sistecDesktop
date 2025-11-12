using sistecDesktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace sistecDesktop.Views.Pages
{
    /// <summary>
    /// Interaction logic for Tickets.xaml
    /// </summary>
    public partial class Tickets : UserControl
    {
        public Tickets()
        {
            InitializeComponent();
        }
        public TicketsViewModel ViewModel
        {
            get => (TicketsViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(TicketsViewModel),
                typeof(Tickets),
                new PropertyMetadata(null));

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Sempre que alguém faz: tickets.ViewModel = meuVm;
            // Isso garante que o DataContext do UserControl sempre acompanha o ViewModel.
            if (d is Tickets uc)
                uc.DataContext = e.NewValue;
        }

    }
}
