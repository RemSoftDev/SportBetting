using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;

namespace SportRadar.DAL.CommonObjects
{
    public class SerializableObjectList : SyncList<ISerializableObject>
    {
        public Type ContentType { get; protected set; }
        public string XmlTagName { get; protected set; }

        public SerializableObjectList(Type contentType, string sXmlTagName)
        {
            this.ContentType = contentType;
            this.XmlTagName = sXmlTagName;
        }

        public SerializableProperty GetSerializableProperty(string sPropertyName)
        {
            return null;
        }

        public void ToXml(XmlTextWriter tw)
        {
            tw.WriteStartElement(this.XmlTagName);

            foreach (ISerializableObject obj in this)
            {
                obj.ToXml(tw);
            }

            tw.WriteEndElement();
        }

        public void FromXml(XmlTextReader tr)
        {
            while (tr.Read())
            {
                string sCurrentTagName = tr.Name;

                switch (tr.NodeType)
                {
                    case XmlNodeType.Element:

                        if (sCurrentTagName == this.XmlTagName)
                        {
                            break;
                        }

                        ISerializableObject obj = null;

                        try
                        {
                            obj = new SerializableObject(this.ContentType);
                            obj.FromXml(tr);

                            this.Add(obj);
                        }
                        catch (Exception excp)
                        {
                            
                            throw;
                        }


                        break;

                    case XmlNodeType.Text:

                        break;

                    case XmlNodeType.EndElement:

                        if (tr.Name == this.XmlTagName)
                        {
                            return;
                        }

                        break;
                }
            }
        }
    }

    public class LineListDictionary<T> : SyncDictionary<string, SerializableObjectList> where T : ISerializableObject
    {
        public SerializableObjectList EnsureSerializableObjectList(Type contentType, string sObjectListName)
        {
            SerializableObjectList lObjects = null;

            if (this.ContainsKey(sObjectListName))
            {
                lObjects = this[sObjectListName];
            }
            else
            {
                lObjects = new SerializableObjectList(contentType, sObjectListName);
                base.Add(sObjectListName, lObjects);
            }

            return lObjects;
        }
    }

    public sealed class LineContainer : DynamicObject, ISerializableObject
    {
        public const string ATTR_DOC_ID    = "docid";
        public const string ATTR_TYPE      = "type";
        public const long   ERROR_DOC_ID   = - 1;

        public const string SUFFIX = "List";
        public const string BASE_NAMESPACE = "SportRadar.DAL.NewLineObjects.";

        private static ILog m_logger = LogFactory.CreateLog(typeof (LineContainer));

        private LineListDictionary<SerializableObject> m_di = new LineListDictionary<SerializableObject>();

        public LineListDictionary<SerializableObject> Objects { get { return m_di; } }

        public SyncDictionary<string, string> Attributes { get; private set; } 

        private string m_sTrace = string.Empty;

        public string OriginalXml { get; private set; }

        public LineContainer()
        {
            this.Attributes = new SyncDictionary<string, string>();
        }

        public long GetDocId()
        {
            long lDocId = 0;

            return long.TryParse(this.Attributes.SafelyGetValue(ATTR_DOC_ID), out lDocId) ? lDocId : ERROR_DOC_ID;
        }

        public string GetType()
        {
            return this.Attributes.SafelyGetValue(ATTR_TYPE);
        }

        public static string ContentTypeToObjectListName(Type contentType) 
        {
            return contentType.Name + SUFFIX;
        }

        public static Type ObjectListNameToContentType(string sObjectListName)
        {
            if (sObjectListName.EndsWith(SUFFIX))
            {
                sObjectListName = sObjectListName.Substring(0, sObjectListName.Length - SUFFIX.Length);

                string sTypeName = BASE_NAMESPACE + sObjectListName;

                Assembly asm = Assembly.GetExecutingAssembly();

                Type type = asm.GetType(sTypeName);

                return type;
            }

            return null;
        }

        public SerializableProperty GetSerializableProperty(string sPropertyName)
        {
            return null;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (m_di.ContainsKey(binder.Name))
            {
                result = m_di[binder.Name];

                return true;
            }

            result = null;

            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return false;
        }

        public void ToXml(XmlTextWriter tw)
        {
            tw.WriteStartElement(this.XmlTagName);

            foreach (SerializableObjectList lObjects in m_di.Values)
            {
                lObjects.ToXml(tw);
            }

            tw.WriteEndElement();
        }

        public string ToXmlString()
        {
            try
            {
                using (StringWriter sw = new StringWriter())
                {
                    using (XmlTextWriter tw = new XmlTextWriter(sw) { Formatting = Formatting.Indented, Indentation = 4 })
                    {
                        this.ToXml(tw);
                        return sw.ToString();
                    }
                }
            }
            catch (Exception excp)
            {
                //m_logger.Excp(excp, "LineSerializeHelper.ObjectToString({0}) ERROR", oInstance);
            }

            return string.Empty;
        }

        public string XmlTagName { get { return "container"; } }
        public bool IsToRemove() { return false; }
        public SyncList<SerializableProperty> GetProperties() { return null; }

        private delegate string DelegateFormatString(dynamic so);

