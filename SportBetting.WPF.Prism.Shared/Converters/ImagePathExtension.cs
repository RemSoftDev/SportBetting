using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using Ninject;
using SportRadar.Common.Logs;
using TranslationByMarkupExtension;
using WsdlRepository;

namespace SportBetting.WPF.Prism.Shared.Converters
{

    public class ResolveImagePath : MarkupExtension
    {
        #region Private Members
        private static readonly string ImageRelativePath = ConfigurationManager.AppSettings["images_relative_path"];
        private static readonly string WorkingDirectory =
                            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(ImagePathConverter)).Location);

        private string _path;
        private static readonly ILog _logger = LogFactory.CreateLog(typeof(ResolveImagePath));
        private static IDictionary<string, BitmapImage> imagecash = new Dictionary<string, BitmapImage>(1000);
        #endregion

        #region Construction

        public ResolveImagePath(string path)
        {
            _path = path;
        }
        public ResolveImagePath()
        {
        }

        #endregion
        private static IStationRepository StationRepository = IocContainer.IoCContainer.Kernel.Get<IStationRepository>();

        [ConstructorArgument("Path")]
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        private string _layoutName = null;
        private string LayoutName
        {
            get
            {
                if (_layoutName == null)
                {
                    if (!string.IsNullOrEmpty(StationRepository.LayoutName))
                        _layoutName = StationRepository.LayoutName;
                    _layoutName = "Defaultviews";
                }
                return _layoutName;
            }
        }

        /// <summary>
        /// See <see cref="MarkupExtension.ProvideValue" />
        /// </summary>
        /// 
        object _addLocker = new object();
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (imagecash.ContainsKey(_path))
                return imagecash[_path];
            var path = "";
            if (_path != null)
            {
                if (File.Exists(WorkingDirectory + ImageRelativePath + LayoutName + "\\" + _path))
                {
                    path = WorkingDirectory + ImageRelativePath + LayoutName + "\\" + _path;
                }
                else
                {
                    path = "pack://application:,,,/" + LayoutName + ";component/Resources/Images/" + LayoutName + "/" + _path;
                }
            }

            try
            {
                lock (_addLocker)
                {
                    var bitmap = new BitmapImage(new Uri(path, UriKind.Absolute));
                    bitmap.Freeze();
                    if (!imagecash.ContainsKey(_path))
                        imagecash.Add(_path, bitmap);
                    return bitmap;
                }

            }
            catch (Exception e)
            {
                lock (_addLocker)
                {
                    if (!imagecash.ContainsKey(_path))
                        imagecash.Add(_path, null);

                    _logger.Error("can't find image:" + path, e);
                    return null;
                }
            }

        }
        private static ResolveImagePath _resolveImagePath = new ResolveImagePath();
        public static object ResolvePath(string path)
        {
            _resolveImagePath.Path = path;
            return _resolveImagePath.ProvideValue(null);
        }
    }
}
