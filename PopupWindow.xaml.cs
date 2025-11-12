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
using System.Windows.Shapes;

namespace sistecDesktop
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window
    {
        public PopupWindow()
        {
            InitializeComponent();

            // Adiciona um evento pra quando o DataContext for alterado nessa janela
            DataContextChanged += (s, e) =>
            {               
                if (DataContext is BasePopupViewModel vm)
                {
                    Height = vm.PopupHeight;
                    Width = vm.PopupWidth;
                    Title = vm.PopupTitle;

                    // Se alguma dessas propriedades mudar no ViewModel enquanto a janela está aberta, atualiza a janela também
                    vm.PropertyChanged += (s2, e2) =>
                    {
                        // Se a altura mudar no ViewModel, atualize a altura da janela
                        if (e2.PropertyName == nameof(vm.PopupHeight))
                            Dispatcher.Invoke(() => Height = vm.PopupHeight);
                        // Se a largura mudar no ViewModel, atualize a largura da janela
                        if (e2.PropertyName == nameof(vm.PopupWidth))
                            Dispatcher.Invoke(() => Width = vm.PopupWidth);
                        // Se o título mudar no ViewModel, atualize o título da janela
                        if (e2.PropertyName == nameof(vm.PopupTitle))
                            Dispatcher.Invoke(() => Title = vm.PopupTitle);
                    };
                }
            };
        }
    }
}
