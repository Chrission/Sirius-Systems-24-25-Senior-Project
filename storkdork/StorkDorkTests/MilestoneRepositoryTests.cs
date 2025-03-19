using Moq;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using StorkDorkMain.Data;
using StorkDorkMain.Models;
using StorkDorkMain.DAL.Concrete;
<<<<<<< HEAD
using StorkDorkMain.DAL.Abstract;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StorkDork.Controllers;
using System.Security.Claims;
using StorkDorkMain.Models.DTO;
=======
using System.Linq;
using System.Threading.Tasks;
using StorkDorkMain.DAL.Abstract;
using Microsoft.AspNetCore.Mvc;
using StorkDork.Controllers;
using System.Security.Claims;
>>>>>>> dev

namespace StorkDorkMain.Tests.DAL
{
    [TestFixture]
    public class MilestoneRepositoryTests : IDisposable
    {
        private Mock<IMilestoneRepository> _mockMilestoneRepo;
        private Mock<ISDUserRepository> _mockSDUserRepo;
        private MilestoneController _controller;
        private List<Milestone> _testMilestones;
        private bool _disposed;

        [SetUp]
        public void SetUp()
        {
            _mockMilestoneRepo = new Mock<IMilestoneRepository>();
            _mockSDUserRepo = new Mock<ISDUserRepository>();
            _controller = new MilestoneController(_mockMilestoneRepo.Object, _mockSDUserRepo.Object);

            _testMilestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = 1,
                    SDUserId = 2,
                    SightingsMade = 25,
                    PhotosContributed = 25
                },
                new Milestone
                {
                    Id = 2,
                    SDUserId = 2,
                    SightingsMade = 53,
                    PhotosContributed = 51
                },
                new Milestone
                {
                    Id = 3,
                    SDUserId = 3,
                    SightingsMade = 82,
                    PhotosContributed = 77
                },
                new Milestone
                {
                    Id = 4,
                    SDUserId = 4,
                    SightingsMade = 162,
                    PhotosContributed = 122
                },
                new Milestone
                {
                    Id = 5,
                    SDUserId = 5,
                    SightingsMade = 0,
                    PhotosContributed = 0
                }
            };
        }

        [Test]
        public async Task MilestoneController_ReturnsIndexView()
        {
            // Arrange
            var mockUser = new SdUser
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User"
            };

            var mockMilestone = new Milestone
            {
                Id = 1,
                SDUserId = 1,
                SightingsMade = 5,
                PhotosContributed = 10
            };

            _mockSDUserRepo.Setup(repo => repo.GetSDUserByIdentity(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync(mockUser);

            _mockMilestoneRepo.Setup(repo => repo.GetSightingsMade(mockUser.Id))
                              .ReturnsAsync(mockMilestone.SightingsMade);

            _mockMilestoneRepo.Setup(repo => repo.GetPhotosContributed(mockUser.Id))
                              .ReturnsAsync(mockMilestone.PhotosContributed);

            _mockMilestoneRepo.Setup(repo => repo.GetMilestoneTier(mockMilestone.SightingsMade))
                              .Returns(2); // Example tier value

            _mockMilestoneRepo.Setup(repo => repo.GetMilestoneTier(mockMilestone.PhotosContributed))
                              .Returns(3); // Example tier value

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = (ViewResult)result;
            Assert.That(viewResult.ViewName, Is.EqualTo("Index").Or.Null); // null means it uses the default view
            Assert.That(viewResult.Model, Is.TypeOf<MilestoneViewModel>());

            var model = (MilestoneViewModel)viewResult.Model;
            Assert.That(model.FirstName, Is.EqualTo(mockUser.FirstName));
            Assert.That(model.Milestone.SightingsMade, Is.EqualTo(mockMilestone.SightingsMade));
            Assert.That(model.Milestone.PhotosContributed, Is.EqualTo(mockMilestone.PhotosContributed));
            Assert.That(model.SightingsTier, Is.EqualTo(2));
            Assert.That(model.PhotosTier, Is.EqualTo(3));
        }

        [Test]
        public async Task GetSightingsMade_ReturnsCorrectValue()
        {
            // Arrange: Set up a test user ID and expected SightingsMade value
            int testUserId = 1;
            int expectedSightingsMade = 5;

            // Mock the repository method to return the expected value
            _mockMilestoneRepo.Setup(repo => repo.GetSightingsMade(testUserId))
                              .ReturnsAsync(expectedSightingsMade);

            // Act: Call the method under test
            int result = await _mockMilestoneRepo.Object.GetSightingsMade(testUserId);

            // Assert: Verify the method returns the expected value
            Assert.That(result, Is.EqualTo(expectedSightingsMade));
        }

        [Test]
        public async Task GetPhotosContributed_ReturnsCorrectValue()
        {
            // Arrange: 

            int testUserId = 1;
            int expectedPhotosContributed = 3;

            _mockMilestoneRepo.Setup(repo => repo.GetPhotosContributed(testUserId))
                              .ReturnsAsync(expectedPhotosContributed);
            // Act
            int result = await _mockMilestoneRepo.Object.GetPhotosContributed(testUserId);

            // Assert
            Assert.That(result, Is.EqualTo(expectedPhotosContributed));
        }

        [Test]
        public void GetMilestoneTier_ReturnsCorrectTier()
        {
            // Arrange
            int sightingsMadeGold = 150;
            int sightingsMadeSilver = 85;
            int sightingsMadeBronze = 33;
            int sightingsMadeNone = 0;

            int noTier = 0;
            int bronzeTier = 1;
            int silverTier = 2;
            int goldTier = 3;

            _mockMilestoneRepo.Setup(repo => repo.GetMilestoneTier(sightingsMadeNone))
                              .Returns(noTier);

            _mockMilestoneRepo.Setup(repo => repo.GetMilestoneTier(sightingsMadeBronze))
                              .Returns(bronzeTier);

            _mockMilestoneRepo.Setup(repo => repo.GetMilestoneTier(sightingsMadeSilver))
                              .Returns(silverTier);

            _mockMilestoneRepo.Setup(repo => repo.GetMilestoneTier(sightingsMadeGold))
                              .Returns(goldTier);


            //Act
            int resultOne = _mockMilestoneRepo.Object.GetMilestoneTier(sightingsMadeNone);
            int resultTwo = _mockMilestoneRepo.Object.GetMilestoneTier(sightingsMadeBronze);
            int resultThree = _mockMilestoneRepo.Object.GetMilestoneTier(sightingsMadeSilver);
            int resultFour = _mockMilestoneRepo.Object.GetMilestoneTier(sightingsMadeGold);

            // Assert
            Assert.That(resultOne, Is.EqualTo(noTier));
            Assert.That(resultTwo, Is.EqualTo(bronzeTier));
            Assert.That(resultThree, Is.EqualTo(silverTier));
            Assert.That(resultFour, Is.EqualTo(goldTier));
        }

        [Test]
        public void IncrementSightingsMade_IncrementCorrectly()
        {
            // Arrange: Set up a test milestone with a known initial value
            int testUserId = 1;
            var testMilestone = new Milestone
            {
                SDUserId = testUserId,
                SightingsMade = 5
            };

            // Mock the repository to return the test milestone when queried
            _mockMilestoneRepo.Setup(repo => repo.GetSightingsMade(testUserId))
                              .ReturnsAsync(testMilestone.SightingsMade);

            _mockMilestoneRepo.Setup(repo => repo.IncrementSightingsMade(testUserId))
                              .Callback(() => testMilestone.SightingsMade++);

            // Act: Call the method under test
            _mockMilestoneRepo.Object.IncrementSightingsMade(testUserId);

            // Assert: Verify that the SightingsMade field was incremented correctly
            Assert.That(testMilestone.SightingsMade, Is.EqualTo(6));
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        [TearDown]
        public void TearDown()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _controller?.Dispose();
                }
                _mockMilestoneRepo = null;
                _mockSDUserRepo = null;
                _controller = null;
                _testMilestones = null;
                _disposed = true;
            }
        }
    }
}
