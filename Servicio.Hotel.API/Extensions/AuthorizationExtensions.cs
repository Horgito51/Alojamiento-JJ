using Servicio.Hotel.API.Authorization;

namespace Servicio.Hotel.API.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.AdminProfile, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context =>
                        !context.User.IsInRole(AuthorizationPolicies.ClienteRole));
                });
            });

            return services;
        }
    }
}
