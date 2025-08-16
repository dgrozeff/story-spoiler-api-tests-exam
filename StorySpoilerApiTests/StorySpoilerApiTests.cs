using NUnit.Framework;
using RestSharp;
using System.Net;
using System.Text.Json;
using StorySpoilerApiTests.Models;

namespace StorySpoilerApiTests
{
    [TestFixture]
    [FixtureLifeCycle(LifeCycle.SingleInstance)]
    [Parallelizable(ParallelScope.None)]
    public class StorySpoilerApiTests
    {
        private const string ApiBaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net/api";
        private RestClient _client = null!;
        private static string? _storyId;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            // Create client
            var options = new RestClientOptions(ApiBaseUrl);
            _client = new RestClient(options);

            // Authenticate
            var login = new LoginDTO
            {
                UserName = "testExamUser123",
                Password = "P@ssw0rd123"
            };

            var loginReq = new RestRequest("/User/Authentication", Method.Post)
                .AddJsonBody(login);

            var loginResp = await _client.ExecuteAsync(loginReq);
            Assert.That(loginResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var auth = JsonSerializer.Deserialize<AuthResponseDTO>(loginResp.Content!);
            Assert.That(auth?.AccessToken, Is.Not.Null.And.Not.Empty);

            // Attach token for all requests
            _client.AddDefaultHeader("Authorization", $"Bearer {auth!.AccessToken}");
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _client?.Dispose();
        }

        // 1. Create story
        [Test, Order(1)]
        public async Task CreateStorySpoiler()
        {
            var story = new StoryDTO
            {
                Title = "Exam Story Title",
                Description = "This is the description for the exam story",
                Url = ""
            };

            var req = new RestRequest("/Story/Create", Method.Post).AddJsonBody(story);
            var resp = await _client.ExecuteAsync(req);

            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var data = JsonSerializer.Deserialize<ApiResponseDTO>(resp.Content!);
            Assert.That(data?.StoryId, Is.Not.Null.And.Not.Empty);
            Assert.That(data?.Msg, Does.Contain("Successfully created"));

            _storyId = data!.StoryId;
        }

        // 2. Edit story
        [Test, Order(2)]
        public async Task EditStorySpoiler()
        {
            var story = new StoryDTO
            {
                Title = "Edited Story Title",
                Description = "Edited description",
                Url = ""
            };

            var req = new RestRequest($"/Story/Edit/{_storyId}", Method.Put).AddJsonBody(story);
            var resp = await _client.ExecuteAsync(req);

            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var data = JsonSerializer.Deserialize<ApiResponseDTO>(resp.Content!);
            Assert.That(data?.Msg, Does.Contain("Successfully edited"));
        }

        // 3. Get all stories
        [Test, Order(3)]
        public async Task GetAllStories()
        {
            var req = new RestRequest("/Story/All", Method.Get);
            var resp = await _client.ExecuteAsync(req);

            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var stories = JsonSerializer.Deserialize<List<StoryDTO>>(resp.Content!);

            Assert.That(stories, Is.Not.Null);
            Assert.That(stories!.Count, Is.GreaterThan(0), "Expected at least one story in the list");
        }


        // 4. Delete story
        [Test, Order(4)]
        public async Task DeleteStory()
        {
            var req = new RestRequest($"/Story/Delete/{_storyId}", Method.Delete);
            var resp = await _client.ExecuteAsync(req);

            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var data = JsonSerializer.Deserialize<ApiResponseDTO>(resp.Content!);
            Assert.That(data?.Msg, Does.Contain("Deleted successfully"));
        }

        // 5. Create without required fields
        [Test, Order(5)]
        public async Task CreateStoryWithoutRequiredFields()
        {
            var badStory = new StoryDTO
            {
                Title = "",
                Description = ""
            };

            var req = new RestRequest("/Story/Create", Method.Post).AddJsonBody(badStory);
            var resp = await _client.ExecuteAsync(req);

            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        // 6. Edit non-existing story
        [Test, Order(6)]
        public async Task EditNonExistingStory()
        {
            var story = new StoryDTO
            {
                Title = "Ghost",
                Description = "Does not exist",
                Url = ""
            };

            var req = new RestRequest($"/Story/Edit/00000000-0000-0000-0000-000000000000", Method.Put)
                .AddJsonBody(story);
            var resp = await _client.ExecuteAsync(req);

            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var data = JsonSerializer.Deserialize<ApiResponseDTO>(resp.Content!);
            Assert.That(data?.Msg, Does.Contain("No spoilers"));
        }

        // 7. Delete non-existing story
        [Test, Order(7)]
        public async Task DeleteNonExistingStory()
        {
            var req = new RestRequest($"/Story/Delete/00000000-0000-0000-0000-000000000000", Method.Delete);
            var resp = await _client.ExecuteAsync(req);

            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var data = JsonSerializer.Deserialize<ApiResponseDTO>(resp.Content!);
            Assert.That(data?.Msg, Does.Contain("Unable to delete"));
        }
    }
}
