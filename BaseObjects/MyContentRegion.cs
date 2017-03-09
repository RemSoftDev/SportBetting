using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BaseObjects.ViewModels;

namespace BaseObjects
{
    public class MyContentRegion : ContentControl
    {
        private IDictionary<string, MyContentRegion> _childRegions = new Dictionary<string, MyContentRegion>();

        public bool IsVirtualRegion { get; set; }
        public string VirtualRegionName { get; set; }

        public IDictionary<string, MyContentRegion> ChildRegions
        {
            get { return _childRegions; }
            set { _childRegions = value; }
        }

        public MyContentRegion ActiveRegion
        {
            get
            {
                var result = this;
                foreach (var childRegion in ChildRegions)
                {
                    if (childRegion.Value.Visibility == Visibility.Visible)
                        result = childRegion.Value;
                }
                return result;
            }
        }

        public void Close()
        {
            var oldContent = (UserControl)this.Content;
            this.Visibility = Visibility.Collapsed;
            if (oldContent != null)
            {
                if (oldContent.DataContext != null)
                {
                    var baseViewModel = oldContent.DataContext as IBaseViewModel;
                    if (baseViewModel != null)
                    {
                        baseViewModel.Close();
                    }
                }
            }
            
        }
    }
}
