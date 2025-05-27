using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using StorkDork.Controllers;
using StorkDorkMain.Controllers;
using StorkDorkMain.Data;
using StorkDorkMain.DAL.Abstract;
using StorkDorkMain.Models;
using StorkDorkTests.Helpers;
using Newtonsoft.Json;
using StorkDorkMain.Services; // Add this line if INotificationService is in this namespace

namespace StorkDorkTests
{
    [TestFixture]
    public class BirdLogControllerTests
    {
        private BirdLogController _controller;
        private Mock<StorkDorkDbContext> _mockContext;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<ISDUserRepository> _mockSdUserRepository;
        private Mock<IMilestoneRepository> _mockMilestoneRepo;
        private Mock<INotificationService> _mockNotificationService;

        // ===================================================================
        // Setup & Teardown
        // ===================================================================
        [SetUp]
        public void Setup()
        {
            // Initialize mock dependencies
            _mockContext = new Mock<StorkDorkDbContext>();
            _mockSdUserRepository = new Mock<ISDUserRepository>();
            _mockMilestoneRepo = new Mock<IMilestoneRepository>();
            _mockNotificationService = new Mock<INotificationService>();

            // Initialize controller with all required dependencies
            _controller = new BirdLogController(
                _mockContext.Object,
                _mockUserManager.Object,
                _mockSdUserRepository.Object,
                _mockMilestoneRepo.Object,
                _mockNotificationService.Object
            );

        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup resources
            _controller?.Dispose();
            _controller = null;
        }

        // ===================================================================
        // Helper Methods
        // ===================================================================
        private Mock<DbSet<Bird>> GetMockBirdDbSet(List<Bird> birds)
        {
            // Creates a mock DbSet that behaves like an in-memory collection
            var mockSet = new Mock<DbSet<Bird>>();
            var queryableBirds = birds.AsQueryable();

            mockSet.As<IQueryable<Bird>>().Setup(m => m.Provider).Returns(queryableBirds.Provider);
            mockSet.As<IQueryable<Bird>>().Setup(m => m.Expression).Returns(queryableBirds.Expression);
            mockSet.As<IQueryable<Bird>>().Setup(m => m.ElementType).Returns(queryableBirds.ElementType);
            mockSet.As<IQueryable<Bird>>().Setup(m => m.GetEnumerator()).Returns(queryableBirds.GetEnumerator());
            
            return mockSet;
        }

        private ClaimsPrincipal GetMockUser(string userId)
        {
            // Creates a mock authenticated user
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            return new ClaimsPrincipal(identity);
        }

        // ===================================================================
        // Test Cases
        // ===================================================================
        
        // -------------------------------------------------------------------
        // Test 1: Empty Search Term Handling
        // -------------------------------------------------------------------
        [Test]
        public async Task GetBirdSpecies_ReturnsEmptyList_WhenTermIsEmpty()
        {
            // Purpose: Validate empty search term returns empty results
            // Expected: Empty JSON array
            // Dependencies: BirdLogController.GetBirdSpecies()

            // Arrange
            const string term = "";

            // Act
            var result = await _controller.GetBirdSpecies(term);

            // Assert
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = result as JsonResult;
            var birds = jsonResult?.Value as List<object>;
            Assert.IsEmpty(birds);
        }

        // -------------------------------------------------------------------
        // Test 2: Valid Sighting Submission
        // -------------------------------------------------------------------
        [Test]
        public async Task Create_Success_WhenBothLocationAndBirdAreProvided()
        {
            // Purpose: Validate successful submission with valid data
            // Expected: Redirect to confirmation page
            // Dependencies: SdUserRepository, Birds DbSet

            // Arrange
            var sighting = new Sighting 
            { 
                BirdId = 1, 
                Date = DateTime.Now, 
                Notes = "Test notes" 
            };
            const string selectedLocation = "48.4244,-122.3358";

            // Mock database operations
            _mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);
            
