using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportRadar.DAL.CommonObjects;

namespace MVVMTest.Services
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var xmlPreDuplicate = new List<string>() {"<sdgsdg>", "<segsgsdgsdg>"};
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(xmlPreDuplicate);
            Console.WriteLine(json);
            var deserializedObj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json);
            for (int i = 0; i < xmlPreDuplicate.Count; i++)
            {
                Assert.AreEqual(xmlPreDuplicate[i],deserializedObj[i]);
            }
        }  
        
    }
}
