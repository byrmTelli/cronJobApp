using cronJobApp.Jobs;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Hangfire to use PostgreSQL with new method
builder.Services.AddHangfire(config => config
    .UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("PostgresConn"));
        // Diğer PostgreSQL yapılandırma seçeneklerini burada belirleyebilirsiniz
    }));
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseHangfireDashboard();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Hangfire işlerini burada tanımlayın
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // Dakikada bir çalışacak işi burada tanımlayın
    recurringJobManager.AddOrUpdate<Jobs>(
        "her-dk-bildirim", 
        job => job.ConsoleInfo(),
        Cron.Minutely
    );
}

app.Run();
