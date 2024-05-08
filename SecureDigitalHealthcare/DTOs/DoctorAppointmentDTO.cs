using SecureDigitalHealthcare.Models;

namespace SecureDigitalHealthcare.DTOs;

public record DoctorAppointmentDTO
{
    public DoctorAppointmentDTO(Doctor doctor)
    {
        DoctorId = doctor.Id;
        DoctorName = doctor.IdNavigation.Name!;
        DoctorLastName = doctor.IdNavigation.LastName!;
        Speciality = doctor.Speciality!.Name!;
        ProfilePicturePath = doctor.IdNavigation.ProfileImagePath!;
        Availabilities = doctor.Availabilities.ToList();
        DoctorsComment = doctor.IdNavigation.CommentReceivers.ToList();
    }

    public int DoctorId { get; init; }
    public string DoctorName { get; init; }
    public string DoctorLastName { get; init; }
    public string Speciality { get; init; }
    public string ProfilePicturePath { get; init; }
    public List<Availability> Availabilities { get; init; } = new List<Availability>();
    public List<Comment> DoctorsComment { get; init; } = new List<Comment>();
}


