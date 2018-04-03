using System.Collections.Generic;
using System.IO;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using TodoApi.Models;
using TodoApi.Services;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace TodoApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddMemoryCache()
				.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"))
				.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"))
				.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>()
				.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>()
				.AddScoped<ListService>()
				.AddScoped<ItemService>()
				.AddDbContext<TodoContext>(options => options.UseSqlServer(Configuration.GetConnectionString("sqlserver")))
				.AddSingleton<SignalRConnection>()
				.AddSingleton<IHostedService>(ctx => ctx.GetRequiredService<SignalRConnection>());

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.Authority = Configuration.GetSection("IdentityServer").GetValue<string>("Url");
					options.Audience = "api1";
					options.RequireHttpsMetadata = false;
				});

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info()
				{
					Title = ".NET Summit Todo Api",
					Version = "v1",
				});
				c.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "TodoApi.xml"));
				c.AddSecurityDefinition("oauth2", new OAuth2Scheme()
				{
					Type = "oauth2",
					Flow = "password",
					TokenUrl = $"{Configuration.GetSection("IdentityServer").GetValue<string>("Url")}/connect/token",
					Scopes = new Dictionary<string, string>()
					{
						{ "api1", "Access the Todo APi" },
					},
				});
				c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>()
				{
					{
						"oauth2", new [] { "api1" }
					},
				});
			});

			services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				scope.ServiceProvider.GetRequiredService<TodoContext>().Database.Migrate();
			}

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseIpRateLimiting();
			app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials());
			app.UseAuthentication();
			app.UseMvc();
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", ".NET Summit Todo API v1");
				c.OAuthClientId(Configuration.GetSection("IdentityServer").GetValue<string>("SwaggerClientId"));
				c.OAuthClientSecret(Configuration.GetSection("IdentityServer").GetValue<string>("SwaggerClientSecret"));
				c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
			});
		}
	}
}