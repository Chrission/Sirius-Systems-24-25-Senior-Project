using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using StorkDorkMain.Data;
using StorkDorkMain.Models;
using StorkDorkMain.DAL.Abstract;
using StorkDorkMain.Controllers;
using Moq;

namespace StorkDorkMain.Tests.Controllers
{
    [TestFixture]
    public class ChecklistEditTests
    {
        private StorkDorkDbContext   _context;
        private ISDUserRepository    _userRepo;
        private ChecklistsController _controller;

        [SetUp]
        public void SetUp()
        {

            var opts = new DbContextOptionsBuilder<StorkDorkDbContext>()
                           .UseInMemoryDatabase("TestDb")
                           .Options;
            var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            _context = new StorkDorkDbContext(opts, mockConfig.Object);

            // add irds
            _context.Birds.AddRange(
                new Bird { Id = 42, CommonName = "Blue Jay",    ScientificName = "Cyanocitta cristata", Order = "0" },
                new Bird { Id = 43, CommonName = "Cardinal",   ScientificName = "Cardinalis cardinalis", Order = "1" }
            );

            // seed a checklist
            var seededChecklist = new Checklist
            {
                Id            = 5,
                SdUserId      = 1,
                ChecklistName = "MyList",
                ChecklistItems =
                {
                    new ChecklistItem { BirdId = 42, Sighted = false }
                }
            };
            _context.Checklists.Add(seededChecklist);

            _context.SaveChanges();

            // user repository mock
            var mockRepo = new Mock<ISDUserRepository>();
            mockRepo
              .Setup(r => r.GetSDUserByIdentity(It.IsAny<ClaimsPrincipal>()))
              .ReturnsAsync(new SdUser { Id = 1 });
            _userRepo = mockRepo.Object;

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "1") },
                "TestAuth"
            ));
            _controller = new ChecklistsController(_context, _userRepo)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task Create_Post_ShouldAddChecklistAndChecklistItem_AndRedirectToIndex()
        {
            var newChecklist  = new Checklist { ChecklistName = "NewList" };
            var selectedBirds = new[] { 43 };

            var result = await _controller.Create(newChecklist, selectedBirds);

        
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.AreEqual("Index", ((RedirectToActionResult)result).ActionName);

            var saved = _context.Checklists
                                .Include(c => c.ChecklistItems)
                                .Single(c => c.ChecklistName == "NewList");
            Assert.AreEqual(1, saved.SdUserId);
            Assert.AreEqual(1, saved.ChecklistItems.Count);
            Assert.AreEqual(43, saved.ChecklistItems.Single().BirdId);
        }

        [Test]
        public async Task Edit_Post_ShouldUpdateNameAndBirds_AndRedirectToIndex()
        {
            var editedChecklist = new Checklist
            {
                Id            = 5,
                ChecklistName = "UpdatedList",
                SdUserId      = 1
            };
            var selectedBirds = new[] { 43 };

            var result = await _controller.Edit(5, editedChecklist, selectedBirds);

            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.AreEqual("Index", ((RedirectToActionResult)result).ActionName);

            var saved = _context.Checklists
                                .Include(c => c.ChecklistItems)
                                .Single(c => c.Id == 5);
            Assert.AreEqual("UpdatedList", saved.ChecklistName);
            Assert.AreEqual(1, saved.ChecklistItems.Count);
            Assert.AreEqual(43, saved.ChecklistItems.Single().BirdId);
        }

        [Test]
        public async Task Details_WithValidId_ReturnsViewWithChecklistModel()
        {
            var result = await _controller.Details(5) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Checklist>(result.Model);

            var model = (Checklist)result.Model;
            Assert.AreEqual(5, model.Id);
            Assert.AreEqual("MyList", model.ChecklistName);
            Assert.AreEqual(1, model.ChecklistItems.Count);
            var item = model.ChecklistItems.Single();
            Assert.AreEqual(42, item.BirdId);
            Assert.AreEqual("Blue Jay", item.Bird.CommonName);
        }

        [Test]
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            Assert.IsInstanceOf<NotFoundResult>(await _controller.Details(999));
            Assert.IsInstanceOf<NotFoundResult>(await _controller.Details(null));
        }
    }
}
