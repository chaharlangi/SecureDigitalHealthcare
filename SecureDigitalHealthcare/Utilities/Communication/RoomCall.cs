using Azure.Communication.Identity;
using Azure.Communication;
using Azure.Communication.Rooms;
using Microsoft.AspNetCore.Mvc;
using SecureDigitalHealthcare.Models;
using Azure;
using Microsoft.Identity.Client;

namespace SecureDigitalHealthcare.Utilities.Communication;

public static class RoomCall
{
    //private static readonly string connectionString = "https://easyhealthcommunicationservice.europe.communication.azure.com/";
    private static readonly string connectionString = "endpoint=https://easyhealthcommunicationservice.europe.communication.azure.com/;accesskey=i3cI3sXbr/agwxQs1lvJTq9zMpwHVRT7x1TaOM3EghKXj3hspvDj6hMzy59MI9ap7omCkQLws2cZlTFVY1tBjw==";

    private static readonly RoomsClient _roomsClient = new RoomsClient(connectionString);
    private static readonly CommunicationIdentityClient _identityClient = new CommunicationIdentityClient(connectionString);

    public record RoomData()
    {
        public string? RoomId { get;  init; }
        public string? HostId { get;  init; }
        public string? GuestId { get;  init; }
        public DateTimeOffset? ValidFrom { get; init; }
        public DateTimeOffset? ValidUntil { get; init; }
        public int? DurationInMinutes { get; init; }
    }

    public static async Task<RoomData> CreateRoomAsync(int durationValidMinutes)
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        CommunicationUserIdentifier hostUser = _identityClient.CreateUser();
        CommunicationUserIdentifier guestUser = _identityClient.CreateUser();

        List<RoomParticipant> participants = new List<RoomParticipant>()
        {
                new RoomParticipant(hostUser)
                {
                    Role = ParticipantRole.Presenter
                },
                new RoomParticipant(guestUser)
                {
                    Role = ParticipantRole.Attendee
                }
        };

        DateTimeOffset validFrom = DateTimeOffset.UtcNow;
        DateTimeOffset validUntil = validFrom.AddMinutes(durationValidMinutes);

        bool pstnDialOutEnabled = false;
        CreateRoomOptions createRoomOptions = new CreateRoomOptions()
        {
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            PstnDialOutEnabled = pstnDialOutEnabled,
            Participants = participants
        };

        CommunicationRoom createdRoom = await _roomsClient.CreateRoomAsync(createRoomOptions, cancellationToken);

        RoomData roomData = new RoomData()
        {
            RoomId = createdRoom.Id,
            HostId = hostUser.RawId,
            GuestId = guestUser.RawId,
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            DurationInMinutes = durationValidMinutes
        };

        AppDebug.Log($"\nCreated room (id: {createdRoom.Id})" +
                     $"\nhost       (id: {hostUser.RawId}" +
                     $"\nguest      (id: {guestUser.RawId}" +
                     $"\nexpires at {validUntil.ToString("HH:mm:ss dd-MM-yyyy")}" +
                     $"\n\n");

        return roomData;
    }
    public static async Task<CommunicationRoom> GetRoomByIdAsync(string roomId)
    {
        CommunicationRoom room = await _roomsClient.GetRoomAsync(roomId);
        AppDebug.Log($"Got room with id: {room.Id}");

        return room;
    }
    public static async Task<CommunicationRoom> UpdateLifeTimeAsync(string roomId, DateTimeOffset validFrom, DateTimeOffset validUntil)
    {
        if (validFrom >= validUntil)
            throw new ArgumentException("validFrom must be less than validUntil");

        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        UpdateRoomOptions updateRoomOptions = new UpdateRoomOptions()
        {
            ValidFrom = validFrom,
            ValidUntil = validUntil
        };

        CommunicationRoom updatedRoom = await _roomsClient.UpdateRoomAsync(roomId, updateRoomOptions, cancellationToken);

        AppDebug.Log($"\nUpdated room (id: {updatedRoom.Id}) with:" +
                     $"\nvalid from: {updatedRoom.ValidFrom.ToString("HH:mm:ss dd-MM-yyyy")}" +
                     $"\nvalid till: {updatedRoom.ValidUntil.ToString("HH:mm:ss dd-MM-yyyy")}" +
                     $"\n\n");

        return updatedRoom;
    }
    public static async Task<IEnumerable<CommunicationRoom>> GetRoomsAsync()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        AsyncPageable<CommunicationRoom> allRooms = _roomsClient.GetRoomsAsync(cancellationToken);
        List<CommunicationRoom> rooms = new List<CommunicationRoom>();

        string message = "\nAll active rooms:";
        await foreach (CommunicationRoom room in allRooms)
        {
            rooms.Add(room);
            message += $"\nRoom (id: {room.Id})" +
                       $"\ncreated at: {room.ValidFrom.ToString("HH:mm:ss dd-MM-yyyy")}" +
                       $"\nvalid from: {room.ValidFrom.ToString("HH:mm:ss dd-MM-yyyy")}" +
                       $"\nvalid till: {room.ValidUntil.ToString("HH:mm:ss dd-MM-yyyy")}";
            message += "\n\n";
        }

        AppDebug.Log(message);

        return rooms;
    }
    public static async Task<RoomParticipant> AddNewParticipantAsync(string roomId, ParticipantRole participantRole)
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        List<RoomParticipant> roomParticipants = new List<RoomParticipant>();

        CommunicationUserIdentifier newParticipantIdentity = _identityClient.CreateUser();

        RoomParticipant newParticipant = new RoomParticipant(newParticipantIdentity) { Role = participantRole };
        roomParticipants.Add(newParticipant);

        Response response = await _roomsClient.AddOrUpdateParticipantsAsync(roomId, roomParticipants, cancellationToken);

        AppDebug.Log("\nAdded or updated participants to room");

        return newParticipant;
    }
    public static async Task<IEnumerable<RoomParticipant>> GetParticipantsAsync(string roomId)
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        AsyncPageable<RoomParticipant> allParticipants = _roomsClient.GetParticipantsAsync(roomId, cancellationToken);
        List<RoomParticipant> participants = new List<RoomParticipant>();

        string message = $"\nParticipants in room (id: {roomId}):";
        await foreach (RoomParticipant participant in allParticipants)
        {
            participants.Add(participant);
            message += $"\n=> (id: {participant.CommunicationIdentifier.RawId})";
        }
        AppDebug.Log(message);

        return participants;
    }
    public static async Task<bool> RemoveParticipantAsync(string roomId, params string[] participantsId)
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        List<CommunicationIdentifier> participantsToRemove = new();
        string message = $"\nParticipants removed from room (id: {roomId}):";
        foreach (string id in participantsId)
        {
            message += $"\n=> (id: {id})";
            participantsToRemove.Add(new CommunicationUserIdentifier(id));
        }
        Response removeParticipantsResponse = await _roomsClient.RemoveParticipantsAsync(roomId, participantsToRemove, cancellationToken);

        message += "\n\n";
        AppDebug.Log($"{message}");

        return true;
    }
    public static async Task<bool> DeleteRoom(string roomId)
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        Response deleteRoomResponse = await _roomsClient.DeleteRoomAsync(roomId, cancellationToken);

        AppDebug.Log($"\nDeleted room (id: {roomId})   success: {!deleteRoomResponse.IsError}");

        return true;
    }
}

