using Microsoft.OpenApi.Models;

namespace tapcet_api.Extensions
{
    public static class SwaggerServiceExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Tapcet Quiz API", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    new string[] {}
                }
            });
            });

            return services;
        }
    }
}
