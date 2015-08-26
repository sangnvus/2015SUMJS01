using FlyAwayPlus.Helpers;
using System.ComponentModel.DataAnnotations;

namespace FlyAwayPlus.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public string Content { get; set; }
        public string DateCreated { get; set; }
        public string Privacy { get; set; }

        public string ToRealtime()
        {
            return DateHelpers.Instance.DisplayRealtime(DateCreated);
        }
    }
}