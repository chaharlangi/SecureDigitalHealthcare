using System.Numerics;

namespace SecureDigitalHealthcare.Models
{
    public class BookAppointmentViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorLastName { get; set; }
        public string Speciality { get; set; }
        public string ProfilePicturePath { get; set; }
        public List<Availability> availabilities { get; set; } = new List<Availability>();
        public List<Comment> DoctorsComment { get; set; } = new List<Comment>();

        public static BookAppointmentViewModel GetModelByDoctor(Doctor doctor)
        {
            //Print console.wrtieline if any fields are null
            

            BookAppointmentViewModel bookAppointmentViewModel = new BookAppointmentViewModel()
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.IdNavigation.Name,
                DoctorLastName = doctor.IdNavigation.LastName,
                Speciality = doctor.Speciality.Name,
                ProfilePicturePath = doctor.IdNavigation.ProfileImagePath,
                availabilities = doctor.Availabilities.ToList(),
                DoctorsComment = doctor.IdNavigation.CommentReceivers.ToList()
            };
            return bookAppointmentViewModel;
        }
    }
}
