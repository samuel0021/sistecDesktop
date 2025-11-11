using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace sistecDesktop.Converters
{
    public class PriorityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int prioridade = value is int p ? p : 1;
            switch (prioridade)
            {
                case 1: return "Baixa";
                case 2: return "Média";
                case 3: return "Alta";
                case 4: return "Urgente";
                default: return "Desconhecida";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
