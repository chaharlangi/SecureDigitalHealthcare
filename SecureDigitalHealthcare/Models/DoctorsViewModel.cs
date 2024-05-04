namespace SecureDigitalHealthcare.Models
{
    public class DoctorsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Speciality { get; set; }
        public string ProfilePicturePath { get; set; }

        public static List<DoctorsViewModel> GetListsByDoctors(List<Doctor> doctors)
        {
            List<DoctorsViewModel> doctorsList = new();
            foreach (var doctor in doctors)
            {
                doctorsList.Add(new DoctorsViewModel
                {
                    Id = doctor.Id,
                    Name = doctor.IdNavigation.Name,
                    LastName = doctor.IdNavigation.LastName,
                    Speciality = doctor.Speciality.Name,
                    ProfilePicturePath = doctor.IdNavigation.ProfileImagePath
                });
            }

            return doctorsList;
        }
    }
}
