using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class PerformanceModel
    {
        private string _state;

        public string State
        {
            get { return _state; }
            set { _state = value; }
        }

        public BitmapImage Box
        {
            get
            {
                if(State.Equals("W"))
                {
                    return new BitmapImage(new Uri(new ImagePathConverter().Convert(null, null, "won.png", null).ToString()));
                }
                if (State.Equals("D"))
                {
                    return new BitmapImage(new Uri(new ImagePathConverter().Convert(null, null, "draw.png", null).ToString()));
                }

                if (State.Equals("L"))
                {
                    return new BitmapImage(new Uri(new ImagePathConverter().Convert(null, null, "lost.png", null).ToString()));
                }

                return null;
            }
        }

    }
}
