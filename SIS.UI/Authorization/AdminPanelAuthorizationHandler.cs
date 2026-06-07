using Microsoft.AspNetCore.Authorization;
using SIS.UI.ApiClients;

namespace SIS.UI.Authorization
{
    public class AdminPanelAuthorizationHandler : AuthorizationHandler<AdminPanelRequirement>
    {
        private readonly IAdminAccessApi _adminAccessApi;

        public AdminPanelAuthorizationHandler(IAdminAccessApi adminAccessApi)
        {
            _adminAccessApi = adminAccessApi;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, AdminPanelRequirement requirement)
        {
            var user = context.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return;

            // SuperAdmin Academic Admin panelə həmişə icazəlidir.
            if (user.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
                return;
            }

            // Moderator yalnız PublicAdmin panelə icazəlidir — Academic Admin DENY.
            if (user.IsInRole("Moderator"))
            {
                return;
            }

            // Admin yalnız aktiv və vaxtı keçməmiş grant olduqda Academic Admin panelə girə bilər.
            if (user.IsInRole("Admin"))
            {
                try
                {
                    if (await _adminAccessApi.HasAccessAsync())
                        context.Succeed(requirement);
                }
                catch
                {
                    // Yoxlama mümkün olmadıqda fail-closed (icazə verilmir).
                }
                return;
            }

            // Teacher / Student / digərləri: deny.
        }
    }
}
