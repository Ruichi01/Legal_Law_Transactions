using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        return userRole switch
        {
            "Admin" => RedirectToAction("AdminDashboard"),
            "Citizen" => RedirectToAction("CitizenDashboard"),
            "LawEnforcer" => RedirectToAction("LawEnforcerDashboard"),
            "Lawyer" => RedirectToAction("LawyerDashboard"),
            _ => View()
        };
    }

    public IActionResult AdminDashboard() => View();
    public IActionResult CitizenDashboard() => View();
    public IActionResult LawEnforcerDashboard() => View();
    public IActionResult LawyerDashboard() => View();
}
