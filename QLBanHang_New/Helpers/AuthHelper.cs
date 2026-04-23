using Microsoft.AspNetCore.Http;
using QLBanHang_New.Data;

namespace QLBanHang_New.Helpers
{
    public static class AuthHelper
    {
        // LẤY ROLE 
        public static int GetRole(HttpContext context)
        {
            var role = context.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return 999; 

            if (int.TryParse(role, out int result))
                return result;

            return 999; 
        }

        //CHECK LOGIN 
        public static bool IsLoggedIn(HttpContext context)
        {
            var user = context.Session.GetString("User");
            return !string.IsNullOrEmpty(user);
        }

        // LẤY USER
        public static string? GetUser(HttpContext context)
        {
            return context.Session.GetString("User");
        }

        // LẤY USER ID
        public static int GetUserId(HttpContext context, ApplicationDbContext db)
        {
            var username = context.Session.GetString("User");

            if (string.IsNullOrEmpty(username))
                return 0;

            var user = db.Users.FirstOrDefault(x => x.Username == username);

            return user?.UserId ?? 0;
        }
    }
}