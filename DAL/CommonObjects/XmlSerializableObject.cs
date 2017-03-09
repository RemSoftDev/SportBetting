using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
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
    public abstract class SerializableProperty
    {
        protected static ILog m_logger = LogFactory.CreateLog(typeof(SerializableProperty));

        protected Type m_type = typeof(object);
        public bool IsSpecified { get; protected set; }
        public string PropertyName { get; protected set; }
        public Type PropertyType { get { return m_type; } }
        public object PropertyValue { get; set; }

        private delegate SerializableProperty DelegateCreateSerializableProperty();
        private static readonly SyncDictionary<Type, DelegateCreateSerializableProperty> diSerializablePropertyFactory = new SyncDictionary<Type, DelegateCreateSerializableProperty>()
        {
            {typeof(int), delegate() { return new SerializableProperty<int>(); }},
            {typeof(int?), delegate() { return new SerializableProperty<int?>(); }},
            {typeof(long), delegate() { return new SerializableProperty<long>(); }},
            {typeof(long?), delegate() { return new SerializableProperty<long?>(); }},
            {typeof(decimal), delegate() { return new SerializableProperty<decimal>(); }},
            {typeof(decimal?), delegate() { return new SerializableProperty<decimal?>(); }},
            {typeof(bool), delegate() { return new SerializableProperty<bool>(); }},
            {typeof(bool?), delegate() { return new SerializableProperty<bool?>(); }},
            {typeof(string), delegate() { return new SerializableProperty<string>(); }},
            {typeof(DateTime), delegate() { return new SerializableProperty<DateTime>(); }},
            {typeof(DateTime?), delegate() { return new SerializableProperty<DateTime?>(); }},
            {typeof(DateTimeSr), delegate() { return new SerializableProperty<DateTimeSr>(); }},
            {typeof(eBetDomainStatus), delegate() { return new SerializableProperty<eBetDomainStatus>(); }},
            {typeof(eMatchStatus), delegate() { return new SerializableProperty<eMatchStatus>(); }},
            {typeof(eLivePeriodInfo), delegate() { return new SerializableProperty<eLivePeriodInfo>(); }},
            {typeof(eServerSourceType), delegate() { return new SerializableProperty<eServerSourceType>(); }},
            {typeof(eOutrightType), delegate() { return new SerializableProperty<eOutrightType>(); }},
        };

        private delegate object DelegateGetDefaultValue();
        private static readonly SyncDictionary<Type, DelegateGetDefaultValue> diDefaultValueFactory = new SyncDictionary<Type, DelegateGetDefaultValue>()
        {
            {typeof(int), delegate() { return 0; }},
            {typeof(long), delegate() { return 0L; }},
            {typeof(decimal), delegate() { return 0.0m; }},
            {typeof(bool), delegate() { return false; }},
            {typeof(DateTime), delegate() { return DateTime.MinValue; }},
            {typeof(eBetDomainStatus), delegate() { return eBetDomainStatus.Visible; }},
            {typeof(eMatchStatus), delegate() { return eMatchStatus.NotStarted; }},
            {typeof(eLivePeriodInfo), delegate() { return eLivePeriodInfo.NotStarted; }},
            {typeof(eServerSourceType), delegate() { return eServerSourceType.BtrPre; }},
            {typeof(eOutrightType), delegate() { return eOutrightType.None; }},
        };

        private static SerializableProperty CreateSerializableProperty(Type propertyType, string sPropertyName, object oValue, bool bIsSpecified)
        {
            DelegateCreateSerializableProperty dcsp = diSerializablePropertyFactory.SafelyGetValue(propertyType);
            ExcpHelper.ThrowIf(dcsp == null, "CreateSerializableProperty({0}, '{1}') ERROR", propertyType, sPropertyName);

            SerializableProperty sp = dcsp();
            sp.PropertyName = sPropertyName;
            sp.PropertyValue = oValue;
            sp.IsSpecified = bIsSpecified;

            return sp;
        }

        public static SerializableProperty CreateSerializableProperty(Type propertyType, string sPropertyName, object oValue)
        {
            return CreateSerializableProperty(propertyType, sPropertyName, oValue, true);
        }

        public static SerializableProperty CreateSerializableProperty<T>(string sPropertyName, T value)
        {
            return CreateSerializableProperty(typeof(T), sPropertyName, value, true);
        }

        public static SerializableProperty CreateSerializableProperty(Type propertyType, string sPropertyName)
        {
            DelegateGetDefaultValue dgdv = diDefaultValueFactory.SafelyGetValue(propertyType);

            return CreateSerializableProperty(propertyType, sPropertyName, dgdv != null ? dgdv() : null, false);
        }

        public static SerializableProperty CreateSerializableProperty<T>(string sPropertyName)
        {
            return CreateSerializableProperty(typeof(T), sPropertyName, default(T), false);
        }

        protected SerializableProperty()
        {
            this.IsSpecified = false;
        }
    }

    public sealed class SerializableProperty<T> : SerializableProperty
    {
        public SerializableProperty() : base ()
        {
            m_type = typeof(T);
        }

        public T Value { get { return (T)this.PropertyValue; }}

        public override string ToString()
        {
            return string.Format("SerializableProperty<{0}> {{Name='{1}', Value='{2}', IsSpecified={3}}}", m_type, this.PropertyName, this.Value, this.IsSpecified);
        }
    }

    public sealed class SerializableObject : DynamicObject, ISerializableObject
    {
        private const string COMMAND_REMOVE = "remove";

        private static ILog m_logger = LogFactory.CreateLog(typeof(SerializableObject));

        private SyncDictionary<string, SerializableProperty> m_di = new SyncDictionary<string, SerializableProperty>();

        protected Type m_type = null;

        public SerializableObject(Type type)
        {
            m_type = type;
        }

        public Type ObjectType
        {
            get { return m_type; }
        }

        public SyncList<SerializableProperty> GetProperties()
        {
            return m_di.ToSyncList();
        }

        private SerializablePropertyDictionary m_PropTypes = null;

        private SerializablePropertyDictionary PropTypes
        {
            get
            {
                if (m_PropTypes == null)
                {
                    if (m_type == typeof (MatchLn))
                    {
                        
                    }

                    m_PropTypes = PropertyInfoDictionary<Type>.GetSerializablePropertyDictionary(m_type);
                }

                Debug.Assert(m_PropTypes != null);

                return m_PropTypes;
            }
        }

        /*
        public static SerializableProperty CreateSerializableProperty(Type propertyType, string sPropertyName, object oValue)
        {
            Type tSerializableProperty = typeof(SerializableProperty<>).MakeGenericType(propertyType);
            SerializableProperty sp = Activator.CreateInstance(tSerializableProperty, sPropertyName, oValue) as SerializableProperty;

            return sp;
        }

        public static SerializableProperty CreateSerializableProperty(Type propertyType, string sPropertyName)
        {
            Type tSerializableProperty = typeof(SerializableProperty<>).MakeGenericType(propertyType);
            SerializableProperty sp = Activator.CreateInstance(tSerializableProperty, sPropertyName) as SerializableProperty;

            return sp;
        }
        */

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            SerializableProperty sp = this.GetSerializableProperty(binder.Name);

            if (sp != null)
            {
                result = sp;

                return true;
            }

            result = null;

            return false;
        }

        public SerializableProperty GetSerializableProperty(string sPropertyName)
        {
            string sName = sPropertyName.ToLowerInvariant();

            if (m_di.ContainsKey(sName))
            {
                return m_di[sName];
            }

            if (this.PropTypes.ContainsKey(sName))
            {
                Type tProperty = this.PropTypes[sName];

                //object oDefaultValue = tProperty.IsValueType ? Activator.CreateInstance(tProperty) : null;

                return SerializableProperty.CreateSerializableProperty(tProperty, sPropertyName);
            }

            return null;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string sName = binder.Name.ToLowerInvariant();

            try
            {
                SerializableProperty sp = value as SerializableProperty;

                if (sp != null)
                {
                    m_di[sName] = sp;
                    return true;
                }

                if (this.PropTypes.ContainsKey(sName))
                {
                    Type tProperty = this.PropTypes[sName];

                    m_di[sName] = SerializableProperty.CreateSerializableProperty(tProperty, binder.Name, value);
                    return true;
                }
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "TrySetMember() ERROR");
            }

            return false;
        }

        public void ToXml(XmlTextWriter tw)
        {
            tw.WriteStartElement(this.XmlTagName);

            foreach (string sKey in m_di.Keys)
            {
                SerializableProperty sp = m_di[sKey];

                if (sp.PropertyValue != null)
                {
                    string sValue = SerializablePropertyToString(sp);
                    tw.WriteElementString(sKey, sValue);
                }
            }

            tw.WriteEndElement();
        }

        private string SerializablePropertyToString(SerializableProperty sp)
        {
            Debug.Assert(sp.PropertyValue != null);
            string sName = sp.PropertyName.ToLowerInvariant();

            try
            {
                ExcpHelper.ThrowIf<InvalidOperationException>(!this.PropTypes.ContainsKey(sName), "Cannot find property type.");
                Type tProperty = this.PropTypes[sName];

                if (tProperty == typeof(Int32) || tProperty == typeof(Int32?))
                {
                    return XmlConvert.ToString((Int32)sp.PropertyValue);
                }

                if (tProperty == typeof(Int64) || tProperty == typeof(Int64?))
                {
                    return XmlConvert.ToString((Int64)sp.PropertyValue);
                }

                if (tProperty == typeof(DateTime) || tProperty == typeof(DateTime?))
                {
                    return XmlConvert.ToString((DateTime)sp.PropertyValue);
                }

                if (tProperty == typeof(DateTimeSr))
                {
                    DateTimeSr dtsr = sp.PropertyValue as DateTimeSr;
                    Debug.Assert(dtsr != null);

                    return dtsr.ToXmlString();
                }

                if (tProperty == typeof(bool) || tProperty == typeof(bool?))
                {
                    return XmlConvert.ToString((bool)sp.PropertyValue);
                }

                if (tProperty == typeof(decimal) || tProperty == typeof(decimal?))
                {
                    return XmlConvert.ToString((decimal)sp.PropertyValue);
                }

                if (tProperty.IsEnum)
                {
                    return sp.PropertyValue.ToString();
                }

                return sp.PropertyValue.ToString();
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "SerializablePropertyToString({0}) ERROR.", sp);
                throw;
            }
        }

        private delegate object DelegateStringToObject(string sValue);
        private static readonly SyncDictionary<Type, DelegateStringToObject> diObjectFactory = new SyncDictionary<Type, DelegateStringToObject>()
        {
            {typeof(string), delegate(string sValue) { return sValue; }},
            {typeof(Int32), delegate(string sValue) { return XmlConvert.ToInt32(sValue); }},
            {typeof(Int32?), delegate(string sValue) { return XmlConvert.ToInt32(sValue); }},
            {typeof(Int64), delegate(string sValue) { return XmlConvert.ToInt64(sValue); }},
            {typeof(Int64?), delegate(string sValue) { return XmlConvert.ToInt64(sValue); }},
            {typeof(DateTime), delegate(string sValue) { return XmlConvert.ToDateTime(sValue); }},
            {typeof(DateTime?), delegate(string sValue) { return XmlConvert.ToDateTime(sValue); }},
            {typeof(DateTimeSr), delegate(string sValue)
            {
                DateTimeSr dtsr = new DateTimeSr();
                dtsr.FromXmlString(sValue);
                return dtsr;
            }},
            {typeof(bool), delegate(string sValue) { return XmlConvert.ToBoolean(sValue); }},
            {typeof(bool?), delegate(string sValue) { return XmlConvert.ToBoolean(sValue); }},
            {typeof(decimal), delegate(string sValue) { return XmlConvert.ToDecimal(sValue); }},
            {typeof(decimal?), delegate(string sValue) { return XmlConvert.ToDecimal(sValue); }},
        };

        private SerializableProperty StringToSerializableProperty(string sPropertyName, string sValue)
        {
            string sName = sPropertyName.ToLowerInvariant();

            try
            {
                Type tProperty = this.PropTypes.SafelyGetValue(sName);

                //ExcpHelper.ThrowIf<InvalidOperationException>(!this.PropTypes.ContainsKey(sName), "Cannot find property type for '{0}'", sName);
                //Type tProperty = this.PropTypes[sName];

                if (tProperty != null)
                {
                    DelegateStringToObject dso = diObjectFactory.SafelyGetValue(tProperty);


                    object oValue = null;

                    if (dso != null)
                    {
                        oValue = dso(sValue);
                    }
                    else if (tProperty.IsEnum)
                    {
                        oValue = Enum.Parse(tProperty, sValue, true);
                    }
                    else
                    {
                        throw new Exception(string.Format("Cannot get value for type '{0}' (PropertyName='{1}', value='{2}')", tProperty, sPropertyName, sValue));
                    }

                    return SerializableProperty.CreateSerializableProperty(tProperty, sPropertyName, oValue);
                }

                return null;
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "StringToSerializableProperty(sPropertyName='{0}', sValue='{1}') ERROR.", sPropertyName, sValue);
                throw;
            }
        }

        public void FromXml(XmlTextReader tr)
        {
            Debug.Assert(tr != null);

            string sCurrentTagName = string.Empty;

            while (tr.Read())
            {
                switch (tr.NodeType)
                {
                    case XmlNodeType.Element:

                        sCurrentTagName = tr.Name;

                        if (sCurrentTagName == this.XmlTagName)
                        {
                            break;
                        }

                        break;

                    case XmlNodeType.Text:

                        SerializableProperty sp = StringToSerializableProperty(sCurrentTagName, tr.Value);

                        if (sp != null)
                        {
                            m_di.Add(sCurrentTagName, sp);
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

        public override int GetHashCode()
        {
            int iHashCode = 0;

            SyncList<SerializableProperty> lProps = m_di.ToSyncList();

            lProps.Sort(delegate(SerializableProperty sp1, SerializableProperty sp2) { return sp1.PropertyName.CompareTo(sp2.PropertyName); });

            foreach (SerializableProperty sp in lProps)
            {
                if (sp.PropertyValue != null)
                {
                    iHashCode ^= sp.PropertyValue.GetHashCode();
                }
            }

            return iHashCode;
        }

        public override bool Equals(object obj)
        {
            SerializableObject so = obj as SerializableObject;

            return so != null ? this.GetHashCode() == so.GetHashCode() : false;
        }

        public string XmlTagName { get { return m_type.Name; } }

        public bool IsToRemove()
        {
            SerializableProperty<bool> sp = m_di.SafelyGetValue(COMMAND_REMOVE) as SerializableProperty<bool>;

            return sp != null && sp.Value;
        }

        public override string ToString()
        {
            List<string> lPropNameValues = new List<string>();
            SyncList<SerializableProperty> lProps = m_di.ToSyncList();

            foreach (SerializableProperty sp in lProps)
            {
                lPropNameValues.Add(string.Format("{0}='{1}'", sp.PropertyName, sp.PropertyValue));
            }

            return string.Format("{0} {{{1}}}", m_type.Name, string.Join(", ", lPropNameValues.ToArray()));
        }

        /*
        public object MethodTag { get; set; }

        public T GetObject<T>() where T : ILineObject<T>
        {
            Type t = typeof (T);
            ExcpHelper.ThrowIf<InvalidOperationException>(t != m_type, "GetObject<{0}> ERROR. Current type is {1}", t.Name, m_type.Name);

            LineSr.DelegateGetLineObject<T> dglo = this.MethodTag as LineSr.DelegateGetLineObject<T>;
            ExcpHelper.ThrowIf<ArgumentException>(t != m_type, "GetObject<{0}> ERROR. MethodTag is not set correctly.", t.Name);

            try
            {
                return dglo(this);
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "GetObject<{0}> ERROR for {1}", this);
            }

            return default(T);
        }
        */
    }
}
