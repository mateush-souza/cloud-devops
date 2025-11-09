using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace challenge_moto_connect.Domain.ValueObjects
{
    public class Password
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;
        // Senha mais flexível: mínimo 6 caracteres, pelo menos uma letra e um número
        private static readonly Regex ComplexityPattern = new Regex("^(?=.*[A-Za-z])(?=.*\\d).{6,}$", RegexOptions.Compiled);

        public string Value { get; private set; }

        private Password(string value)
        {
            Value = value;
        }

        public static Password FromPlainText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Senha não pode ser vazia.");

            if (value.Length < 6)
                throw new ArgumentException("Senha deve ter pelo menos 6 caracteres.");

            if (!ComplexityPattern.IsMatch(value))
                throw new ArgumentException("Senha inválida. A senha deve conter pelo menos uma letra e um número.");

            return new Password(Hash(value));
        }

        public static Password FromHash(string value)
        {
            return new Password(value);
        }

        public bool Verify(string plainText)
        {
            var parts = Value.Split(':');
            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var iterations))
                return false;

            var salt = Convert.FromBase64String(parts[1]);
            var hash = Convert.FromBase64String(parts[2]);
            var incoming = Rfc2898DeriveBytes.Pbkdf2(plainText, salt, iterations, HashAlgorithmName.SHA256, hash.Length);
            return CryptographicOperations.FixedTimeEquals(incoming, hash);
        }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (Password)obj;
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

        private static string Hash(string value)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(value, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
            return string.Join(':', Iterations.ToString(CultureInfo.InvariantCulture), Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }
    }
}


