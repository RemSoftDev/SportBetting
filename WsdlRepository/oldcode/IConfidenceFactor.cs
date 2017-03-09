using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SportRadar.DAL.ViewObjects;
using SportRadar.DAL.NewLineObjects;

namespace WsdlRepository.oldcode
{
    public interface IConfidenceFactor
    {
        decimal CalculateFactor(Ticket ticket);
    }
}
