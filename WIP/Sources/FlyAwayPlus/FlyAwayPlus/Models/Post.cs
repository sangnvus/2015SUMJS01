using FlyAwayPlus.Helpers;
using System.ComponentModel.DataAnnotations;

namespace FlyAwayPlus.Models
{
    public class Post
    {
        public int postID { get; set; }
        public string content { get; set; }
        public string dateCreated { get; set; }
        public string privacy { get; set; }

        public string toRealtime()
        {
            return DateHelpers.Instance.DisplayRealtime(dateCreated);
        }
    }
}