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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var me = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var dto = await _profileService.GetByUserIdAsync(me);
            if (dto == null) return NotFound();

            ViewBag.IsOwner = true;
            return View(dto);   
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var dto = await _profileService.GetByUserIdAsync(id);
            if (dto == null) return NotFound();

            var me = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            ViewBag.IsOwner = (id == me);
            return View(dto);   
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var me = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var dto = await _profileService.GetByUserIdAsync(me);
            if (dto == null) return NotFound();

            ViewBag.CurrentFirstName = dto.FirstName;
            ViewBag.CurrentLastName = dto.LastName;
            ViewBag.CurrentBio = dto.Bio;
            ViewBag.CurrentAvatar = dto.ProfilePicUrl;
            ViewBag.UserInitial = !string.IsNullOrEmpty(dto.UserName) ? dto.UserName[0].ToString() : "?";

            return View(new UpdateProfileRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Edit(UpdateProfileRequest model)
        {
            if (model.Avatar != null)
            {
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif" };
                var extension = Path.GetExtension(model.Avatar.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Avatar", $"File extension \"{extension}\" is not supported (allowed: {allowedExtensions})");
                }

                const long maxFileSize = 5 * 1024 * 1024; 
                if (model.Avatar.Length > maxFileSize)
                {
                    ModelState.AddModelError("Avatar", "File is too large, maximum size is 5 MB");
                }
            }

            if (!ModelState.IsValid)
            {
                var me = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var dto = await _profileService.GetByUserIdAsync(me);
                ViewBag.CurrentFirstName = dto.FirstName;
                ViewBag.CurrentLastName = dto.LastName;
                ViewBag.CurrentBio = dto.Bio;
                ViewBag.CurrentAvatar = dto.ProfilePicUrl;
                ViewBag.UserInitial = !string.IsNullOrEmpty(dto.UserName) ? dto.UserName[0].ToString() : "?";

                return View(model);
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _profileService.UpdateAsync(userId, model, model.Avatar);

            var updatedDto = await _profileService.GetByUserIdAsync(userId);

            await _profileHub.Clients.Group(userId.ToString()).SendAsync("ProfileUpdated", updatedDto);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _profileService.DeleteAccountAsync(userId);

            await HttpContext.SignOutAsync();

            return RedirectToAction("Login", "Account");
        }

    }
}
