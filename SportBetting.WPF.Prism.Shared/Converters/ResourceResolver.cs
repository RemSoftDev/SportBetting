using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

using System.Configuration;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ninject;
using SportRadar.Common.Logs;
using WsdlRepository;


namespace SportBetting.WPF.Prism.Shared
{
    public class ResourceResolver : MarkupExtension
    {
        #region Private Members
        private static readonly ILog _logger = LogFactory.CreateLog(typeof(ResourceResolver));
        private static IDictionary<string, object> imagecash = new Dictionary<string, object>(100);
        #endregion

        #region Construction

        public ResourceResolver(string path)
        {
            ResourceKey = path;
        }
        public ResourceResolver()
        {
        }

        #endregion

        [ConstructorArgument("ResourceKey")]
        public string ResourceKey { get; set; }

        object _addLocker = new object();
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (imagecash.ContainsKey(ResourceKey))
                return imagecash[ResourceKey];
            var path = "";


            try
            {
                lock (_addLocker)
                {
                    var resource = Application.Current.FindResource(ResourceKey);
                    if (!imagecash.ContainsKey(ResourceKey))
                        imagecash.Add(ResourceKey, resource);
                    return resource;
                }

            }
            catch (Exception e)
            {
                lock (_addLocker)
                {
                    if (!imagecash.ContainsKey(ResourceKey))
                        imagecash.Add(ResourceKey, null);

                    _logger.Error("can't find image:" + path, e);
                    return null;
                }
            }

        }
       
    }
}
