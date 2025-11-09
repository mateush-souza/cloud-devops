using challenge_moto_connect.Domain.ValueObjects;

namespace challenge_moto_connect.Domain.Entity
{
    public class User
    {

        public Guid UserID { get; set; }
        public string Name { get; set; }
        public Email Email { get; private set; }
        public Password Password { get; private set; }
        public UserType Type { get; set; }

        public User() { }

        public User(Guid id, string name, Email email, Password password, UserType type)
        {
            UserID = id;
            Name = name;
            Email = email;
            Password = password;
            Type = type;

        }

        public void UpdateEmail(Email newEmail)
        {
            Email = newEmail;
        }

        public void UpdatePassword(Password newPassword)
        {
            Password = newPassword;
        }
    }
}


