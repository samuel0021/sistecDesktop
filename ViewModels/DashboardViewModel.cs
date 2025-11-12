using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using sistecDesktop.Commands;
using sistecDesktop.Models;
using sistecDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace sistecDesktop.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ApiClient _apiClient;

        private DashboardStats _stats;
        public DashboardStats Stats
        {
            get => _stats;
            set
            {
                _stats = value;
                OnPropertyChanged(nameof(Stats));
                OnPropertyChanged(nameof(ChamadosAbertos));
                OnPropertyChanged(nameof(ChamadosResolvidos));
                OnPropertyChanged(nameof(ChamadosRejeitados));
                OnPropertyChanged(nameof(ChamadosEmAndamento));
                OnPropertyChanged(nameof(ChamadosTotal));
            }
        }

        // Cards - Calculados conforme estrutura do JSON
        public int ChamadosAbertos => Stats?.abertos ?? 0;
        public int ChamadosResolvidos => Stats?.resolvidos ?? 0;
        public int ChamadosRejeitados => Stats?.rejeitados ?? 0;
        public int ChamadosEmAndamento =>
            (Stats?.triagem_ia ?? 0)
            + (Stats?.aguardando_resposta ?? 0)
            + (Stats?.com_analista ?? 0);

        public int ChamadosTotal => Stats?.total ?? 0;


        // Gráficos

        private SeriesCollection _analystSeries;
        public SeriesCollection AnalystSeries
        {
            get => _analystSeries;
            set { _analystSeries = value; OnPropertyChanged(nameof(AnalystSeries)); }
        }
        private string[] _analystLabels;
        public string[] AnalystLabels
        {
            get => _analystLabels;
            set { _analystLabels = value; OnPropertyChanged(nameof(AnalystLabels)); }
        }
        public int AnalystTotal => _analystData?.Sum(x => x.chamados) ?? 0;
        public int AnalystCount => _analystData?.Count ?? 0;
        public double AnalystMedia => AnalystCount > 0 ? (double)AnalystTotal / AnalystCount : 0;

        private List<AnalystDataItem> _analystData;



        private SeriesCollection _barSeries;
        public SeriesCollection BarSeries
        {
            get => _barSeries;
            set { _barSeries = value; OnPropertyChanged(nameof(BarSeries)); }
        }
        private string[] _mesesLabels;
        public string[] MesesLabels
        {
            get => _mesesLabels;
            set { _mesesLabels = value; OnPropertyChanged(nameof(MesesLabels)); }
        }

        private SeriesCollection _pieSeries;
        public SeriesCollection PieSeries
        {
            get => _pieSeries;
            set { _pieSeries = value; OnPropertyChanged(nameof(PieSeries)); }
        }

        private SeriesCollection _lineSeries;
        public SeriesCollection LineSeries
        {
            get => _lineSeries;
            set { _lineSeries = value; OnPropertyChanged(nameof(LineSeries)); }
        }
        private string[] _anosLabels;
        public string[] AnosLabels
        {
            get => _anosLabels;
            set { _anosLabels = value; OnPropertyChanged(nameof(AnosLabels)); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public ICommand LoadDashboardCommand { get; }

        public DashboardViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            _analystData = new List<AnalystDataItem>();
            LoadDashboardCommand = new AsyncRelayCommand(LoadDashboardAsync);

            AnalystSeries = new SeriesCollection();
            AnalystLabels = new string[0];

            _ = LoadDashboardAsync();
        }

        public async Task LoadDashboardAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                // Cards do topo
                Stats = await _apiClient.GetDashboardStatsAsync() ?? new DashboardStats();

                // Barras por mês
                var mensais = await _apiClient.GetMonthlyDataAsync();
                MesesLabels = mensais.Select(m => m.month).ToArray();
                BarSeries = new SeriesCollection {
                    new ColumnSeries {
                        Title = "Chamados",
                        Values = new ChartValues<int>(mensais.Select(m => m.value))
                    }
                };

                // Pizza por categoria
                var cores = new[] { Brushes.MediumPurple, Brushes.Orange, Brushes.Gray, Brushes.DeepSkyBlue, Brushes.Green, Brushes.Red, Brushes.CadetBlue };
                var categorias = await _apiClient.GetCategoryDataAsync();
                PieSeries = new SeriesCollection();
                for (int i = 0; i < categorias.Count; i++)
                {
                    var item = categorias[i];
                    PieSeries.Add(new PieSeries
                    {
                        Title = item.name,
                        Values = new ChartValues<int> { item.value },
                        Fill = cores[i % cores.Length]
                    });
                }

                // Linha anual
                var anuais = await _apiClient.GetYearlyDataAsync();
                AnosLabels = anuais.Select(y => y.month).ToArray();
                LineSeries = new SeriesCollection {
                    new LineSeries {
                        Title = "Abertos",
                        Values = new ChartValues<int>(anuais.Select(y => y.abertos)),
                        PointGeometry = DefaultGeometries.Circle,
                        Stroke = Brushes.MediumSlateBlue,
                        Fill = Brushes.Transparent
                    },
                    new LineSeries {
                        Title = "Resolvidos",
                        Values = new ChartValues<int>(anuais.Select(y => y.resolvidos)),
                        PointGeometry = DefaultGeometries.Square,
                        Stroke = Brushes.OrangeRed,
                        Fill = Brushes.Transparent
                    },
                };

                var analysts = await _apiClient.GetAnalystDataAsync();
                _analystData = analysts; // Inicialize em algum lugar do construtor como new List<AnalystDataItem>();
                AnalystSeries = new SeriesCollection
                {
                    new RowSeries
                    {
                        Title = "Chamados",
                        Values = new ChartValues<int>(analysts.Select(a => a.chamados)),
                        DataLabels = true
                    }
                };
                AnalystLabels = analysts.Select(a => a.nome).ToArray();
                OnPropertyChanged(nameof(AnalystTotal));
                OnPropertyChanged(nameof(AnalystCount));
                OnPropertyChanged(nameof(AnalystMedia));
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro ao carregar dashboard: " + ex.Message;
            }
            finally { IsLoading = false; }
        }



    }
}
