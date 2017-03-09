using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using SportRadar.Common.Logs;

namespace SportBetting.WPF.Prism.Shared
{
    public enum eTransactionQueueType
    {
        None = 0,
        Ticket = 1,
        Deposit = 2,
        DepositByTicket = 4,
        DepositByCreditNote = 5,
        Cash = 6,
        PayTicket = 7,
        PayCreditNote = 8,
    }

    public static class SerializeHelper
    {
        //private static ILog m_logger = LogManager.GetLogger(typeof(SerializeHelper));
        private static ILog Log = LogFactory.CreateLog(typeof(SerializeHelper));

        #region DataContract

        public static MemoryStream DataContractObjectToStream<T>(object objToSerialize)
        {
            Type tObject = typeof(T);

            Debug.Assert(objToSerialize != null && objToSerialize.GetType() == tObject);

            try
            {
                MemoryStream ms = new MemoryStream();
                DataContractSerializer dcs = new DataContractSerializer(tObject);

                dcs.WriteObject(ms, objToSerialize);

                ms.Position = 0;

                return ms;
            }
            catch (Exception excp)
            {
                Log.ErrorFormat("DataContractObjectToStream<{0}> ({1}) ERROR:{2}\r\n{3}", excp,tObject, objToSerialize != null ? objToSerialize.ToString() : "NULL", excp.Message, excp.StackTrace);
            }

            return null;
        }

        public static byte[] DataContractObjectToByteArray<T>(object objToSerialize)
        {
            Type tObject = typeof(T);

            try
            {
                using (MemoryStream ms = SerializeHelper.DataContractObjectToStream<T>(objToSerialize))
                {
                    return ms.ToArray();
                }
            }
            catch (Exception excp)
            {
                Log.ErrorFormat("DataContractObjectToByteArray<{0}> ({1}) ERROR:{2}\r\n{3}", excp, tObject, objToSerialize != null ? objToSerialize.ToString() : "NULL", excp.Message, excp.StackTrace);
            }

            return null;
        }

