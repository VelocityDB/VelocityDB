#if !NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VelocityDb;
using VelocityDb.Session;

namespace MvcMusicStore.Models
{

    public class Cart : VelocityClass<Cart>
    {
        private string _CartId;
        private Album _Album;
        private int _Count;
        private DateTime _DateCreated;

        public string CartId
        {
            get
            {
                return _CartId;
            }
            set
            {
                _CartId = value;
            }
        }
        public Album Album
        {
            get
            {
                return _Album;
            }
            set
            {
                _Album = value;
            }
        }
        public int Count
        {
            get
            {
                return _Count;
            }
            set
            {
                _Count = value;
            }
        }
        public DateTime DateCreated
        {
            get
            {
                return _DateCreated;
            }
            set
            {
                _DateCreated = value;
            }
        }

        public Cart() : base() { }

        public static Cart FindByCartIdAlbum(string cartId,Album album)
        {
            return (from c in VelocityDB.Session.OfType<Cart>() where c.CartId == cartId && c.Album == album select c).FirstOrDefault();
        }

        public static void DeleteByCartId(string cartId)
        {
            foreach (Cart cart in from c in VelocityDB.Session.OfType<Cart>() where c.CartId == cartId select c)
            {
                VelocityDB.Session.BeginUpdate();

                //remove
                
                VelocityDB.Session.Commit();
            }
        }

        public static IEnumerable<Cart> FindByCartId(string cartId)
        {
            return from c in VelocityDB.Session.OfType<Cart>() where c.CartId == cartId select c;
        }
    }
}
#endif