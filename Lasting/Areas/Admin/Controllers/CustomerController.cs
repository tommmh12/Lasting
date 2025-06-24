using Lasting.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lasting.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: Admin/Customer
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // GET: Admin/Customer/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Admin/Customer/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    user.FullName = model.FullName;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    user.UserName = model.Email; // Thường UserName sẽ giống Email

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = "Thông tin khách hàng đã được cập nhật!";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UserExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(model);
        }

        // POST: Admin/Customer/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Khách hàng đã được xóa!";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa khách hàng!";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> UserExists(string id)
        {
            return await _userManager.FindByIdAsync(id) != null;
        }
    }
}