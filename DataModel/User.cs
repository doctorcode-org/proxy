using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorProxy.DataModel
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 4)]
        public string Username { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Password { get; set; }

        [Required]
        public DateTime RegistDate { get; set; }

        public string Email { get; set; }

        [StringLength(11, MinimumLength = 11)]
        public string Mobile { get; set; }

        public DateTime? StartDate { get; set; }

        [Required]
        public int CreditDays { get; set; }

        [Required]
        public int ConcurrentLoginCount { get; set; }

    }
}
