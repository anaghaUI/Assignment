//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Assignment.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Event
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Event()
        {
            this.Bookings = new HashSet<Booking>();
        }
    
        public int Id { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Event name can only contain alphanumeric characters")]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string Optional_Services { get; set; }
        public string ImagePath { get; set; }
        public int BasePrice { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}
