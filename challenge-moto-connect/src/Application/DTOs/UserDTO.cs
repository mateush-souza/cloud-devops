using System;
using challenge_moto_connect.Application.DTOs.HATEOAS;

namespace challenge_moto_connect.Application.DTOs
{
    public class UserDTO : BaseDto
    {
        public Guid UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Password { get; set; }
        public int Type { get; set; }
    }
}

