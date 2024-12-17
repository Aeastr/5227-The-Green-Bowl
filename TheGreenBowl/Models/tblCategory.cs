namespace TheGreenBowl.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class tblCategory
{
    [Key]
    public int CategoryId { get; set; } // maps to category_id

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    public string Description { get; set; }

    // navigation property
    public ICollection<tblMenuItem> MenuItems { get; set; }
}