        public static string DataContractObjectToString<T>(object objToSerialize)
        {
            Type tObject = typeof(T);

            try
            {
                using (MemoryStream ms = SerializeHelper.DataContractObjectToStream<T>(objToSerialize))
                {
                    using (StreamReader sr = new StreamReader(ms))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception excp)
            {
                Log.ErrorFormat("DataContractObjectToString<{0}> ({1}) ERROR:{2}\r\n{3}", excp, tObject, objToSerialize != null ? objToSerialize.ToString() : "NULL", excp.Message, excp.StackTrace);
            }

            return null;
        }

        public static T StreamToDataContractObject<T>(Stream sm)
        {
            Debug.Assert(sm != null);
            Type tObject = typeof(T);

            try
            {
                XmlDictionaryReader xdr = XmlDictionaryReader.CreateTextReader(sm, new XmlDictionaryReaderQuotas());
                DataContractSerializer dcs = new DataContractSerializer(tObject);

                return (T)dcs.ReadObject(xdr, true);
            }
            catch (Exception excp)
            {
                Log.ErrorFormat("StreamToDataContractObject<{0}> () ERROR:{1}\r\n{2}", excp, tObject, excp.Message, excp.StackTrace);
            }

            return default(T);
        }

        public static T BytesToDataContractObject<T>(byte[] arrBytes)
        {
            Debug.Assert(arrBytes != null);
            Type tObject = typeof(T);

            try
            {
                using (MemoryStream ms = new MemoryStream(arrBytes))
                {
                    return SerializeHelper.StreamToDataContractObject<T>(ms);
                }
            }
            catch (Exception excp)
            {
                Log.ErrorFormat("BytesToDataContractObject<{0}> () ERROR:{1}\r\n{2}", excp, tObject, excp.Message, excp.StackTrace);
            }

            return default(T);
        }

        public static T StringToDataContractObject<T>(string sSerializedObject)
        {
            Debug.Assert(!string.IsNullOrEmpty(sSerializedObject));
            Type tObject = typeof(T);

            try
            {
                byte[] arrBytes = Encoding.ASCII.GetBytes(sSerializedObject);
                return SerializeHelper.BytesToDataContractObject<T>(arrBytes);
            }
            catch (Exception excp)
            {
                Log.ErrorFormat("StringToDataContractObject<{0}> (string {1} characters) ERROR:{2}\r\n{3}", excp, tObject, !string.IsNullOrEmpty(sSerializedObject) ? sSerializedObject.Length : -1, excp.Message, excp.StackTrace);
            }

            return default(T);
        }

        #endregion // DataContract

        #region Serializable

        public static MemoryStream SerializableObjectToStream<T>(object objToSerialize)
        {
            Type tObject = typeof(T);

            Debug.Assert(objToSerialize != null && objToSerialize.GetType() == tObject);

            try
            {
                MemoryStream ms = new MemoryStream();
                XmlSerializer xs = new XmlSerializer(tObject);

                xs.Serialize(ms, objToSerialize);
                ms.Position = 0;

                return ms;
            }
            catch (Exception excp)
            {
                Log.ErrorFormat(" SerializableObjectToStream<{0}> ({1}) ERROR:{2}\r\n{3}", excp, tObject, objToSerialize != null ? objToSerialize.ToString() : "NULL", excp.Message, excp.StackTrace);
            }

            return null;
        }

        public static byte[] SerializableObjectToByteArray<T>(object objToSerialize)
        {
            Type tObject = typeof(T);

            try
            {
                using (MemoryStream ms = SerializeHelper.SerializableObjectToStream<T>(objToSerialize))
                {
                    return ms.ToArray();
                }
            }
            catch (Exception excp)
            {
                Log.ErrorFormat("SerializableObjectToByteArray<{0}> ({1}) ERROR:{2}\r\n{3}", excp, tObject, objToSerialize != null ? objToSerialize.ToString() : "NULL", excp.Message, excp.StackTrace);
            }

            return null;
        }



        public static string SerializableObjectToString<T>(object objToSerialize)
        {
            Type tObject = typeof(T);

            try
            {
                using (MemoryStream ms = SerializeHelper.SerializableObjectToStream<T>(objToSerialize))
                {
                    using (StreamReader sr = new StreamReader(ms))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception excp)
            {
                Log.ErrorFormat("SerializableObjectToString<{0}> ({1}) ERROR:{2}\r\n{3}", excp, tObject, objToSerialize != null ? objToSerialize.ToString() : "NULL", excp.Message, excp.StackTrace);
            }

            return null;
        }

        public static T StreamToSerializableObject<T>(Stream sm)
        {
            Debug.Assert(sm != null);
            Type tObject = typeof(T);

            try
            {
                XmlSerializer xs = new XmlSerializer(tObject);

                return (T)xs.Deserialize(sm); ;
            }
            catch (Exception excp)
            {
                Log.ErrorFormat("StreamToSerializableObject<{0}> () ERROR:{1}\r\n{2}", excp, tObject, excp.Message, excp.StackTrace);
            }

            return default(T);
        }

        public static T BytesToSerializableObject<T>(byte[] arrBytes)
        {
            Debug.Assert(arrBytes != null);
            Type tObject = typeof(T);

            try
            {
                using (MemoryStream ms = new MemoryStream(arrBytes))
                {
                    return SerializeHelper.StreamToSerializableObject<T>(ms);
                }
            }
            catch (Exception excp)
            {
                Log.ErrorFormat("BytesToSerializableObject<{0}> () ERROR:{1}\r\n{2}", excp, tObject, excp.Message, excp.StackTrace);
            }

            return default(T);
        }

        public static T StringToSerializableObject<T>(string sSerializedObject)
        {
            Debug.Assert(!string.IsNullOrEmpty(sSerializedObject));
            Type tObject = typeof(T);

            try
            {
                byte[] arrBytes = Encoding.ASCII.GetBytes(sSerializedObject);
                return SerializeHelper.BytesToSerializableObject<T>(arrBytes);
            }
            catch (Exception excp)
            {
                Log.ErrorFormat("StringToSerializableObject<{0}> (string {1} characters) ERROR:{2}\r\n{3}", excp, tObject, !string.IsNullOrEmpty(sSerializedObject) ? sSerializedObject.Length : -1, excp.Message, excp.StackTrace);
            }

            return default(T);
        }

        #endregion // Serializable
    }

    [Serializable()]
    public class TicketData
    {
        [DataMember(IsRequired = true)]
        public string StationNumber { get; set; }

        [DataMember(IsRequired = true)]
        public bool IsOffLineTicket { get; set; }

        [DataMember(IsRequired = false)]
        public string PinCode { get; set; }
    }
}
