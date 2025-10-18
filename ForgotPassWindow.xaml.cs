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
    /// Interaction logic for ForgotPassWindow.xaml
    /// </summary>
    public partial class ForgotPassWindow : Window
    {
        public ForgotPassWindow()
        {
            InitializeComponent();
            DataContext = new ForgotPassViewModel(); // IMPORTANTE
        }

        public static void Mostrar()
        {
            // 1. Cria a janela que vai ser mostrada
            var janela = new ForgotPassWindow();

            // 2. Encontra qual janela está ativa (a que tem foco)
            var ativa = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.IsActive);

            Grid overlay = null;

            if (ativa != null)
            {
                // 3. Define a janela ativa como "dona" da nova janela
                janela.Owner = ativa;

                // 4. PROCURA o overlay na árvore visual da janela ativa
                overlay = FindOverlayInVisualTree(ativa);

                // 5. Se encontrou o overlay, torna ele visível (escurece a tela)
                if (overlay != null)
                {
                    overlay.Visibility = Visibility.Visible;
                }
            }

            // 6. Mostra a janela modal (bloqueia interação com outras janelas)
            janela.ShowDialog();

            // 7. Quando a janela fechar, esconde o overlay novamente
            if (overlay != null)
            {
                overlay.Visibility = Visibility.Collapsed;
            }
        }

        // Método helper para procurar o overlay na árvore visual
        private static Grid FindOverlayInVisualTree(DependencyObject parent)
        {
            // Se não tem pai, retorna null
            if (parent == null) return null;

            // Conta quantos elementos filhos este elemento tem
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            // Loop por todos os filhos
            for (int i = 0; i < childCount; i++)
            {
                // Pega o filho na posição i
                var child = VisualTreeHelper.GetChild(parent, i);

                // Se for um FrameworkElement E o nome for "DarkOverlay"
                if (child is FrameworkElement element && element.Name == "DarkOverlay")
                {
                    return element as Grid; // ENCONTROU! Retorna
                }

                // Se não encontrou, procura nos filhos deste filho (RECURSÃO)
                var result = FindOverlayInVisualTree(child);
                if (result != null)
                    return result; // Encontrou nos filhos, retorna
            }

            // Não encontrou em nenhum lugar
            return null;
        }
    }
}
/*

[.FirstOrDefault(w => w.IsActive)] - pega a primeira janela que está com foco

[w => w.IsActive] expressão lambda que verifica se a janela está ativa

 */
