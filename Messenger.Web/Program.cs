using Messenger.Application.Mappings;
using Messenger.Application.MessageProcessing.handlers;
using Messenger.Application.MessageProcessing.interfaces;
using Messenger.Application.Services.Factories;
using Messenger.Application.Services.Implementations;
using Messenger.Application.Services.Interfaces;
using Messenger.CrossCutting.Services;
using Messenger.Domain.Repositories;
using Messenger.Infrastructure.Persistence;
using Messenger.Infrastructure.Repositories;
using Messenger.Infrastructure.Services;
using Messenger.Web.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 15 * 1024 * 1024;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR();

builder.Services.AddTransient<ITextMessageHandler, TextMessageHandler>();
builder.Services.AddTransient<IFileMessageHandler, FileMessageHandler>();

builder.Services.AddScoped<IChatFactory, ChatFactory>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

builder.Services.AddSingleton<IOnlineUserTracker, OnlineUserTracker>();



builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.Cookie.Name = "AuthCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();



var app = builder.Build();

var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".pdf", ".docx", ".mp4", ".mp3" };
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/uploads"))
    {
        var ext = Path.GetExtension(ctx.Request.Path.Value ?? "").ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Response.WriteAsync($"Downloading files of type '{ext}' is not allowed.");
            return;
        }
    }
    await next();
});

app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Chats}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<ChatListHub>("/hubs/chatList");
app.MapHub<ProfileHub>("/hubs/profile");
app.MapHub<UserStateHub>("/hubs/userState").RequireAuthorization();

app.Run();
