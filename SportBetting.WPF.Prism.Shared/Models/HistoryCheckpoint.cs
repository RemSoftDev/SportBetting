using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SportBetting.WPF.Prism.Shared.Annotations;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class HistoryCheckpoint : INotifyPropertyChanged
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Decimal SettlementSaldo { get; set; }

        private List<SettlementHistoryDetail> checkpointDetails;
        public List<SettlementHistoryDetail> CheckpointDetails { get { return checkpointDetails; } set { checkpointDetails = value; OnPropertyChanged("CheckpointDetails"); } }

        public bool IsSelected { get; set; }

        public int Id { get; set; }

        private bool isVisible;
        public bool IsVisible { get { return isVisible; } set { isVisible = value; OnPropertyChanged("IsVisible"); } }

        public int index { get; set; }

        public HistoryCheckpoint(DateTime startDate, DateTime endDate, int id, decimal sSaldo)
        {
            Id = id;
            StartDate = startDate;
            EndDate = endDate;
            SettlementSaldo = sSaldo;
            //CheckpointDetails = checkpointDetails;
            IsSelected = false;
            IsVisible = false;
            index = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
