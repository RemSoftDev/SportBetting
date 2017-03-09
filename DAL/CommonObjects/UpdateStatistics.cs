using System.Collections.Generic;

namespace SportRadar.DAL.CommonObjects
{
    public class UpdateStatistics : Dictionary<string, UpdateStatistic>
    {
        public bool IsInsrtedOrUpdatedOrDeleted
        {
            get
            {
                foreach (UpdateStatistic us in this.Values)
                {
                    if (us.InsertCount > 0 || us.UpdateCount > 0 || us.DeleteCount > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public UpdateStatistic EnsureStatistic(string sTableName)
        {
            if (!this.ContainsKey(sTableName))
            {
                this.Add(sTableName, new UpdateStatistic());
            }

            return this[sTableName];
        }

        public void Append(UpdateStatistics uss)
        {
            foreach (string sTableName in uss.Keys)
            {
                this.Append(sTableName, uss[sTableName]);
            }
        }

        public void Append(string sTableName, UpdateStatistic us)
        {
            if (!this.ContainsKey(sTableName))
            {
                this.Add(sTableName, new UpdateStatistic());
            }

            this[sTableName].Append(us);
        }

        public override string ToString()
        {
            const int TABLE_WIDTH = 24;
            const int COUNT_WIDTH = 12;

            string sResult = @"
Table Name                  Inserted     Updated     Deleted
************************************************************
";

            if (this.Count == 0)
            {
                sResult += "No result found.";
            }

            foreach (string sTableName in this.Keys)
            {
                UpdateStatistic us = this[sTableName];

                sResult += sTableName.PadRight(TABLE_WIDTH, ' ') +
                           us.InsertCount.ToString("G").PadLeft(COUNT_WIDTH, ' ') +
                           us.UpdateCount.ToString("G").PadLeft(COUNT_WIDTH, ' ') +
                           us.DeleteCount.ToString("G").PadLeft(COUNT_WIDTH, ' ') + "\r\n";
            }

            return sResult;
        }
    }
}