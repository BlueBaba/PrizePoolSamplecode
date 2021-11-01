using System;
using Microsoft.Extensions.DependencyInjection;

namespace App.Utilities
{
    public static class ServiceCollectionExtentions
        {
            public static IServiceCollection AddPolicy(this IServiceCollection services, String PolicyName = "", String ClaimType = "", String ClaimValue = "")
            {
                if (!String.IsNullOrWhiteSpace(ClaimType))
                {
                    services.AddAuthorization(options =>
                    {


                        options.AddPolicy(PolicyName, policy => policy.RequireClaim(ClaimType, ClaimValue));

                    });
                }

                return services;
            }

        }
    
}
