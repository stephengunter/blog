using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationCore.DataAccess;
using ApplicationCore.Models;
using ApplicationCore.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NLog;
using ApplicationCore.Authorization;
using ApplicationCore.Middlewares;
using Microsoft.Extensions.Hosting;
using ApplicationCore.DI;
using ApplicationCore;
using Microsoft.AspNetCore.Http;
using Hangfire;

namespace Web
{
	public class Startup
	{
		
		public Startup(IConfiguration configuration)
		{
			var nLogConfigPath = string.Concat(Directory.GetCurrentDirectory(), "/nlog.config");
			if (File.Exists(nLogConfigPath)) { LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config")); }
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		string ClientUrl => Configuration["AppSettings:ClientUrl"];

		string AdminUrl => Configuration["AppSettings:AdminUrl"];

		// This method gets called by the runtime. Use this method to add services to the container.
		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<DefaultContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("Default"),
				b => b.MigrationsAssembly("ApplicationCore"))
			);

			services.AddIdentity<User, IdentityRole>(options =>
			{
				options.User.RequireUniqueEmail = true;

			})
			.AddEntityFrameworkStores<DefaultContext>()
			.AddDefaultTokenProviders();

			services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
			services.Configure<AuthSettings>(Configuration.GetSection("AuthSettings"));
			services.Configure<AdminSettings>(Configuration.GetSection("AdminSettings"));


			services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("Default")));
			services.AddHangfireServer();


			var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["AuthSettings:SecurityKey"]));

			string issuer = Configuration["AppSettings:Name"];
			string audience = ClientUrl;
			string securityKey = Configuration["AuthSettings:SecurityKey"];
			int tokenValidHours = Convert.ToInt32(Configuration["AuthSettings:TokenValidHours"]);
			services.AddJwtBearer(tokenValidHours, issuer, audience, securityKey);

			services.AddSwagger("Blog Starter", "v1");

			services.AddDtoMapper();

			services.AddAuthorization(options =>
			{
				options.AddPolicy("Admin", policy =>
					policy.Requirements.Add(new HasPermissionRequirement(Permissions.Admin)));
			});


			services.AddCors(options =>
			{
				options.AddPolicy("Api",
				builder =>
				{
					builder.WithOrigins(ClientUrl)
							.AllowAnyHeader()
							.AllowAnyMethod().AllowCredentials();
				});

				options.AddPolicy("Admin",
				builder =>
				{
					builder.WithOrigins(AdminUrl)
							.AllowAnyHeader()
							.AllowAnyMethod();
				});

				options.AddPolicy("Global",
				builder =>
				{
					builder.WithOrigins(ClientUrl, AdminUrl)
							.AllowAnyHeader()
							.AllowAnyMethod();
				});
			});

			services.AddControllers();

			services.AddSignalR();


			services.AddHttpClient(ApplicationCore.Consts.Common.Google, c =>
			{
				c.BaseAddress = new Uri("https://www.google.com");
			});

			
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			return AutofacRegister.Register(services);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			//是否過濾IP
			//app.UseMiddleware<IPListMiddleware>(Configuration["IPList"]);

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseMiddleware<ExceptionMiddleware>();
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseHangfireDashboard();


			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog");
			});
			app.UseSwagger();

			app.UseAuthentication();

			app.UseRouting();

			app.UseCors();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

		}
	}
}
