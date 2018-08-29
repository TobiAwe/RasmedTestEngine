using System;
using System.ComponentModel.DataAnnotations;

namespace RasmedTestEngine.Core.Entities
{
    public class MemberProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        public string Address { get; set; }

        public DateTime RegistrationDate { get; set; }


        [Required]
        public virtual Member Member { get; set; }
    }
}