using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportRadar.Common.Extensions
{
    public static class ConfigurationHelper
    {
        public static T AppSettings<T>(string name, T def) where T : class
        {
            var t = ConfigurationManager.AppSettings[name];
            if (t == null)
                return def;
            var type = typeof (T);
            if (type == typeof(int)) return (T)Convert.ChangeType(Convert.ToInt32(t), type);
            if (type == typeof (long)) return (T) Convert.ChangeType(Convert.ToInt64(t), type);
            if (type == typeof (string)) return (T)Convert.ChangeType(t, type);
            if (type == typeof (double)) return (T) Convert.ChangeType(Convert.ToDouble(t), type);
            return (T) Convert.ChangeType(t, type);
        }
    }
}
