using Backend.Models;
using Backend.Models.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.DTOs.Requests.Identity
{
    public class CreateTeamSocialRequest
    {
        public Guid TeamMemberId { get; set; }
        public Guid? Id { get; set; }


        public string Platform { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
    }
}
