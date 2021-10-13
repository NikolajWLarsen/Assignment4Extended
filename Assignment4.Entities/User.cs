using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assignment4.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        //[IsUnique = true] //DataAnnotation
        [StringLength(100)]
        public string Email { get; set; }
        
        public ICollection<Task> Tasks { get; set; }
    }
}
