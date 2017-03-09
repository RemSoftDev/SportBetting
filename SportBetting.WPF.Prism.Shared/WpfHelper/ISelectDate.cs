using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SportBetting.WPF.Prism.Shared.WpfHelper
{
    public interface ISelectDate
    {
        DateTime? SelectDate(DateTime? initialDate, DateTime? minDate, DateTime? maxDate);
    }
}
