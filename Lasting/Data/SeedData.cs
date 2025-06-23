using Lasting.Data;
using Lasting.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lasting.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                // Tạo các role nếu chưa tồn tại
                string[] roleNames = { "Admin", "Staff", "Customer" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Tạo tài khoản Admin mẫu
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FullName = "Quản trị viên"
                };

                var adminResult = await userManager.CreateAsync(adminUser, "Admin@123");
                if (adminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }

                // Tạo tài khoản Staff mẫu
                var staffUser = new ApplicationUser
                {
                    UserName = "staff@example.com",
                    Email = "staff@example.com",
                    FullName = "Nhân viên"
                };

                var staffResult = await userManager.CreateAsync(staffUser, "    ");
                if (staffResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(staffUser, "Staff");
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Lỗi khi khởi tạo dữ liệu");
            }
        }
    }
}