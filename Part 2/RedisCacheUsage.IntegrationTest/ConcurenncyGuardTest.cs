using Microsoft.AspNetCore.Mvc.Testing;

namespace RedisCacheUsage.IntegrationTest
{
    [TestClass]
    public class ConcurenncyGuardTest
    {
        private readonly HttpClient _client;
        private const string DefaultEventName = "Test";

        public ConcurenncyGuardTest()
        {
            var webAppFactory = new WebApplicationFactory<Program>();
            _client = webAppFactory.CreateClient();
        }
        [TestMethod]
        public async Task Scenario_1_AddEventWhenNotExistsShouldPass()
        {
            var response = await AddEvent(DefaultEventName);
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task Scenario_2_AddEventWithSameNameBeforeFirstOneIsDoneShoudFail()
        {
            var response = await AddEvent(DefaultEventName);
            Assert.IsTrue(response.IsSuccessStatusCode);

            response = await AddEvent(DefaultEventName);
            Assert.IsFalse(response.IsSuccessStatusCode);

            response = await RemoveEvent(DefaultEventName);
            Assert.IsTrue(response.IsSuccessStatusCode);

            response = await AddEvent(DefaultEventName);
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task Scenario_3_ConcurrentAddEventWithSameNameShouldFailExceptOne()
        {
            var responseList = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < 10; i++)
            {
                var response = AddEvent(DefaultEventName);
                responseList.Add(response);
            }

            var successCount = 0;
            foreach (var response in responseList)
            {
                var message = await response;
                successCount = message.IsSuccessStatusCode ? successCount + 1 : successCount;
            }
            Assert.AreEqual(1, successCount);
        }

        private Task<HttpResponseMessage> AddEvent(string eventName)
        {
            return _client.PostAsync($"/ConcurrencyGuard?eventName={eventName}", null);
        }

        private Task<HttpResponseMessage> RemoveEvent(string eventName)
        {
            return _client.DeleteAsync($"/ConcurrencyGuard?eventName={eventName}");
        }

        [TestCleanup()]
        public async Task TestCleanup()
        {
            var webAppFactory = new WebApplicationFactory<Program>();
            var client = webAppFactory.CreateClient();
            var response = await client.DeleteAsync($"/ConcurrencyGuard?eventName={DefaultEventName}");
        }
    }
}