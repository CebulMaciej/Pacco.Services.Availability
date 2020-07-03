using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Pacco.Services.Availability.Api;
using Pacco.Services.Availability.Application.Commands;
using Pacco.Services.Availability.Application.Events;
using Pacco.Services.Availability.Infrastructure.Mongo.Documents;
using Pacco.Services.Availability.Tests.Shared.Factories;
using Pacco.Services.Availability.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace Pacco.Services.Availability.Tests.Integration.RabbitMq
{
    public class AddResourceRabbitMqTests : IClassFixture<PaccoApplicationFactory<Program>>, IDisposable
    {
        Task Act(AddResource command)
            => _rabbitMqFixture.PublishAsync(command, Namespace);

        [Fact]
        public async Task given_valid_data_add_resource_command_should_persist_document_into_mongodb()
        {
            var command = new AddResource(Guid.NewGuid(), new []{"tag"});

            var tcs = _rabbitMqFixture.SubscribeAndGet<ResourceAdded, ResourceDocument>(
                Namespace, _mongoDbFixture.GetAsync, command.ResourceId);

            await Act(command);

            var document = await tcs.Task;
            
            document.ShouldNotBeNull();
            document.Id.ShouldBe(command.ResourceId);
            document.Tags.ShouldBe(command.Tags);
        }

        #region ARRANGE

        private const string Namespace = "availability";
        private readonly RabbitMqFixture _rabbitMqFixture;
        private readonly MongoDbFixture<ResourceDocument, Guid> _mongoDbFixture;
        
        public AddResourceRabbitMqTests(PaccoApplicationFactory<Program> factory)
        {
            _rabbitMqFixture = new RabbitMqFixture(Namespace);
            _mongoDbFixture = new MongoDbFixture<ResourceDocument, Guid>("resources");
            factory.Server.AllowSynchronousIO = true;
        }

        public void Dispose()
        {
        }

        #endregion
    }
}