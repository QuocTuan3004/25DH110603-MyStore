using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _25DH110603.Models
{
    public class CategoryMetadata
    {
        [HiddenInput]
        public int CategoryID { get; set; }
        [Required]
        [StringLength(50, MinimumLength =5)]
        public string CategoryName { get; set; }
    }
    public class UserMetadata
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string UserRole { get; set; }
    }
    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category { }

}