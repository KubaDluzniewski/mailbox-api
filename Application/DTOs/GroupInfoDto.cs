using System.Collections.Generic;

namespace Application.DTOs
{
    public class GroupInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<UserDto> Members { get; set; }
        public int MemberCount { get; set; }
    }
}
