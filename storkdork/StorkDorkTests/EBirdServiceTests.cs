// StorkDorkTests/EBirdServiceTests.cs
using NUnit.Framework;
using Moq;
using Moq.Protected;
using StorkDorkMain.Services;
using StorkDorkMain.DAL.Abstract;
using StorkDorkMain.Models;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;

namespace StorkDorkTests
{
    [TestFixture]
    public class EBirdServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpHandler;
        private HttpClient _httpClient;
        private Mock<IConfiguration> _mockConfig;
        private Mock<IBirdRepository> _mockBirdRepo;
        private EBirdService _eBirdService;

        [SetUp]
        public void Setup()
        {
            _mockHttpHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpHandler.Object);
            _mockConfig = new Mock<IConfiguration>();
            _mockBirdRepo = new Mock<IBirdRepository>();

            _mockConfig.Setup(c => c["EBird:ApiKey"]).Returns("test-api-key");
            _eBirdService = new EBirdService(_httpClient, _mockConfig.Object, _mockBirdRepo.Object);
        }

        [Test]
        public async Task GetNearestSightings_ValidBird_ReturnsSightings()
        {
            // Arrange
            var birdId = 1;
            var bird = new Bird
            {
                Id = birdId,
                CommonName = "Canada Goose",
                SpeciesCode = "cangoo"
            };
            var jsonResponse = @"[
                {
                    ""speciesCode"": ""cangoo"",
                    ""comName"": ""Canada Goose"",
                    ""locName"": ""Test Location"",
                    ""obsDt"": ""2024-03-06 10:00"",
                    ""howMany"": 2,
                    ""lat"": 44.8485,
                    ""lng"": -123.2340
                }
            ]";

            _mockBirdRepo.Setup(r => r.FindById(birdId)).Returns(bird);
            SetupMockHttpResponse(jsonResponse);

            // Act
            var sightings = await _eBirdService.GetNearestSightings(birdId, 44.8485, -123.2340);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(sightings, Is.Not.Empty);
                var sighting = sightings.First();
                Assert.That(sighting.BirdId, Is.EqualTo(birdId));
                Assert.That(sighting.Latitude, Is.EqualTo(44.8485m));
                Assert.That(sighting.Longitude, Is.EqualTo(-123.2340m));
            });
        }

        [Test]
        public async Task GetNearestSightings_InvalidBird_ReturnsEmptyList()
        {
            // Arrange
            var birdId = -1;
            _mockBirdRepo.Setup(r => r.FindById(birdId)).Returns((Bird)null);

            // Act
            var sightings = await _eBirdService.GetNearestSightings(birdId, 44.8485, -123.2340);

            // Assert
            Assert.That(sightings, Is.Empty);
        }

        [Test]
        public async Task GetNearestSightings_ApiError_ReturnsEmptyList()
        {
            // Arrange
            var birdId = 1;
            var bird = new Bird { Id = birdId, SpeciesCode = "cangoo" };
            _mockBirdRepo.Setup(r => r.FindById(birdId)).Returns(bird);
            SetupMockHttpResponse("", HttpStatusCode.InternalServerError);

            // Act
            var sightings = await _eBirdService.GetNearestSightings(birdId, 44.8485, -123.2340);

            // Assert
            Assert.That(sightings, Is.Empty);
        }

        [Test]
        public async Task GetNearestSightings_EmptyResponse_ReturnsEmptyList()
        {
            // Arrange
            var birdId = 1;
            var bird = new Bird { Id = birdId, SpeciesCode = "cangoo" };
            _mockBirdRepo.Setup(r => r.FindById(birdId)).Returns(bird);
            SetupMockHttpResponse("[]");

            // Act
            var sightings = await _eBirdService.GetNearestSightings(birdId, 44.8485, -123.2340);

            // Assert
            Assert.That(sightings, Is.Empty);
        }

        [Test]
        public async Task GetNearbySightings_ReturnsUniqueSightings()
        {
            // Arrange
            var jsonResponse = @"[
                {
                    ""speciesCode"": ""cangoo"",
                    ""comName"": ""Canada Goose"",
                    ""sciName"": ""Branta canadensis"",
                    ""locName"": ""Location A"",
                    ""obsDt"": ""2024-05-20 10:00"",
                    ""howMany"": 2,
                    ""lat"": 44.8485,
                    ""lng"": -123.2340
                },
                {
                    ""speciesCode"": ""cangoo"",
                    ""comName"": ""Canada Goose"",
                    ""sciName"": ""Branta canadensis"",
                    ""locName"": ""Location B"",
                    ""obsDt"": ""2024-05-25 10:00"",
                    ""howMany"": 5,
                    ""lat"": 44.9485,
                    ""lng"": -123.3340
                },
                {
                    ""speciesCode"": ""amecro"",
                    ""comName"": ""American Crow"",
                    ""sciName"": ""Corvus brachyrhynchos"",
                    ""locName"": ""Location C"",
                    ""obsDt"": ""2024-05-24 10:00"",
                    ""howMany"": 1,
                    ""lat"": 44.7485,
                    ""lng"": -123.1340
                }
            ]";

            SetupMockHttpResponse(jsonResponse);

            // Act
            var sightings = await _eBirdService.GetNearbySightings(44.8485, -123.2340, 25);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(sightings.Count(), Is.EqualTo(2)); // Two unique species
                
                // Check that for Canada Goose, we get the more recent sighting (Location B)
                var canadaGoose = sightings.FirstOrDefault(s => s.SpeciesCode == "cangoo");
                Assert.That(canadaGoose, Is.Not.Null);
                Assert.That(canadaGoose.LocationName, Is.EqualTo("Location B"));
                Assert.That(canadaGoose.Count, Is.EqualTo(5));
                
                var crow = sightings.FirstOrDefault(s => s.SpeciesCode == "amecro");
                Assert.That(crow, Is.Not.Null);
                Assert.That(crow.CommonName, Is.EqualTo("American Crow"));
            });
        }

        [Test]
        public async Task GetNearbySightings_ApiError_ReturnsEmptyList()
        {
            // Arrange
            SetupMockHttpResponse("", HttpStatusCode.InternalServerError);

            // Act
            var sightings = await _eBirdService.GetNearbySightings(44.8485, -123.2340, 25);

            // Assert
            Assert.That(sightings, Is.Empty);
        }

        [Test]
        public async Task GetNearbySightings_EmptyResponse_ReturnsEmptyList()
        {
            // Arrange
            SetupMockHttpResponse("[]");

            // Act
            var sightings = await _eBirdService.GetNearbySightings(44.8485, -123.2340, 25);

            // Assert
            Assert.That(sightings, Is.Empty);
        }

        [Test]
        public async Task GetNearbySightings_InvalidRadius_ReturnsEmptyList()
        {
            // Arrange
            var lat = 44.8485;
            var lng = -123.2340;

            // Act with invalid radius
            var sightings = await _eBirdService.GetNearbySightings(lat, lng, 0);

            // Assert
            Assert.That(sightings, Is.Empty);
        }

        [Test]
        public async Task GetNearbySightings_ValidRadius_ReturnsSightings()
        {
            // Arrange
            var lat = 44.8485;
            var lng = -123.2340;
            var radius = 25; // km
            var jsonResponse = @"[
                {
                    ""speciesCode"": ""cangoo"",
                    ""comName"": ""Canada Goose"",
                    ""sciName"": ""Branta canadensis"",
                    ""locName"": ""Test Location"",
                    ""obsDt"": ""2024-03-06 10:00"",
                    ""howMany"": 2,
                    ""lat"": 44.8485,
                    ""lng"": -123.2340
                }
            ]";

            SetupMockHttpResponse(jsonResponse);

            // Act
            var sightings = await _eBirdService.GetNearbySightings(lat, lng, radius);

            // Assert
            Assert.That(sightings, Is.Not.Empty);
            Assert.That(sightings.First().CommonName, Is.EqualTo("Canada Goose"));
        }

        private void SetupMockHttpResponse(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }
    }
}