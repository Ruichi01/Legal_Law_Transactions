using Legal_Law_Transactions.Models;
using Microsoft.EntityFrameworkCore;

namespace Legal_Law_Transactions.Services
{
    public class AdminLogService : IAdminLogService
    {
        private readonly ApplicationDbContext _context;

        public AdminLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void LogAction(int adminId, string action, string target, string details)
        {
            var log = new AdminLog
            {
                adminId = adminId,
                Action = action,
                Target = target,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.AdminLogs.Add(log);
            _context.SaveChanges();
        }
    }
}
