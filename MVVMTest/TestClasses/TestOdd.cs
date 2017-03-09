using System;
using System.Data;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;

namespace MVVMTest
{
    public class TestOdd : ObjectBase, ILineObjectWithId<TestOdd>, IRemovableLineObject<TestOdd>, IOddLn
    {

        public TestOdd()
            : base(true)
        {
        }


        public IBetDomainLn BetDomain { get; set; }
        public ObservableProperty<decimal> Value { get; set; }

        public IOddVw OddView { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsSelected { get; private set; }
        public long OutcomeId { get; set; }
        public ObservableProperty<long> OddId { get; set; }

        public ObservableProperty<int> Sort { get; set; }
        public ObservableProperty<string> NameTag { get; set; }
        public ObservableProperty<bool> Active { get; set; }
        public ObservableProperty<string> ExtendedState { get; set; }
        public ObservableProperty<bool> IsManuallyDisabled { get; set; }
        public long BetDomainId { get; set; }

        public void SetSelected(bool bIsSelected)
        {
            throw new NotImplementedException();
        }

        public string GetDisplayName(string language)
        {
            throw new NotImplementedException();
        }

        public ObservableProperty<string> OddTag { get; set; }

        public ObservableProperty<bool> IsLiveBet { get; set; }
        public override void FillFromDataRow(DataRow dr)
        {
            throw new NotImplementedException();
        }

        public void MergeFrom(IOddLn objSource)
        {
            throw new NotImplementedException();
        }

        public void MergeFrom(TestOdd objSource)
        {
            throw new NotImplementedException();
        }

        public bool IsNew { get; private set; }


        public long Id { get; private set; }
        public long RemoveId { get; private set; }

        public static IOddLn CreateOdd(int id, decimal value, bool isvisible, TestBetDomain betdomain = null)
        {
            var odd = new TestOdd();
            odd.Id = id;
            odd.Sort = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "ChangedCount");
            odd.Sort.Value = id;
            odd.Value = new ObservableProperty<decimal>(new BetDomainLn(), new ObservablePropertyList(), "ChangedCount");
            odd.Value.Value = value;
            odd.IsManuallyDisabled = new ObservableProperty<bool>(new BetDomainLn(), new ObservablePropertyList(), "ChangedCount");
            odd.IsManuallyDisabled.Value = !isvisible;
            odd.Active = new ObservableProperty<bool>(new BetDomainLn(), new ObservablePropertyList(), "ChangedCount");
            odd.Active.Value = isvisible;
            if(betdomain == null)
                odd.BetDomain = TestBetDomain.CreateBetDomain(id+1, true, 3, true);
            else
            {
                odd.BetDomain = betdomain;
            }


            return odd;
        }
    }
}