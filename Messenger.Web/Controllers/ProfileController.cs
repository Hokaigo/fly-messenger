using Messenger.Application.DTOs.Profile;
using Messenger.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Messenger.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly IHubContext<ProfileHub> _profileHub;

        public ProfileController(IProfileService profileService, IHubContext<ProfileHub> profileHub)
        {
            _profileService = profileService;
            _profileHub = profileHub;
        }

        public async Task<IActionResult> Index()
        {
            var me = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var dto = await _profileService.GetByUserIdAsync(me);
            if (dto == null) return NotFound();
            ViewBag.IsOwner = true;
            return View(dto);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var dto = await _profileService.GetByUserIdAsync(id);
            if (dto == null) return NotFound();
            ViewBag.IsOwner = (id == Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var me = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var dto = await _profileService.GetByUserIdAsync(me);
            if (dto == null) return NotFound();

            ViewBag.CurrentFirstName = dto.FirstName;
            ViewBag.CurrentLastName = dto.LastName;
            ViewBag.CurrentBio = dto.Bio;
            ViewBag.CurrentAvatar = dto.ProfilePicUrl;
            ViewBag.UserInitial = dto.UserName.FirstOrDefault().ToString();

            return View(new UpdateProfileRequest());
        }

        [HttpPost, ValidateAntiForgeryToken, DisableRequestSizeLimit]
        public async Task<IActionResult> Edit(UpdateProfileRequest model)
        {
            if (model.Avatar != null)
            {
                var ext = Path.GetExtension(model.Avatar.FileName).ToLowerInvariant();
                if (!new[] { ".png", ".jpg", ".jpeg", ".gif" }.Contains(ext))
                    ModelState.AddModelError("Avatar", "Unsupported extension.");

                if (model.Avatar.Length > 5 * 1024 * 1024)
                    ModelState.AddModelError("Avatar", "Max 5 MB.");
            }

            if (!ModelState.IsValid)
                return await Edit(); 

            var me = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _profileService.UpdateAsync(me, model, model.Avatar);

            var updated = await _profileService.GetByUserIdAsync(me);
            await _profileHub.Clients.Group(me.ToString()).SendAsync("ProfileUpdated", updated);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAccount()
        {
            var me = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _profileService.DeleteAccountAsync(me);
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
