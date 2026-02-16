using LogKt01.Components;
using LogKt01.Data;
using LogKt01.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace LogKt01;

public class Program
{
	public static async Task Main(string[] args)
	{
		Log.Logger = new LoggerConfiguration()
			.WriteTo.Console()
			.CreateBootstrapLogger();

		try
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Host.UseSerilog((context, services, loggerConfiguration) =>
			{
				loggerConfiguration
					.ReadFrom.Configuration(context.Configuration)
					.ReadFrom.Services(services)
					.Enrich.FromLogContext();
			});

			builder.Services.AddRazorComponents()
				.AddInteractiveServerComponents();

			builder.Services.AddDbContext<AppDbContext>(options =>
			{
				options.UseSqlite(builder.Configuration.GetConnectionString("AppDbContext") ??
				                  throw new InvalidOperationException("Connection string 'AppDbContext' not found."));
			});

			builder.Services.AddScoped<TaskManagerService>();

			var app = builder.Build();

			app.UseSerilogRequestLogging(options =>
			{
				options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
				options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
				{
					diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
					diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
					diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
					diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
				};
			});

			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Error");
				app.UseHsts();
			}
			else
			{
				using var scope = app.Services.CreateScope();
				var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

				await dbContext.Database.EnsureCreatedAsync();
			}

			app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
			app.UseHttpsRedirection();

			app.UseAntiforgery();

			app.MapStaticAssets();
			app.MapRazorComponents<App>()
				.AddInteractiveServerRenderMode();

			await app.RunAsync();
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, "Application terminated unexpectedly");
		}
		finally
		{
			await Log.CloseAndFlushAsync();
		}
	}
}
