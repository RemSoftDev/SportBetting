namespace SportRadar.DAL.CommonObjects
{
    public sealed class UpdateStatistic
    {
        public int InsertCount = 0;
        public int UpdateCount = 0;
        //public int SkipCount  = 0;
        public int DeleteCount = 0;

        public void Append(UpdateStatistic us)
        {
            this.InsertCount += us.InsertCount;
            this.UpdateCount += us.UpdateCount;
            //this.SkipCount += us.SkipCount;
            this.DeleteCount += us.DeleteCount;
        }

        public int Count
        {
            get { return this.InsertCount + this.UpdateCount + this.DeleteCount; }
        }
    }
}