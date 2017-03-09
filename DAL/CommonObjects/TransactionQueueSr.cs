using System;
using System.Collections.Generic;
using System.Data;

namespace SportRadar.DAL.CommonObjects
{
    public class TransactionQueueSr : ObjectBase
    {
        public override void FillFromDataRow(DataRow dr)
        {
            this.TransactionQueueID = DbConvert.ToInt64(dr, "TransactionQueueID");
            this.Type = DbConvert.ToInt16(dr, "Type");
            this.Description = DbConvert.ToString(dr, "Description");
            this.TransactionId = DbConvert.ToString(dr, "TransactionId");
            this.UidState = DbConvert.ToString(dr, "UidState");
            this.Object1 = DbConvert.ToString(dr, "Object1");
            this.Object2 = DbConvert.ToString(dr, "Object2");
            this.Object3 = DbConvert.ToString(dr, "Object3");
            this.Tag1 = DbConvert.ToString(dr, "Tag1");
            this.Tag2 = DbConvert.ToString(dr, "Tag2");
            this.Created = DbConvert.ToDateTime(dr, "Created");
        }

        public static TransactionQueueSr CreateFromDataRow(DataRow dr)
        {
            TransactionQueueSr tqs = new TransactionQueueSr();

            tqs.FillFromDataRow(dr);

            return tqs;
        }
        private static readonly TableSpecification TableSpec = new TableSpecification("transactionqueue", true, "TransactionQueueID");

        public override TableSpecification Table { get { return TableSpec; } }


        public long TransactionQueueID { get; set; }
        public override long ORMID { get { return this.TransactionQueueID; } }
        public short Type { get; set; }
        public string Description { get; set; }
        public string TransactionId { get; set; }
        public string UidState { get; set; }
        public string Object1 { get; set; }
        public string Object2 { get; set; }
        public string Object3 { get; set; }

        public string Tag1 { get; set; }
        public string Tag2 { get; set; }
        public DateTime Created { get; set; }

        public static List<TransactionQueueSr> GetTransactionQueueList()
        {
            var lResult = new List<TransactionQueueSr>();

            using (DataTable dt = DataCopy.GetDataTableWithSqlParams("SELECT * FROM " + TableSpec.TableName, new List<IDbDataParameter>()))
            {
                if (dt != null)
                    foreach (DataRow dr in dt.Rows)
                    {
                        TransactionQueueSr tqs = TransactionQueueSr.CreateFromDataRow(dr);

                        lResult.Add(tqs);
                    }
            }

            return lResult;
        }

        public static int GetCountTransactionQueue()
        {
            return GetTransactionQueueList().Count;
        }

        public static List<TransactionQueueSr> GetByQuery(string s, int ticket, int depositByCreditNote, int deposit)
        {
            return new List<TransactionQueueSr>();
        }
        public override void Delete(IDbConnection conn, IDbTransaction transaction)
        {
            DataCopy.ExecuteScalar(conn, transaction, "DELETE FROM {0} WHERE {1} = {2}", this.Table.TableName, this.Table.IdentityNames[0], this.ORMID);
        }
    }
}
