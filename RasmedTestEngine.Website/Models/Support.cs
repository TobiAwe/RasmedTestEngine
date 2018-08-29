using System.ComponentModel.DataAnnotations;

namespace RasmedTestEngine.Website.Models
{
   
    public class Support
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 5)]
        public string Name { get; set; }

      
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(15, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 11)]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 10)]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 50)]
        public string Message { get; set; }
    }

   

  
}
