using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Windows;

namespace SportRadar.Common
{
    public enum eLineType
    {
        Error = -1,
        BtrPre = 0,
        BtrLive = 1,
        BtrVfl = 2,
        BtrVhc = 3,
    }

    public enum eVersionedObjectType
    {
        Error = 0,
        TaggedStringLn = 10,
        GroupLn = 20,
        CompetitorLn = 30,
        MatchLn = 40,
        LiveMatchInfoLn = 42,
        MatchToGroupLn = 45,
        MatchToCompetitor = 48,
        BetDomainTypeLn = 52,
        BetDomainLn = 55,
        OddLn = 60,
    }

    public class SportRadarId
    {
        //public const long MAX_DATA_SOURCE_ID = 17179869183L;
        public const long   MAX_OBJECT_ID   = 4294967291L;
        public const UInt16 MAX_EXTENDED_ID = 32767;
        public const UInt16 MAX_CLIENT_ID   = 7;

        private const long MASK_OBJECT_ID   = 0x7FFFFFFF80000000;
        private const long MASK_EXTENDED_ID = 0x000000007FFF0000;
        private const long MASK_LINE_TYPE   = 0x000000000000FC00;
        private const long MASK_CLIENT_ID   = 0x0000000000000380;
        private const long MASK_OBJECT_TYPE = 0x000000000000007F;

        public long ObjectId { get; private set; }                    // 32 Bits -> 8589934592 combinations
        public UInt16 ExtendedId { get; private set; }                // 15 Bits -> 32768 combinations
        public eLineType LineType { get; private set; }               //  6 Bits -> Max 64 Types
        public UInt16 ClientId { get; private set; }                  //  3 Bits -> Max 8 Clients (In single envinroment)
        public eVersionedObjectType ObjectType { get; private set; }  //  7 Bits -> 128 ObjectTypes

        private SportRadarId()
        {
        }

        public static SportRadarId Create(eVersionedObjectType vot, long lObjectId, UInt16 uiExtendedId, eLineType lt, UInt16 uiClientId)
        {
            try
            {
                ExcpHelper.ThrowIf<ArgumentException>(Math.Abs(lObjectId) > MAX_OBJECT_ID, "SportRadarId.Create(ObjectType={0}, ObjectId={1}, ExtendedId={2}, LineType={3}, Client={4}, ) ERROR. ObjectId is too big. MaxValue={5}",
                    vot, lObjectId, uiExtendedId, lt, uiClientId, MAX_OBJECT_ID);
                ExcpHelper.ThrowIf<ArgumentException>(uiExtendedId > MAX_EXTENDED_ID, "SportRadarId.Create(ObjectType={0}, ObjectId={1}, ExtendedId={2}, LineType={3}, Client={4}, ) ERROR. ExtendedId is too big. MaxValue={5}",
                    vot, lObjectId, uiExtendedId, lt, uiClientId, MAX_EXTENDED_ID);
                ExcpHelper.ThrowIf<ArgumentException>(uiClientId > MAX_CLIENT_ID, "SportRadarId.Create(ObjectType={0}, ObjectId={1}, ExtendedId={2}, LineType={3}, Client={4}, ) ERROR. ClientId is too big. MaxValue={5}",
                    vot, lObjectId, uiExtendedId, lt, uiClientId, MAX_CLIENT_ID);

                SportRadarId srid = new SportRadarId();

                srid.ObjectId = lObjectId;
                srid.ExtendedId = uiExtendedId;
                srid.LineType = lt;
                srid.ClientId = uiClientId;
                srid.ObjectType = vot;

                return srid;
            }
            catch (Exception excp)
            {           
                throw;
            }

            return null;
        }

        public long ToLongId()
        {
            long lHigh = Math.Abs(this.ObjectId) << 31;
            long lExtended = this.ExtendedId << 16;
            long lLineType = ((int)this.LineType) << 10;
            long lClientId = this.ClientId << 7;
            long lObjectType = (long)this.ObjectType;

            long lValue = lHigh | lExtended | lLineType | lClientId | lObjectType;

            return this.ObjectId < 0 ? lValue * -1 : lValue;
        }

        public static SportRadarId FromLongId(long lLongId)
        {
            try
            {
                SportRadarId srid = new SportRadarId();

                long lId = Math.Abs(lLongId);

                srid.ObjectId = (lId & MASK_OBJECT_ID) >> 31;
                srid.ExtendedId = (UInt16)((lId & MASK_EXTENDED_ID) >> 16);
                srid.LineType = (eLineType)((lId & MASK_LINE_TYPE) >> 10);
                srid.ClientId = (UInt16)((lId & MASK_CLIENT_ID) >> 7);
                srid.ObjectType = (eVersionedObjectType)(lId & MASK_OBJECT_TYPE);

                if (lLongId < 0)
                {
                    srid.ObjectId *= -1;
                }

                return srid;
            }
            catch (Exception excp)
            {
                throw;
            }

            return null;
        }

        public override string ToString()
        {
            return string.Format("SportRadarId {{ObjectType={0}, ObjectId={1}, ExtendedId={2}, LineType={3}, ClientId={4}}}",
                this.ObjectType, this.ObjectId, this.ExtendedId, this.LineType, this.ClientId);
        }
    }
}
