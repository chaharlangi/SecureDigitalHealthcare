using System.Security.Claims;

namespace SecureDigitalHealthcare.Utilities
{
    public static class AppRole
    {
        public const string Admin = "Admin";
        public const string Doctor = "Doctor";
        public const string Patient = "Patient";

        public static string GetRoleName(int roleId)
        {
            return roleId switch
            {
                1 => Admin,
                2 => Doctor,
                3 => Patient,
                _ => string.Empty
            };
        }
        public static int GetRoleId(string roleName)
        {
            return roleName switch
            {
                Admin => 1,
                Doctor => 2,
                Patient => 3,
                _ => 0
            };
        }

    }
}
