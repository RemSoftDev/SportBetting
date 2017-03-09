using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportBetting.WPF.Prism.Shared.Models.Repositories;

namespace MVVMTest.Services
{
    [TestClass][Ignore]
    public class ActiveMQTest
    {
        [TestMethod]
        public void ConnectTest()
        {

            ActiveMQClient client = new ActiveMQClient();
           // client.Connect();

        }
    }
}
