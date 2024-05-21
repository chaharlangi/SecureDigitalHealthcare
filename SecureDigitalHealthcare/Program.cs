using DNTCaptcha.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SecureDigitalHealthcare.Constants;
using SecureDigitalHealthcare.Models;
using SecureDigitalHealthcare.Utilities;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Security.Claims;
using Azure.Communication;
using Azure.Communication.Identity;
using Azure.Communication.Rooms;
using Microsoft.CodeAnalysis.Elfie.Model;
using SecureDigitalHealthcare.Utilities.Communication;
using Microsoft.Extensions.Hosting;
using SecureDigitalHealthcare.Controllers;
using System.Configuration;


//var room = await RoomCallManager.CreateRoomAsync();
//AppDebug.Log(room.ToString());


//await RoomCallManager.DeleteRoom(" 99548897045739884");
var rooms = await RoomCallManager.GetRoomsAsync();
//foreach (var item in rooms)
//{
//    await RoomCallManager.DeleteRoom(item.Id);
//}
//rooms = await RoomCallManager.GetRoomsAsync();

//Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
//Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddScoped(typeof(AppController<>));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<EasyHealthContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("EasyHealthDevelopment")
            ?? throw new InvalidOperationException("Connection string 'EasyHealthDevelopment' not found."));
        options.EnableSensitiveDataLogging(true);
    });

}
else
{
    builder.Services.AddDbContext<EasyHealthContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("EasyHealthProduction")
            ?? throw new InvalidOperationException("Connection string 'EasyHealthProduction' not found.")));
}


// Add services To the container.
builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNodeApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:8080")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});


builder.Services.AddAntiforgery(options =>
{
    // Set Cookie properties using CookieBuilder properties†.
    options.FormFieldName = "AntiforgeryFieldname";
    options.HeaderName = "X-CSRF-TOKEN-HEADERNAME";
    options.SuppressXFrameOptionsHeader = false;
});

builder.Services.AddDNTCaptcha(options =>
{
    // options.UseSessionStorageProvider() // -> It doesn't rely on the server or client'question times. Also it'question the safest one.
    // options.UseMemoryCacheStorageProvider() // -> It relies on the server'question times. It'question safer than the CookieStorageProvider.
    options.UseCookieStorageProvider(SameSiteMode.Strict) // -> It relies on the server and client'question times. It'question ideal for scalability, because it doesn't save anything in the server'question memory.
                                                          // .UseDistributedCacheStorageProvider() // --> It'question ideal for scalability using `services.AddStackExchangeRedisCache()` for instance.
                                                          // .UseDistributedSerializationProvider()

    // Don't set this line (remove it) To use the installed system'question fonts (FontName = "Tahoma").
    // Or if you want To use a custom font, make sure that font is present in the wwwroot/fonts folder and also use a good and complete font!
    //.UseCustomFont(Path.Combine(_env.WebRootPath, "fonts", "IRANSans(FaNum)_Bold.ttf")) // This is optional.
    .AbsoluteExpiration(minutes: 7)
    .RateLimiterPermitLimit(10) // for .NET 7x+, Also you need To call app.UseRateLimiter() after calling app.UseRouting().
    .ShowThousandsSeparators(false)
    .WithNoise(0.015f, 0.015f, 1, 0.0f)
    .WithEncryptionKey("This is my secure key!")
    .WithNonceKey("NETESCAPADES_NONCE")
    //.WithCaptchaImageControllerRouteTemplate("my-custom-captcha/[action]")
    //.WithCaptchaImageControllerNameTemplate("my-custom-captcha")
    .InputNames(// This is optional. Change it if you don't like the default names.
        new DNTCaptchaComponent
        {
            CaptchaHiddenInputName = "DNTCaptchaText",
            CaptchaHiddenTokenName = "DNTCaptchaToken",
            CaptchaInputName = "DNTCaptchaInputText"
        })
    .Identifier("dntCaptcha")// This is optional. Change it if you don't like its default name.
    ;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyConstants.MustBeAdmin, policy =>
    {
        policy.RequireRole(AppRole.Admin);
    });
    options.AddPolicy(PolicyConstants.MustBeDoctor, policy =>
    {
        policy.RequireRole(AppRole.Doctor);
    });
    options.AddPolicy(PolicyConstants.MustBePatient, policy =>
    {
        policy.RequireRole(AppRole.Patient);
    });
    options.AddPolicy(PolicyConstants.MustBeDoctorOrPatient, policy =>
    {
        policy.RequireRole(AppRole.Patient, AppRole.Doctor);
    });

    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
    .Build();

});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).
    AddCookie(options =>
    {
        options.Cookie.Name = "TokenLoginCookie";
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.AccessDeniedPath = "/Account/AccessDenied";
    });


var app = builder.Build();

app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want To change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

//
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
//               Path.Combine(Directory.GetCurrentDirectory(), "MyRoomCallPlugin", "node_modules")),
//    RequestPath = new PathString("/vendor")
//});
//

app.UseRouting();

app.UseCors("AllowNodeApp");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();



//while (true)
//{
//    Console.WriteLine("1. Create Room: ");
//    Console.WriteLine("2. Delete Room: ");
//    Console.WriteLine("3. Get Room: ");
//    Console.WriteLine("4. Get Rooms: ");
//    Console.WriteLine("5. Add Participants: ");
//    Console.WriteLine("6. Get Participants: ");
//    Console.WriteLine("7. Remove Participants: ");

//    Console.Write("\nInput Operation: ");
//    Console.WriteLine();

//    var input = Console.ReadLine();
//    if (input is null)
//        input = "0";

//    RoomCallManager roomCall = new RoomCallManager();
//    int operation = int.Parse(input);

//    switch (operation)
//    {
//        case 1:
//            Console.WriteLine("input host id");
//            var hostId = Console.ReadLine();
//            Console.WriteLine("input guest id");
//            var guestId = Console.ReadLine();
//            Console.WriteLine("input remain valid minutes");
//            var remainValidMinutes = int.Parse(Console.ReadLine());
//            var room = roomCall.CreateRoomAsync(remainValidMinutes, hostId, guestId);

//            Console.WriteLine("Input any to continue...");
//            Console.ReadLine();
//            break;
//        case 2:

//            break;
//        case 3:

//            break;
//        case 4:

//            var rooms = roomCall.GetRoomsAsync();

//            Console.WriteLine("Input any to continue...");
//            Console.ReadLine();
//            break;
//        case 5:
//            Environment.Exit(0);
//            break;
//        default:
//            Console.Clear();
//            break;
//    }
//}
