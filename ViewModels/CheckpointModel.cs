using System;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels
{
    public class CheckpointModel
    {
        public Boolean IsLastCheckpoint { get; set; }
        public ProfitAccountingCheckpoint ProfitAccountingCheckpoint { get; set; }
    }
}
