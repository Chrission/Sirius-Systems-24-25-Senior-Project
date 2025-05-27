using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using StorkDorkMain.Controllers;
using StorkDorkMain.DAL.Abstract;
using StorkDorkMain.Data;
using StorkDorkMain.Models;
using StorkDorkMain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StorkDorkTests
{
    [TestFixture]
    public class ChecklistAutoUpdateTests : IDisposable
    {
        // Controller and mocks
        private BirdLogController _controller;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<ISDUserRepository> _mockSdUserRepository;
        private Mock<IMilestoneRepository> _mockMilestoneRepository;
        private Mock<INotificationService> _mockNotificationService;
        private StorkDorkDbContext _dbContext;
        
        // Test data
        private SdUser _testUser;
        private Bird _testBird1;
        private Bird _testBird2;
        private Checklist _testChecklist;
        private bool _disposed;

        [SetUp]
        public void Setup()
        {
            // Set up in-memory database
            var options = new DbContextOptionsBuilder<StorkDorkDbContext>()
                .UseInMemoryDatabase(databaseName: $"ChecklistAutoUpdate_{Guid.NewGuid()}")
                .Options;
            _dbContext = new StorkDorkDbContext(options);
            
            // Set up UserManager mock
            var mockUserStore = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                mockUserStore.Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<IdentityUser>>().Object,
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<IdentityUser>>>().Object);

            // Set up repository mocks
            _mockSdUserRepository = new Mock<ISDUserRepository>();
            _mockMilestoneRepository = new Mock<IMilestoneRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            
            // Mock notification service success
            _mockNotificationService
                .Setup(ns => ns.CreateNotificationAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(true);
            
            // Create test data
            SetupTestData();
            
            // Create controller with dependencies
            _controller = new BirdLogController(
                _dbContext,
                _mockUserManager.Object,
                _mockSdUserRepository.Object,
                _mockMilestoneRepository.Object,
                _mockNotificationService.Object);
            
            // Set up controller context with test user
            SetupControllerContext();
        }

        private void SetupTestData()
        {
            // Create test user
            _testUser = new SdUser
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com"
            };
            _dbContext.SdUsers.Add(_testUser);

            // Create test birds
            _testBird1 = new Bird
            {
                Id = 1,
                CommonName = "Northern Cardinal",
                ScientificName = "Cardinalis cardinalis"
            };
            
            _testBird2 = new Bird
            {
                Id = 2,
                CommonName = "American Robin",
                ScientificName = "Turdus migratorius"
            };
            
            _dbContext.Birds.AddRange(_testBird1, _testBird2);

            // Create milestone for user
            var milestone = new Milestone
            {
                Id = 1,
                SDUserId = _testUser.Id,
                SightingsMade = 0
            };
            
            _dbContext.Milestone.Add(milestone);

            // Create test checklist
            _testChecklist = new Checklist
            {
                Id = 1,
                ChecklistName = "Test Checklist",
                SdUserId = _testUser.Id
            };
            
            _dbContext.Checklists.Add(_testChecklist);
            
            // Add checklist items
            var checklistItems = new List<ChecklistItem>
            {
                new ChecklistItem
                {
                    Id = 1,
                    ChecklistId = _testChecklist.Id,
                    BirdId = _testBird1.Id,
                    Sighted = false
                },
                new ChecklistItem
                {
                    Id = 2,
                    ChecklistId = _testChecklist.Id,
                    BirdId = _testBird2.Id,
                    Sighted = null
                }
            };
            
            _dbContext.ChecklistItems.AddRange(checklistItems);
            
            // Create another user and their checklist
            var otherUser = new SdUser
            {
                Id = 2,
                FirstName = "Other",
                LastName = "User",
                Email = "other@example.com"
            };
            
            _dbContext.SdUsers.Add(otherUser);
            
            var otherChecklist = new Checklist
            {
                Id = 2,
                ChecklistName = "Other User's Checklist",
                SdUserId = otherUser.Id
            };
            
            _dbContext.Checklists.Add(otherChecklist);
            
            // Add item to other user's checklist
            var otherChecklistItem = new ChecklistItem
            {
                Id = 3,
                ChecklistId = otherChecklist.Id,
                BirdId = _testBird1.Id,
                Sighted = false
            };
            
            _dbContext.ChecklistItems.Add(otherChecklistItem);
            
            _dbContext.SaveChanges();
            
            // Setup mock repository to return our test user
            _mockSdUserRepository
                .Setup(repo => repo.GetSDUserByIdentity(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(_testUser);
        }

        private void SetupControllerContext()
        {
            // Create claims identity for the test user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            // Create mock HttpContext
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            // Assign the HttpContext to the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TearDown]
        public void TearDown()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _controller?.Dispose();
                    _dbContext?.Dispose();
                }
                _disposed = true;
            }
        }

        [Test]
        public async Task Create_SightingWithExistingBird_UpdatesChecklistItems()
        {
            // Arrange
            var sighting = new Sighting
            {
                SdUserId = _testUser.Id,
                BirdId = _testBird1.Id,
                Date = DateTime.Now,
                Notes = "Test sighting"
            };

            // Act
            var result = await _controller.Create(sighting);

            // Assert
            // 1. Check that the checklist item was updated
            var checklistItem = await _dbContext.ChecklistItems.FindAsync(1);
            Assert.That(checklistItem.Sighted, Is.True, "Checklist item should be marked as sighted");
            
            // 2. Check that other user's checklist was not modified
            var otherUserItem = await _dbContext.ChecklistItems.FindAsync(3);
            Assert.That(otherUserItem.Sighted, Is.False, "Other user's checklist should not be modified");
            
            // 3. Verify the redirect result
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = (RedirectToActionResult)result;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Confirmation"));
        }

        [Test]
        public async Task Create_SightingWithNullValuedItem_UpdatesToTrue()
        {
            // Arrange
            var sighting = new Sighting
            {
                SdUserId = _testUser.Id,
                BirdId = _testBird2.Id,
                Date = DateTime.Now,
                Notes = "Test sighting for bird with null sighted value"
            };

            // Act
            await _controller.Create(sighting);

            // Assert
            var checklistItem = await _dbContext.ChecklistItems.FindAsync(2);
            Assert.That(checklistItem.Sighted, Is.True, "Null-valued checklist item should be updated to true");
        }

        [Test]
        public async Task Create_SightingWithBirdNotInChecklist_DoesNotThrowError()
        {
            // Arrange
            var newBird = new Bird
            {
                Id = 3,
                CommonName = "Bald Eagle",
                ScientificName = "Haliaeetus leucocephalus"
            };
            
            _dbContext.Birds.Add(newBird);
            await _dbContext.SaveChangesAsync();
            
            var sighting = new Sighting
            {
                SdUserId = _testUser.Id,
                BirdId = newBird.Id,
                Date = DateTime.Now,
                Notes = "Test sighting for bird not in any checklist"
            };

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _controller.Create(sighting));
            
            // Additional verification - sighting should be created
            var createdSighting = await _dbContext.Sightings.FirstOrDefaultAsync(s => s.BirdId == newBird.Id);
            Assert.That(createdSighting, Is.Not.Null, "Sighting should be created even for birds not in checklists");
        }

        [Test]
        public async Task Create_AlreadySightedBird_DoesNotUpdateAgain()
        {
            // Arrange
            // First mark the item as sighted
            var checklistItem = await _dbContext.ChecklistItems.FindAsync(1);
            checklistItem.Sighted = true;
            await _dbContext.SaveChangesAsync();
            
            // Create a new sighting for the same bird
            var sighting = new Sighting
            {
                SdUserId = _testUser.Id,
                BirdId = _testBird1.Id,
                Date = DateTime.Now,
                Notes = "Test sighting for already sighted bird"
            };

            // Act
            await _controller.Create(sighting);
            
            // Assert - nothing has changed
            var updatedItem = await _dbContext.ChecklistItems.FindAsync(1);
            Assert.That(updatedItem.Sighted, Is.True, "Item should still be marked as sighted");
        }

        [Test]
        public async Task Create_MultipleChecklistsWithSameBird_UpdatesAllUserChecklists()
        {
            // Arrange
            // Add another checklist for the test user with the same bird
            var secondChecklist = new Checklist
            {
                Id = 3,
                ChecklistName = "Second Test Checklist",
                SdUserId = _testUser.Id
            };
            
            _dbContext.Checklists.Add(secondChecklist);
            
            var secondChecklistItem = new ChecklistItem
            {
                Id = 4,
                ChecklistId = secondChecklist.Id,
                BirdId = _testBird1.Id,
                Sighted = false
            };
            
            _dbContext.ChecklistItems.Add(secondChecklistItem);
            await _dbContext.SaveChangesAsync();
            
            var sighting = new Sighting
            {
                SdUserId = _testUser.Id,
                BirdId = _testBird1.Id,
                Date = DateTime.Now,
                Notes = "Test sighting for bird in multiple checklists"
            };

            // Act
            await _controller.Create(sighting);

            // Assert
            var firstChecklistItem = await _dbContext.ChecklistItems.FindAsync(1);
            var secondChecklistItemUpdated = await _dbContext.ChecklistItems.FindAsync(4);
            
            Assert.Multiple(() => {
                Assert.That(firstChecklistItem.Sighted, Is.True, "First checklist item should be marked as sighted");
                Assert.That(secondChecklistItemUpdated.Sighted, Is.True, "Second checklist item should also be marked as sighted");
            });
        }
    }
}