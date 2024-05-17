using Azure.Communication.Identity;
using Azure.Communication;
using Azure.Communication.Rooms;
using Microsoft.AspNetCore.Mvc;
using SecureDigitalHealthcare.Models;

namespace SecureDigitalHealthcare.Controllers
{
    public class VideoCallController_cs : Controller
    {
        private readonly EasyHealthContext _context;
        private readonly IWebHostEnvironment _environment;

        //private static readonly string connectionString = "https://easyhealthcommunicationservice.europe.communication.azure.com/";
        private static readonly string connectionString = "endpoint=https://easyhealthcommunicationservice.europe.communication.azure.com/;accesskey=i3cI3sXbr/agwxQs1lvJTq9zMpwHVRT7x1TaOM3EghKXj3hspvDj6hMzy59MI9ap7omCkQLws2cZlTFVY1tBjw==";

        public VideoCallController_cs(EasyHealthContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<string> CreateRoom()
        {
            // Find your Communication Services resource in the Azure portal
            RoomsClient roomsClient = new RoomsClient(connectionString);

            // Create identities for users who will join the room
            CommunicationIdentityClient identityClient = new CommunicationIdentityClient(connectionString);
            CommunicationUserIdentifier user1 = identityClient.CreateUser();
            CommunicationUserIdentifier user2 = identityClient.CreateUser();

            List<RoomParticipant> participants = new List<RoomParticipant>()
            {
                new RoomParticipant(user1) { Role = ParticipantRole.Presenter },
                new RoomParticipant(user2) // The default participant role is ParticipantRole.Attendee
            };

            // Create a room
            DateTimeOffset validFrom = DateTimeOffset.UtcNow;
            DateTimeOffset validUntil = validFrom.AddDays(1);
            CancellationToken cancellationToken = new CancellationTokenSource().Token;

            CommunicationRoom createdRoom = await roomsClient.CreateRoomAsync(validFrom, validUntil, participants, cancellationToken);

            // CreateRoom or CreateRoomAsync methods can take CreateRoomOptions type as an input parameter.
            bool pstnDialOutEnabled = false;
            CreateRoomOptions createRoomOptions = new CreateRoomOptions()
            {
                ValidFrom = validFrom,
                ValidUntil = validUntil,
                PstnDialOutEnabled = pstnDialOutEnabled,
                Participants = participants
            };

            createdRoom = await roomsClient.CreateRoomAsync(createRoomOptions, cancellationToken);
            string roomId = createdRoom.Id;
            Console.WriteLine("\nCreated room with id: " + roomId);

            return roomId;
        }
    }
}
