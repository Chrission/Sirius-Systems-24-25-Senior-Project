using Microsoft.AspNetCore.Mvc;
using StorkDorkMain.Models;
using StorkDorkMain.DAL.Abstract;
using StorkDorkMain.DAL.Concrete;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace StorkDork.Controllers;

[Authorize]
public class MilestoneController : Controller
{
    private readonly IMilestoneRepository _milestoneRepo;
    private readonly ISDUserRepository _sduserRepo;

    public MilestoneController(IMilestoneRepository milestoneRepo, ISDUserRepository sduserRepo)
    {
        _milestoneRepo = milestoneRepo;
        _sduserRepo = sduserRepo;
    }

    public async Task<IActionResult> Index()
    {
        SdUser user = await _sduserRepo.GetSDUserByIdentity(User);
        MilestoneViewModel vm = new MilestoneViewModel();
        Milestone ms = new Milestone();
        ms.SightingsMade = await _milestoneRepo.GetSightingsMade(user.Id);
        ms.PhotosContributed = await _milestoneRepo.GetPhotosContributed(user.Id);
        vm.FirstName = user.FirstName;
        vm.Milestone = ms;
        vm.SightingsTier = _milestoneRepo.GetMilestoneTier(ms.SightingsMade);
        vm.PhotosTier = _milestoneRepo.GetMilestoneTier(ms.PhotosContributed);

        return View(vm);
    }
}