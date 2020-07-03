using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pacco.Services.Availability.Api;
using Pacco.Services.Availability.Application.DTO;
using Pacco.Services.Availability.Infrastructure.Mongo.Documents;
using Pacco.Services.Availability.Tests.Shared.Factories;
using Pacco.Services.Availability.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace Pacco.Services.Availability.Tests.EndToEnd.Api
{
    public class GetResourceApiTests : IClassFixture<PaccoApplicationFactory<Program>>, IDisposable
    {
        Task<HttpResponseMessage> Act()
            => _httpClient.GetAsync($"resources/{_resourceId}");

        [Fact]
        public async Task given_id_not_present_within_mongo_collection_get_resource_endpoint_should_return_http_status_code_bad_request()
        {
            var response = await Act();
            
            response.ShouldNotBeNull();
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task given_valid_id_get_resource_endpoint_should_return_dto_with_correct_data()
        {
            var document = CreateResourceDocument();
            await _mongoDbFixture.InsertAsync(document);

            var response = await Act();
            
            response.ShouldNotBeNull();
            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<ResourceDto>(json);
            
            dto.Id.ShouldBe(_resourceId);
        }

        #region ARRANGE

        private readonly HttpClient _httpClient;
        private readonly MongoDbFixture<ResourceDocument, Guid> _mongoDbFixture;
        private readonly Guid _resourceId;

        public GetResourceApiTests(PaccoApplicationFactory<Program> factory)
        {
            _mongoDbFixture = new MongoDbFixture<ResourceDocument, Guid>("resources");
            factory.Server.AllowSynchronousIO = true;
            _httpClient = factory.CreateClient();
            _resourceId = Guid.NewGuid();
        }        
        
        private ResourceDocument CreateResourceDocument()
            => new ResourceDocument
            {
                Id = _resourceId,
                Tags = new [] {"tag"},
                Reservations = new []
                {
                    new ReservationDocument
                    {
                        TimeStamp = DateTime.UtcNow.AsDaysSinceEpoch(),
                        Priority = 0
                    } 
                }
            };

        public void Dispose()
        {
            _httpClient?.Dispose();
            _mongoDbFixture?.Dispose();
        }
        
        #endregion
    }
}