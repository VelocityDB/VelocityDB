#if !NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using VelocityDb;
using VelocityDb.Session;

namespace MvcMusicStore.Models
{

    public class Album : VelocityClass<Album>
    {
        private string title;
        private decimal price;
        private Genre genre;
        private string albumArtUrl;
        private Artist artist;
        
        public string AlbumArtUrl
        {
            get
            {
                return albumArtUrl;
            }
            set
            {
                albumArtUrl = value;
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Required price")]
        [Range(0, double.MaxValue)]
        public decimal Price
        {
            get
            {
                return price;
            }
            set
            {
                price = value;
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Required title")]
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
            }
        }
        
        [Required(AllowEmptyStrings=false, ErrorMessage="Required genre")]
        public Genre Genre
        {
            get
            {
                return genre;
            }
            set
            {
                genre = value;
            }
        }
        
        [Required(AllowEmptyStrings=false, ErrorMessage="Required artist")]
        public Artist Artist
        {
            get
            {
                return artist;
            }
            set
            {
                artist = value;
            }
        }

        public Album() : base() { }

        public IEnumerable<OrderDetail> OrderDetails()
        {
            return from a in VelocityDB.Session.OfType<OrderDetail>() where a.Album == this select a;
        }

        public static IEnumerable<Album> FindByGenre(Genre genre)
        {
            return from a in VelocityDB.Session.OfType<Album>() where a.Genre == genre select a;
        }

        public static IEnumerable<Album> FindByArtist(Artist artist)
        {
            return from a in VelocityDB.Session.OfType<Album>() where a.Artist == artist select a;
        }

        public static IEnumerable<Album> GetTopSelling(int count)
        {
           return VelocityDB.Session.OfType<Album>().OrderByDescending(a => a.OrderDetails().Count()).Take(count);
        }
    }
}
#endif