        private void BuildTraceString<T>(DelegateFormatString dfs)
        {
            string sObjectListName = LineContainer.ContentTypeToObjectListName(typeof(T));
            SerializableObjectList lObjects = m_di.SafelyGetValue(sObjectListName);

            if (lObjects != null && lObjects.Count > 0)
            {
                List<string> l = new List<string>();

                foreach (ISerializableObject so in lObjects)
                {
                    string sData = dfs(so);
                    l.Add(sData);
                }

                m_sTrace += string.Format("{0} (Count={1}): {2}\r\n", sObjectListName, l.Count, string.Join(", ", l.ToArray()));
            }
        }

        private static string spget (SerializableProperty sp)
        {
            return sp != null && sp.PropertyValue != null ? sp.PropertyValue.ToString() : string.Empty;
        }

        public string BuildTraceString()
        {
            m_sTrace = string.Empty;

            BuildTraceString<TimeTypeLn>(delegate(dynamic so) { return string.Format("{0}:'{1}'", spget(so.Tag), spget(so.Name)); });
            BuildTraceString<ScoreTypeLn>(delegate(dynamic so) { return string.Format("{0}:'{1}'", spget(so.Tag), spget(so.Name)); });
            BuildTraceString<BetTypeLn>(delegate(dynamic so) { return string.Format("{0}:'{1}'", spget(so.Tag), spget(so.Name)); });
            BuildTraceString<BetDomainTypeLn>(delegate(dynamic so) { return string.Format("{0}:'{1}'", spget(so.Tag), spget(so.Name)); });

            BuildTraceString<GroupLn>(delegate(dynamic so) { return string.Format("{0}:'{1}'", spget(so.GroupId), spget(so.Type)); });
            BuildTraceString<CompetitorLn>(delegate(dynamic so) { return string.Format("{0}", spget(so.CompetitorId)); });
            BuildTraceString<TaggedStringLn>(delegate(dynamic so) { return string.Format("{0}:'{1}':'{2}':'{3}':{4}", spget(so.StringId), spget(so.Category), spget(so.Tag), spget(so.Language), spget(so.ObjectId)); });
            BuildTraceString<MatchLn>(delegate(dynamic so) { return string.Format("{0}:{1}", spget(so.MatchId), spget(so.BtrMatchId)); });
            BuildTraceString<MatchToGroupLn>(delegate(dynamic so) { return string.Format("{0}:{1}", spget(so.MatchId), spget(so.GroupId)); });
            BuildTraceString<LiveMatchInfoLn>(delegate(dynamic so) { return string.Format("{0}:'{1}':'{2}'", spget(so.MatchId), spget(so.Status), spget(so.PeriodInfo)); });
            BuildTraceString<BetDomainLn>(delegate(dynamic so) { return string.Format("{0}:{1}:'{2}'", spget(so.BetDomainId), spget(so.MatchId), spget(so.Status)); });
            BuildTraceString<OddLn>(delegate(dynamic so) { return string.Format("{0}:{1}:{2}:{3}", spget(so.OutcomeId), spget(so.OddId), spget(so.BetDomainId), spget(so.Value)); });

            return m_sTrace;
        }

        public void FromXml(XmlTextReader tr)
        {
            CheckTime ct = new CheckTime(false, "LineContainer.FromXml() entered.");

            try
            {
                while (tr.Read())
                {
                    string sCurrentTagName = tr.Name;

                    switch (tr.NodeType)
                    {
                        case XmlNodeType.Attribute:

                            break;

                        case XmlNodeType.Element:

                            while (tr.MoveToNextAttribute())
                            {
                                this.Attributes[tr.Name] = tr.Value;
                            }

                            if (sCurrentTagName == this.XmlTagName)
                            {
                                break;
                            }

                            if (!tr.IsEmptyElement)
                            {
                                try
                                {
                                    Type contentType = ObjectListNameToContentType(sCurrentTagName);
                                    SerializableObjectList lObjects = m_di.EnsureSerializableObjectList(contentType, sCurrentTagName);
                                    lObjects.FromXml(tr);
                                    ct.AddEvent("{0} completed", sCurrentTagName);
                                }
                                catch (Exception excp)
                                {
                                    
                                    throw;
                                }
                            }

                            break;

                        case XmlNodeType.EndElement:

                            if (tr.Name == this.XmlTagName)
                            {
                                return;
                            }

                            break;
                    }
                }
            }
            finally
            {
                ct.AddEvent("LineContainer.FromXml() completed.");
                ct.Info(m_logger);
            }
        }

        public static LineContainer FromXml(string sXml)
        {
            try
            {

                using (StringReader sr = new StringReader(sXml))
                {
                    using (XmlTextReader tr = new XmlTextReader(sr))
                    {
                        LineContainer lc = new LineContainer();
                        lc.FromXml(tr);
                        lc.OriginalXml = sXml;

                        return lc; // 4369910973073
                    }
                }

            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "FromXml() ERROR");
                throw;
            }

            return null;
        }

        public static LineContainer FromStream(Stream st)
        {
            try
            {
                using (XmlTextReader tr = new XmlTextReader(st))
                {
                    LineContainer lc = new LineContainer();
                    lc.FromXml(tr);

                    return lc; // 4369910973073
                }
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "FromStream() ERROR");
                throw;
            }

            return null;
        }

        public object MethodTag { get; set; }

        [Obsolete]
        public T GetObject<T>() where T : ILineObject<T>
        {
            throw new InvalidOperationException("GetObject() method is not allowed");
        }
    }
}
