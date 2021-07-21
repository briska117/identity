using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Iusacom.Tank.Services;
using Iusacom.Tank.Services.Customers;
using Iusacom.Tank.Services.CustomersServiceClient.Configuration;
using Iusacom.Tank.Services.Requests;
using Iusacomm.Recreo.Services.IdentityService.Configuration;
using Iusacomm.Recreo.Services.IdentityService.Data;
using Iusacomm.Recreo.Services.IdentityService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Iusacomm.Recreo.Services.IdentityService
{
    public class Startup
    {
        private const string ApiVersion = "v1";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<CustomerServiceSettings>(Configuration.GetSection("CustomerServiceSettings"));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddOptions();
            services.Configure<MessagingSettings>(Configuration.GetSection("Messaging"));
            services.Configure<JwtSettings>(Configuration.GetSection("Jwt"));
            services.Configure<LoginSettings>(Configuration.GetSection("Login"));
            services.AddControllers();

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 737280000;
            });

            services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                    };
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(IdentityConstants.ElevatedRightsPolicyName, policy =>
                {
                    policy.RequireClaim(ClaimTypes.Role, IdentityConstants.GlobalAdministrator);
                    policy.RequireRole(IdentityConstants.GlobalAdministrator);
                });

                options.AddPolicy(IdentityConstants.AdminRightsPolicyName, policy =>
                {
                    policy.RequireClaim(ClaimTypes.Role, IdentityConstants.GlobalAdministrator, IdentityConstants.Administrator);
                    policy.RequireRole(IdentityConstants.GlobalAdministrator, IdentityConstants.Administrator);
                });

                options.AddPolicy(IdentityConstants.TeacherRightsPolicyName, policy =>
                {
                    policy.RequireClaim(ClaimTypes.Role, IdentityConstants.GlobalAdministrator, IdentityConstants.Administrator, IdentityConstants.Teacher);
                    policy.RequireRole(IdentityConstants.GlobalAdministrator, IdentityConstants.Administrator, IdentityConstants.Teacher);
                });

                options.AddPolicy(IdentityConstants.FatherRightsPolicyName, policy =>
                {
                    policy.RequireClaim(ClaimTypes.Role, IdentityConstants.GlobalAdministrator, IdentityConstants.Administrator, IdentityConstants.Teacher, IdentityConstants.Father);
                    policy.RequireRole(IdentityConstants.GlobalAdministrator, IdentityConstants.Administrator, IdentityConstants.Teacher, IdentityConstants.Father);
                });

                options.AddPolicy(IdentityConstants.ChildRightsPolicyName, policy =>
                {
                    policy.RequireClaim(ClaimTypes.Role, IdentityConstants.GlobalAdministrator, IdentityConstants.Administrator, IdentityConstants.Teacher, IdentityConstants.Father, IdentityConstants.Child);
                    policy.RequireRole(IdentityConstants.GlobalAdministrator, IdentityConstants.Administrator, IdentityConstants.Teacher, IdentityConstants.Father, IdentityConstants.Child);
                });

            });

            services.AddTransient<IRequestService, HttpClientRequestService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IAccountManager, AccountManager>();
            services.AddTransient<ICustomerService, CustomerService>();

            services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, UserClaimsPrincipalFactory<IdentityUser>>();


            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(ApiVersion, new OpenApiInfo
                {
                    Version = ApiVersion,
                    Title = "Recreo Identity Service",
                    Description = "Manages Recreo Identity Service ASP.NET Core Web API",
                    Contact = new OpenApiContact
                    {
                        Name = "IUSACOMM 2019",
                        Email = string.Empty
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Copyright (c) 2020 RECREO. All rights reserved."
                    }
                });
                c.DescribeAllEnumsAsStrings();

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                var securitySchema = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\", provide value: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securitySchema);

                var securityRequirement = new OpenApiSecurityRequirement();
                securityRequirement.Add(securitySchema, new[] { "Bearer" });
                c.AddSecurityRequirement(securityRequirement);
            });




        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Let Tank's Middleware handle exceptions
            app.UseMiddlewareExceptionHandling();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/swagger/{ApiVersion}/swagger.json", $"Tank Identity Service {ApiVersion}");

                options.RoutePrefix = string.Empty;
            });

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
