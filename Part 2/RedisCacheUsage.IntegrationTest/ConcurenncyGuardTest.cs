using Microsoft.AspNetCore.Mvc.Testing;

namespace RedisCacheUsage.IntegrationTest
{
    [TestClass]
    public class ConcurenncyGuardTest
    {
        private readonly HttpClient _client;

        public ConcurenncyGuardTest()
        {
            var webAppFactory = new WebApplicationFactory<Program>();
            _client = webAppFactory.CreateClient();
        }
        [TestMethod]
        public async Task Scenario_1_CheckSingleCase()
        {
            var response = await AddEvent("Test");
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task Scenario_2_CheckMultipleCase()
        {
            var response = await AddEvent("Test");
            Assert.IsTrue(response.IsSuccessStatusCode);
            response = await AddEvent("Test");
            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task Scenario_3_CheckConcurrentCreateCases()
        {
            var responseList=new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < 10; i++)
            {
                var response = AddEvent("Test");
                responseList.Add(response);
            }

            var successCount = 0;
            foreach (var response in responseList)
            {
                var message=await response;
                successCount= message.IsSuccessStatusCode ? successCount + 1 : successCount;
            }
            Assert.AreEqual(1, successCount);
        }

        private Task<HttpResponseMessage> AddEvent(string eventName)
        {
            return _client.PostAsync("/ConcurrencyGuard?eventName=Test", null);
        }

        [TestCleanup()]
        public async Task TestCleanup()
        {
            var webAppFactory = new WebApplicationFactory<Program>();
            var client = webAppFactory.CreateClient();
            var response = await client.DeleteAsync("/ConcurrencyGuard?eventName=Test");
        }
    }
}