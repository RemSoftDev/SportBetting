using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace SportRadar.DAL.OldLineObjects
{    
    public class ActiveTournament
    {
        [XmlText()]
        public string Id { get; set; }
    }
}
