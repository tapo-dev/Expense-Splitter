using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RoommateApp.Core.Data;
using RoommateApp.Core.Factories;
using RoommateApp.Core.Services;
using RoommateApp.Maui.ViewModels;
using RoommateApp.Maui.Views;

namespace RoommateApp.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts => {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "roommate.db");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Registrace služeb
            builder.Services.AddSingleton<SpravceUctu>();
            builder.Services.AddSingleton<DataSeeder>();
            builder.Services.AddSingleton<INotifierFactory, NotifierFactory>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<SkupinaService>();

            // ViewModels
            builder.Services.AddTransient<LoginPageViewModel>();
            builder.Services.AddTransient<RegisterPageViewModel>();
            builder.Services.AddTransient<GroupsPageViewModel>();

            // Registrace stránek
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<GroupsPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<AddExpensePage>();
            builder.Services.AddTransient<EditProfilePage>();
            builder.Services.AddTransient<CreateGroupPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}