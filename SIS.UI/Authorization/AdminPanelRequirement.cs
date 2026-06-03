using Microsoft.AspNetCore.Authorization;

namespace SIS.UI.Authorization
{
    /// <summary>
    /// Admin panelə giriş tələbi: SuperAdmin həmişə, Admin yalnız aktiv (vaxtı keçməmiş)
    /// access grant olduqda, Moderator (mövcud davranış) icazəli; digərləri deny.
    /// </summary>
    public class AdminPanelRequirement : IAuthorizationRequirement { }
}
