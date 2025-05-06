namespace Legal_Law_Transactions.Services
{
    public interface IAdminLogService
    {
        void LogAction(int adminId, string action, string target, string details);
    }

}
    