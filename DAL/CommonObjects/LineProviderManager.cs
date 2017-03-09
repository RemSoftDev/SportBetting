using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;

namespace SportRadar.DAL.CommonObjects
{
    public static class LineProviderManager
    {
        private static readonly ILog m_logger = LogFactory.CreateLog(typeof(IdentityList));

        private static SyncDictionary<string, ILineProvider> m_diProviders = new SyncDictionary<string, ILineProvider>();

        private static List<Type> GetTypeList<T>(Assembly asm)
        {
            Type ti = typeof(T);

            List<Type> lTypes = new List<Type>();

            foreach (Type t in asm.GetTypes())
            {
                if (ti.IsAssignableFrom(t))
                {
                    lTypes.Add(t);
                }
            }

            return lTypes;
        }

        public static ILineProvider GetByUniqueName(string sUniqueName)
        {
            return m_diProviders.SafelyGetValue(sUniqueName);
        }

        public static void Refresh(string sPath)
        {
            m_diProviders.Clear();

            DirectoryInfo di = new DirectoryInfo(sPath);

            if (di.Exists)
            {
                FileInfo[] pluginFiles = di.GetFiles("*.dll");

                m_logger.InfoFormat("Found {0} dll file(s). Starting loading provider assemblies...", pluginFiles.Length);

                foreach (FileInfo fi in pluginFiles)
                {
                    Analyze(fi);
                }
            }
            else
            {
                m_logger.ErrorFormat("Cannot load line providers from {0} because this directory does not exis",new Exception(""), di.FullName);
            }
        }

        private static void Analyze(FileInfo sFileName)
        {
            try
            {
                Assembly asm = Assembly.LoadFile(sFileName.FullName);

                if (asm != null)
                {
                    List<Type> lTypes = GetTypeList<ILineProvider>(asm);

                    m_logger.InfoFormat("Analyzing assembly {0} ({1})", asm.FullName, sFileName);

                    foreach (Type t in lTypes)
                    {
                        try
                        {
                            ILineProvider provider = Activator.CreateInstance(t) as ILineProvider;
                            ExcpHelper.ThrowIf(string.IsNullOrEmpty(provider.UniqueName), "Cannot create provider instance of type {0} from dll '{1}'. Unique Name is empty", t, sFileName);
                            m_diProviders.Add(provider.UniqueName, provider);
                        }
                        catch (Exception excp)
                        {
                            m_logger.Excp(excp, "Cannot create provider instance of type {0} from dll '{1}'", t, sFileName);
                        }

                    }
                }
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "Cannot get provider(s) from dll '{0}'", sFileName);
            }
        }
    }
}
