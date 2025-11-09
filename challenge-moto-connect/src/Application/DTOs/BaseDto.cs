using challenge_moto_connect.Application.DTOs.HATEOAS;
using System.Collections.Generic;

namespace challenge_moto_connect.Application.DTOs
{
    public abstract class BaseDto
    {
        public List<LinkDto> Links { get; set; } = new List<LinkDto>();
    }
}
