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
    /// Interaction logic for MotivoInputWindow.xaml
    /// </summary>
    public partial class MotivoInputWindow : Window
    {
        public string Motivo => InputBox.Text;

        public MotivoInputWindow()
        {
            InitializeComponent();
            InputBox.Focus();

            OkButton.IsEnabled = false;
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OkButton.IsEnabled = InputBox.Text != null && InputBox.Text.Trim().Length >= 10;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
