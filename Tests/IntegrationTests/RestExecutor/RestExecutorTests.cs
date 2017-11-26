using NUnit.Framework;
using Rhino.Mocks;
using Shouldly;
using Services.Infrastructure.RestConfiguration;

namespace Tests.IntegrationTests.RestExecutor
{
    [TestFixture]
    public class RestExecutorTests
    {
        private IRestConfiguration _restConfiguration;

        [SetUp]
        public void Setup()
        {
            _restConfiguration = MockRepository.GenerateMock<IRestConfiguration>();
        }

		[TestCase("http://loripsum.net/api", "headers", "Lorem ipsum dolor sit amet")]
		[TestCase("http://jsonplaceholder.typicode.com", "posts/1", "sunt aut facere repellat provident occaecati excepturi optio reprehenderit")]
        public void Execute_Returns_Response(string restApi, string parameter, string expectedSubstring)
        {
			_restConfiguration.Stub(x => x.RestApi).Return(restApi);

			var execute = GetRestExecutor();
			var response = execute.Execute(parameter);

			response.ShouldNotBeNull();
			response.ShouldContain(expectedSubstring);
		}

		public Services.Integration.RestExecutor.RestExecutor GetRestExecutor()
		{
			return new Services.Integration.RestExecutor.RestExecutor(_restConfiguration);
		}
    }
}