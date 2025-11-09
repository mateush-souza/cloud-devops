using System.Text.RegularExpressions;

namespace challenge_moto_connect.Domain.ValueObjects
{
    public class Password
    {
        public string Value { get; private set; }

        public Password(string value)
        {
            if (!IsValidPassword(value))
                throw new ArgumentException("Senha inválida. A senha deve conter pelo menos 8 caracteres, incluindo letra maiúscula, minúscula, número e caractere especial.");
            Value = value;
        }

        private bool IsValidPassword(string password)
        {
            string pattern = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
            return Regex.IsMatch(password, pattern);
        }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Password other = (Password)obj;
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator string(Password password)
        {
            return password.Value;
        }

        public static implicit operator Password(string value)
        {
            return new Password(value);
        }
    }
}