            // Mock user resolution
            _mockSdUserRepository
                .Setup(m => m.GetSDUserByIdentity(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new SdUser { Id = 1 });

            // Configure HTTP context
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "PnwLocation", selectedLocation }
            });
            httpContext.User = GetMockUser("test-user-id");
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var sightingJson = JsonConvert.SerializeObject(sighting);
            var result = await _controller.Create(sightingJson); // 

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("Confirmation", redirectResult?.ActionName);
        }

        // -------------------------------------------------------------------
        // Test 3: Missing Location Validation
        // -------------------------------------------------------------------
        [Test]
        public async Task Create_Error_WhenLocationIsEmpty()
        {
            // Purpose: Validate location field requirement
            // Expected: Model error for PnwLocation
            // Dependencies: User identity system, Birds DbSet

            // Arrange
            var sighting = new Sighting 
            { 
                BirdId = 1, 
                Date = DateTime.Now, 
                Notes = "Test notes" 
            };
            const string selectedLocation = "";

            // Mock database context
            var mockBirds = GetMockBirdDbSet(new List<Bird>());
            _mockContext.Setup(m => m.Birds).Returns(mockBirds.Object);

            // Mock user resolution
            _mockSdUserRepository
                .Setup(m => m.GetSDUserByIdentity(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new SdUser { Id = 1 });

            // Configure HTTP context
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "PnwLocation", selectedLocation }
            });
            httpContext.User = GetMockUser("test-user-id");
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var sightingJson = JsonConvert.SerializeObject(sighting);
            var result = await _controller.Create(sightingJson);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsTrue(viewResult?.ViewData.ModelState.ContainsKey("PnwLocation"));
            Assert.AreEqual(
                "Please select a location or select N/A",
                viewResult?.ViewData.ModelState["PnwLocation"]?.Errors[0].ErrorMessage
            );
        }

        // -------------------------------------------------------------------
        // Test 4: Missing Bird Validation
        // -------------------------------------------------------------------
        [Test]
        public async Task Create_ReturnsViewWithModelError_WhenBirdIsEmpty()
        {
            // Purpose: Validate bird selection requirement
            // Expected: Model error for BirdId
            // Dependencies: Birds DbSet, Location validation

            // Arrange
            var sighting = new Sighting 
            { 
                BirdId = null, 
                Date = DateTime.Now, 
                Notes = "Test notes" 
            };
            const string selectedLocation = "48.4244,-122.3358";

            // Mock database context
            var mockBirds = GetMockBirdDbSet(new List<Bird>());
            _mockContext.Setup(m => m.Birds).Returns(mockBirds.Object);

            // Mock user resolution
            _mockSdUserRepository
                .Setup(m => m.GetSDUserByIdentity(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new SdUser { Id = 1 });

            // Configure HTTP context
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "PnwLocation", selectedLocation }
            });
            httpContext.User = GetMockUser("test-user-id");
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var sightingJson = JsonConvert.SerializeObject(sighting);
            var result = await _controller.Create(sightingJson);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsTrue(viewResult?.ViewData.ModelState.ContainsKey("BirdId"));
            Assert.AreEqual(
                "Please select a bird or choose N/A.", 
                viewResult?.ViewData.ModelState["BirdId"]?.Errors[0].ErrorMessage
            );
        }

        // -------------------------------------------------------------------
        // Test 5: Combined Validation Errors
        // -------------------------------------------------------------------
        [Test]
        public async Task Create_ReturnsViewWithModelErrors_WhenBothLocationAndBirdAreEmpty()
        {
            // Purpose: Validate combined field requirements
            // Expected: Errors for both BirdId and PnwLocation
            // Dependencies: Full validation stack

            // Arrange
            var sighting = new Sighting 
            { 
                BirdId = null, 
                Date = DateTime.Now, 
                Notes = "Test notes" 
            };
            const string selectedLocation = "";

            // Mock database context
            var mockBirds = GetMockBirdDbSet(new List<Bird>());
            _mockContext.Setup(m => m.Birds).Returns(mockBirds.Object);

            // Mock user resolution
            _mockSdUserRepository
                .Setup(m => m.GetSDUserByIdentity(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new SdUser { Id = 1 });

            // Configure HTTP context
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "PnwLocation", selectedLocation }
            });
            httpContext.User = GetMockUser("test-user-id");
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var sightingJson = JsonConvert.SerializeObject(sighting);
            var result = await _controller.Create(sightingJson);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            
            // Validate bird error
            Assert.IsTrue(viewResult?.ViewData.ModelState.ContainsKey("BirdId"));
            Assert.AreEqual(
                "Please select a bird or choose N/A.", 
                viewResult?.ViewData.ModelState["BirdId"]?.Errors[0].ErrorMessage
            );
            
            // Validate location error
            Assert.IsTrue(viewResult?.ViewData.ModelState.ContainsKey("PnwLocation"));
            Assert.AreEqual(
                "Please select a location or choose N/A.", 
                viewResult?.ViewData.ModelState["PnwLocation"]?.Errors[0].ErrorMessage
            );
        }
    }
}