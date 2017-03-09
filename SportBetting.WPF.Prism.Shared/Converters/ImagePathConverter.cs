using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;

using System.Configuration;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ninject;
using SportRadar.Common.Logs;
using WsdlRepository;


namespace SportBetting.WPF.Prism.Shared
{
    public class ImagePathConverter : IValueConverter
    {
        private static readonly string ImageRelativePath = ConfigurationManager.AppSettings["images_relative_path"];
        private static readonly string WorkingDirectory =
                            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(ImagePathConverter)).Location);
        #region Constructors
        #endregion
        private static readonly ILog _logger = LogFactory.CreateLog(typeof(ImagePathConverter));
        private static IDictionary<string, BitmapImage> imagecash = new Dictionary<string, BitmapImage>(100);

        private static IStationRepository StationRepository = IocContainer.IoCContainer.Kernel.Get<IStationRepository>();

        #region IValueConverter Members
        /* public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = "";
            if (parameter != null)
            {
                path = WorkingDirectory + ImageRelativePath + parameter;
            }
            else
            {
                path = WorkingDirectory + ImageRelativePath + value;
            }
            if (!File.Exists(path))
            {
                return null;
            }
            var bitmap = new BitmapImage(new Uri(path));
            bitmap.Freeze();
            return bitmap;
        }    */

        private string LayoutName
        {
            get
            {
                if (!string.IsNullOrEmpty(StationRepository.LayoutName))
                    return StationRepository.LayoutName;
                return "Defaultviews";
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {



            var path = "";
            if (parameter != null)
            {
                if (File.Exists(WorkingDirectory + ImageRelativePath + LayoutName + "\\" + parameter))
                {
                    path = WorkingDirectory + ImageRelativePath + LayoutName + "\\" + parameter;
                }
                else
                    path = "pack://application:,,,/"+LayoutName+";component/Resources/Images/"+LayoutName+"/" + parameter;
            }
            else
            {
                if (File.Exists(WorkingDirectory + ImageRelativePath + LayoutName + "\\" + value))
                {
                    path = WorkingDirectory + ImageRelativePath + LayoutName + "\\" + value;
                }
                else
                    path = "pack://application:,,,/" + LayoutName + ";component/Resources/Images/" + LayoutName + "/" + value;
            }

            if (imagecash.ContainsKey(path))
                return imagecash[path];

            try
            {
                var bitmap = new BitmapImage(new Uri(path, UriKind.Absolute));
                bitmap.Freeze();
                imagecash.Add(path, bitmap);
                return bitmap;

            }
            catch (Exception e)
            {

                _logger.Error("can't find image:" + path,e);
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
