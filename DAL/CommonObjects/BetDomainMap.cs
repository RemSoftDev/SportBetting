using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;

namespace SportRadar.DAL.CommonObjects
{
    public sealed class BetDomainMap
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(BetDomainMap));
        private XmlDocument m_doc = null;
//        private SyncDictionary<string, BetDomainMapItem> m_di = null;
        private SyncDictionary<int, BetDomainMapItem> m_di = null;

        private static BetDomainMap m_instance = null;

        public static BetDomainMap Instance
        {
            get { return m_instance; }
        }

        public XmlDocument Xml { get { return m_doc; } }

        private delegate void DelegateGenerateType(XmlElement el);

        private static void GenerateTypes (Type type, XmlNodeList nl, DelegateGenerateType dgt)
        {
            foreach (XmlElement el in nl)
            {
                try
                {
                    dgt(el);
                }
                catch (Exception excp)
                {
                    m_logger.Excp(excp, "Cannot create {0} from xml '{1}'", type.Name, el.OuterXml);
                }
            }
        }

        private static void GenerateBetType(XmlElement el)
        {
            MapItem mi = MapItem.FromXmlElement(el);

            BetTypeLn betType = new BetTypeLn();

            betType.Tag = mi.Tag;
            betType.Name = mi.Name;

            LineSr.Instance.AllObjects.BetTypes.MergeLineObject(betType);
        }

        private static void GenerateScoreType(XmlElement el)
        {
            MapItem mi = MapItem.FromXmlElement(el);

            ScoreTypeLn scoreType = new ScoreTypeLn();

            scoreType.Tag = mi.Tag;
            scoreType.Name = mi.Name;

            LineSr.Instance.AllObjects.ScoreTypes.MergeLineObject(scoreType);
        }

        private static void GenerateTimeType(string sTag, string sName)
        {
            TimeTypeLn timeType = new TimeTypeLn();

            timeType.Tag = sTag;
            timeType.Name = sName;

            LineSr.Instance.AllObjects.TimeTypes.MergeLineObject(timeType);
        }

        private delegate void DelegateGenericCycle(int iVal);

        private static void GenericCycle(string sGeneric, DelegateGenericCycle dgc)
        {
            string[] arr = sGeneric.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            int iStart = Convert.ToInt32(arr[0]);
            int iEnd   = Convert.ToInt32(arr[1]);

            for (int i = iStart; i <= iEnd; i ++)
            {
                dgc(i);
            }
        }

        private static void GenerateTimeType(XmlElement el)
        {
            MapItem mi = MapItem.FromXmlElement(el);

            if (mi.IsGeneric)
            {
                GenericCycle(mi.Generic, delegate(int iValue)
                {
                    string sTag = string.Format(mi.Tag, iValue);
                    string sName = string.Format(mi.Name, iValue);

                    GenerateTimeType(sTag, sName);
                });
            }
            else
            {
                GenerateTimeType(mi.Tag, mi.Name);
            }
        }

        private static void GenerateBetDomainType(BetDomainMapItem bdmi, int iSubPart, string sTag, string sTimeTag)
        {
            try
            {
                BetDomainMap.m_instance.m_di.SafelyAdd(bdmi.BetDomainNumber, bdmi);

                BetDomainTypeLn bdt = new BetDomainTypeLn();

                bdt.Tag = sTag;
                bdt.BetTypeTag = bdmi.BetType;
                bdt.ScoreTypeTag = bdmi.ScoreType;
                bdt.TimeTypeTag = sTimeTag;
                bdt.Sort = bdmi.Sort;
                bdt.Part = bdmi.Part;
                bdt.SubPart = iSubPart;
                bdt.TranslationArgsCount = bdmi.ArgsCount;

                LineSr.Instance.AllObjects.BetDomainTypes.MergeLineObject(bdt);
            }
            catch
            {   
                throw;
            }
        }

        private static void GenerateBetDomainType(XmlElement el)
        {
            BetDomainMapItem bdmi = BetDomainMapItem.FromXmlElement(el);

            BetDomainMap.m_instance.m_di.Add(bdmi.BetDomainNumber, bdmi);

            if (bdmi.IsGeneric)
            {
                GenericCycle(bdmi.Generic, delegate(int iValue)
                {
                    string sTag = string.Format(bdmi.BetTag, iValue);
                    string sTimeTag = string.Format(bdmi.TimeType, iValue);

                    GenerateBetDomainType(bdmi, iValue, sTag, sTimeTag);
                });
            }
            else
            {
                GenerateBetDomainType(bdmi, bdmi.SubPart, bdmi.BetTag, bdmi.TimeType);
            }
        }

        public static void Clear()
        {
            m_instance = null;
        }

        public static void EnsureInstance()
        {
            if (m_instance == null)
            {
                try
                {
                    m_instance = new BetDomainMap();

                    ExcpHelper.ThrowIf(!File.Exists(DalStationSettings.Instance.BetDomainMap), "BetDomain Map file does not exist {0}");

                    m_instance.m_di = new SyncDictionary<int, BetDomainMapItem>();

                    m_instance.m_doc = new XmlDocument();
                    m_instance.m_doc.Load(DalStationSettings.Instance.BetDomainMap);

                    GenerateTypes(typeof(BetTypeLn), m_instance.m_doc.SelectNodes("map/betType"), GenerateBetType);
                    GenerateTypes(typeof(ScoreTypeLn), m_instance.m_doc.SelectNodes("map/scoreType"), GenerateScoreType);
                    GenerateTypes(typeof(TimeTypeLn), m_instance.m_doc.SelectNodes("map/timeType"), GenerateTimeType);
                    GenerateTypes(typeof(BetDomainTypeLn), m_instance.m_doc.SelectNodes("map/betDomainType"), GenerateBetDomainType);
                }
                catch (Exception excp)
                {
                    m_logger.Excp(excp, "Cannot create instance");
                }
            }
        }

        public BetDomainMapItem GetBetDomainMapItem(int iBetDomainNumber)
        {
            //string sKey = BetDomainMapItem.GetKeyName(bdmn.Discriminator, bdmn.BetDomainNumber.ToString("G"), bdmn.IsLiveBet);

            return m_di.SafelyGetValue(iBetDomainNumber);
        }
    }

    public class MapItemBase
    {
        public string Generic { get; protected set; }

        public bool IsGeneric { get { return !string.IsNullOrEmpty(this.Generic); } }
    }

    public sealed class MapItem : MapItemBase
    {
        public string Tag { get; private set; }
        public string Name { get; private set; }

        private MapItem()
        {
            
        }

        public static MapItem FromXmlElement(XmlElement el)
        {
            MapItem mi = new MapItem();

            mi.Tag = el.GetAttribute("tag");
            mi.Name = el.GetAttribute("name");
            mi.Generic = el.GetAttribute("generic");

            ExcpHelper.ThrowIf(string.IsNullOrEmpty(mi.Tag), "tag node is not set.");
            ExcpHelper.ThrowIf(string.IsNullOrEmpty(mi.Name), "name node is not set.");

            return mi;
        }
    }

    public class BetDomainMapItem : MapItemBase
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(BetDomainMapItem));

        public  const int  PART_OVERTIME = -1;
        public  const int  PART_UNKNOWN  = 0;

        public string Discriminator { get; private set; }
        public int    BetDomainNumber { get; private set; }
        public bool   IsLiveBet { get; private set; }
        public string BetTag { get; private set; }
        public string BetType { get; private set; }
        public string ScoreType { get; private set; }
        public string TimeType { get; private set; }
        public int    Sort { get; private set; }
        public int    Part { get; private set; }
        public int    SubPart { get; private set; }
        public int    ArgsCount { get; private set; }

        private BetDomainMapItem()
        {
            
        }

        /*
        public string KeyName
        {
            get { return GetKeyName(this.Discriminator, this.BetDomainNumber, this.IsLiveBet); }
        }

        public static string GetKeyName(string sDiscriminator, string sBetDomainNumber, bool bIsLiveBet)
        {
            return string.Format("{0}{1}{2}{3}{4}", sDiscriminator, KEY_SEPARATOR, sBetDomainNumber, KEY_SEPARATOR, bIsLiveBet ? "1" : "0");
        }
        */

        public static BetDomainMapItem FromXmlElement(XmlElement el)
        {
            try
            {
                BetDomainMapItem bdmi = new BetDomainMapItem();

                string sIsLiveBet = el.GetAttribute("isLiveBet");

                string sBetDomainNumber = el.GetAttribute("betDomainNumber");
                string sSort = el.GetAttribute("sort");
                string sPart = el.GetAttribute("part");
                string sSubPart = el.GetAttribute("subPart");
                string sArgsPart = el.GetAttribute("argsCount");

                bdmi.Discriminator = el.GetAttribute("discriminator");
                bdmi.IsLiveBet = sIsLiveBet == "1";
                bdmi.BetTag = el.GetAttribute("betTag");
                bdmi.BetType = el.GetAttribute("betType");
                bdmi.ScoreType = el.GetAttribute("scoreType");
                bdmi.TimeType = el.GetAttribute("timeType");
                bdmi.Generic = el.GetAttribute("generic");

                //ExcpHelper.ThrowIf(string.IsNullOrEmpty(bdmi.Discriminator), "discriminator node is not set.");
                ExcpHelper.ThrowIf(string.IsNullOrEmpty(sBetDomainNumber), "betDomainNumber node is not set.");
                ExcpHelper.ThrowIf(string.IsNullOrEmpty(sIsLiveBet), "isLiveBet node is not set.");
                ExcpHelper.ThrowIf(string.IsNullOrEmpty(bdmi.BetTag), "betTag node is not set.");
                ExcpHelper.ThrowIf(string.IsNullOrEmpty(bdmi.BetType), "betType node is not set.");
                ExcpHelper.ThrowIf(string.IsNullOrEmpty(bdmi.ScoreType), "scoreType node is not set.");
                ExcpHelper.ThrowIf(string.IsNullOrEmpty(bdmi.TimeType), "timeType node is not set.");
                ExcpHelper.ThrowIf(string.IsNullOrEmpty(sSort), "sort node is not set.");
                ExcpHelper.ThrowIf(string.IsNullOrEmpty(sPart), "part node is not set.");

                bdmi.BetDomainNumber = Convert.ToInt32(sBetDomainNumber);
                bdmi.Sort = Convert.ToInt32(sSort);
                bdmi.Part = Convert.ToInt32(sPart);

                try
                {
                    bdmi.SubPart = string.IsNullOrEmpty(sSubPart) ? 0 : Convert.ToInt32(sSubPart);
                }
                catch (Exception)
                {
                    bdmi.SubPart = 0;
                }

                bdmi.ArgsCount = string.IsNullOrEmpty(sArgsPart) ? 0 : Convert.ToInt32(sArgsPart);

                return bdmi;
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "BetDomainMapItem() ERROR.");
                throw;
            }
        }
    }
}
