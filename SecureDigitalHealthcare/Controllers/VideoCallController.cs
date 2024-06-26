﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using SecureDigitalHealthcare.DTOs;
using SecureDigitalHealthcare.Models;
using SecureDigitalHealthcare.Utilities;
using System.Configuration;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SecureDigitalHealthcare.Controllers
{
    [AllowAnonymous]
    public class VideoCallController : Controller
    {
        public static readonly string VideoCallUrl = "https://EasyHealthCallRoom.azurewebsites.net";

        private readonly EasyHealthContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        public VideoCallController(EasyHealthContext context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _environment = environment;
        }
        public static bool IsRunningOnAzureAppService()
        {
            string websiteInstanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
            return !string.IsNullOrEmpty(websiteInstanceId);
        }
        public static string AzureWebsiteName()
        {
            string websiteSiteName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
            return websiteSiteName;
        }
        public static string GetVideoCallUrl()
        {
            return IsRunningOnAzureAppService() ? VideoCallUrl : "http://localhost:8080/";
        }

        public IActionResult Index(string encryptedUserAccessToken, string ivBase64)
        {

            if (string.IsNullOrEmpty(encryptedUserAccessToken))
            {
                return NotFound("User access token is not valid. Check your input");
            }


            AppDebug.Log($"Encrypted ({encryptedUserAccessToken.Length}) {encryptedUserAccessToken}\nIV Base64 ({ivBase64.Length}) {ivBase64}");
            AppDebug.Log($"Decrypted {AppEncryptor.DecryptString(encryptedUserAccessToken, ivBase64)}");


            var userAccessToken = AppEncryptor.DecryptString(encryptedUserAccessToken, ivBase64);
            var rooms = _context.RoomCalls;
            var room = rooms.FirstOrDefault(r => r.HostAccessToken == userAccessToken || r.GuestAccessToken == userAccessToken);

            if (room == null)
            {
                return NotFound("Room not found");
            }


            return Ok(new { RoomId = room.Id });
        }
    }
}
