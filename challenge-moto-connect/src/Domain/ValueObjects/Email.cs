using System.Text.RegularExpressions;

namespace challenge_moto_connect.Domain.ValueObjects
{
    public class Email
    {
        public string Address { get; private set; }

        public Email(string address)
        {
            if (!IsValidEmail(address))
                throw new ArgumentException("Email inválido.");
            Address = address;
        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        public override string ToString()
        {
            return Address;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Email other = (Email)obj;
            return Address == other.Address;
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }

        public static implicit operator string(Email email)
        {
            return email.Address;
        }

        public static implicit operator Email(string address)
        {
            return new Email(address);
        }
    }
}


