using DNTCaptcha.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SecureDigitalHealthcare.Constants;
using SecureDigitalHealthcare.Models;
using System.Text;
using SecureDigitalHealthcare.Models;
using SecureDigitalHealthcare.Utilities;
using Microsoft.AspNetCore.Mvc.Infrastructure;

string question = "Rm9yIHRoaXMgYXNzaWdubWVudCB5b3Ugd2lsbCBoYXZlIHR3byB0YXNrcyBvbiB0aGUgYmFja2VuZDoKLSB1cGxvYWQgYSBmaWxlIHJlY2VpdmVkIHRocm91Z2ggYSBQT1NUIHJlcXVlc3QgYW5kIHN0b3JlIGl0IGxvY2FsbHkgZW5jcnlwdGVkIHdpdGggQUVTMjU2Ci0gaGFzaCB0aGUgcGFzc3dvcmQgcmVjZWl2ZWQgYnkgUE9TVCBwYXJhbWV0ZXIgd2l0aCBiY3J5cHQgYW5kIHNhdmUgaXQgbG9jYWxseSBpbiBhIEpTT04gZmlsZQ==";
//Console.WriteLine(ROT13Cipher.Decrypt(question));

// Base64
byte[] base64Decoded = BaseConverter.Base64Decode(question);
Console.WriteLine("Base64 Decoded: " + Encoding.UTF8.GetString(base64Decoded));
/*
 * For this assignment you will have two tasks on the backend:
- upload a file received through a POST request and store it locally encrypted with AES256
- hash the password received by POST parameter with bcrypt and save it locally in a JSON file
 */
///








//Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
//Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    //builder.Services.AddDbContext<SecureDigitalHealthcareContext>(options =>
    //    options.UseSqlServer(builder.Configuration.GetConnectionString("SecureDigitalHealthcareContextDevelopment")
    //        ?? throw new InvalidOperationException("Connection string 'SecureDigitalHealthcareContextDevelopment' not found.")));


    builder.Services.AddDbContext<EasyHealthContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("EasyHealthDevelopment")
            ?? throw new InvalidOperationException("Connection string 'EasyHealthDevelopment' not found.")));

}
else
{
    //builder.Services.AddDbContext<SecureDigitalHealthcareContext>(options =>
    //    options.UseSqlServer(builder.Configuration.GetConnectionString("SecureDigitalHealthcareContextProduction")
    //        ?? throw new InvalidOperationException("Connection string 'SecureDigitalHealthcareContextProduction' not found.")));
}


// Add services to the container.
builder.Services.AddControllersWithViews();

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

    // Don't set this line (remove it) to use the installed system'question fonts (FontName = "Tahoma").
    // Or if you want to use a custom font, make sure that font is present in the wwwroot/fonts folder and also use a good and complete font!
    //.UseCustomFont(Path.Combine(_env.WebRootPath, "fonts", "IRANSans(FaNum)_Bold.ttf")) // This is optional.
    .AbsoluteExpiration(minutes: 7)
    .RateLimiterPermitLimit(10) // for .NET 7x+, Also you need to call app.UseRateLimiter() after calling app.UseRouting().
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
        policy.RequireClaim(ClaimConstants.AdminId);
    });
    options.AddPolicy(PolicyConstants.MustBeDoctor, policy =>
    {
        policy.RequireClaim(ClaimConstants.DoctorId);
    });
    options.AddPolicy(PolicyConstants.MustBePatient, policy =>
    {
        policy.RequireClaim(ClaimConstants.PatientId);
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

//builder.Services.AddAuthentication("Bearer")
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateIssuerSigningKey = true,
//            //ValidateLifetime = true,
//            ValidIssuer = builder.Configuration.GetValue<string>("Authentication:Issuer"),
//            ValidAudience = builder.Configuration.GetValue<string>("Authentication:Audience"),
//            IssuerSigningKey = new SymmetricSecurityKey(
//                               Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("Authentication:SecretKey")!))
//        };
//    });

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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();

