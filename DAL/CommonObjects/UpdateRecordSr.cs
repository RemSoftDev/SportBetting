using System.Runtime.Serialization;
using SportRadar.Common.Windows;

namespace SportRadar.DAL.CommonObjects
{
    public enum eDataSyncCacheType
    {
        None = 0,
        String = 1,
        Match = 2,
        Statistic = 3,
        ActiveTournaments = 4,
        Resources = 5,
        Metainfo = 7,
        Liability = 8
    }

    [DataContract]
    public class UpdateRecordSr
    {
        public const long INVALID_UPDATE_FIELD_ID = -1;

        public UpdateRecordSr(long lDataSyncCacheId, string sFileName, eDataSyncCacheType dsct, string sData, string sDescription)
        {
            this.DataSyncCacheId = lDataSyncCacheId;
            this.FileName = sFileName;
            this.DataSyncCacheType = dsct;
            this.Data = sData;
            this.Description = sDescription;
        }

        public long DataSyncCacheId { get; set; }
        public string FileName { get; set; }
        public eDataSyncCacheType DataSyncCacheType { get; set; }
        public string Data { get; set; }
        public string Description { get; set; }

        public string GetXmlData()
        {
            string sXmlFile = TextUtil.DecompressBase64String(this.Data);
            return sXmlFile.Substring(sXmlFile.IndexOf("<"));
        }

        public override int GetHashCode()
        {
            return unchecked((int)this.DataSyncCacheId);
        }

        public override string ToString()
        {
            return string.Format("UpdateRecord {{DataSyncCacheId = {0}, FileName = '{1}', Type = {2}, Description = '{3}'}}", this.DataSyncCacheId, this.FileName, this.DataSyncCacheType, this.Description);
        }
    }
}