#if !NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using VelocityDb;
using VelocityDb.Session;

namespace MvcMusicStore.Models
{
    public class Order : VelocityClass<Order>
    {
        #region Fields

        private System.DateTime _OrderDate;

        private string _Username;

        private string _FirstName;

        private string _LastName;

        private string _Address;

        private string _City;

        private string _State;

        private string _PostalCode;

        private string _Country;

        private string _Phone;

        private string _Email;

        private decimal _Total;
        #endregion

        #region Properties
        [ScaffoldColumn(false)]
        public System.DateTime OrderDate
        {
            get
            {
                return _OrderDate;
            }
            set
            {
                _OrderDate = value;
            }
        }

        [ScaffoldColumn(false)]
        public string Username
        {
            get
            {
                return _Username;
            }
            set
            {
                _Username = value;
            }
        }

        [Required(ErrorMessage = "First Name is required")]
        [DisplayName("First Name")]
        [StringLength(160)]
        public string FirstName
        {
            get
            {
                return _FirstName;
            }
            set
            {
                _FirstName = value;
            }
        }

        [Required(ErrorMessage = "Last Name is required")]
        [DisplayName("Last Name")]
        [StringLength(160)]
        public string LastName
        {
            get
            {
                return _LastName;
            }
            set
            {
                _LastName = value;
            }
        }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(70)]
        public string Address
        {
            get
            {
                return _Address;
            }
            set
            {
                _Address = value;
            }
        }

        [Required(ErrorMessage = "City is required")]
        [StringLength(40)]
        public string City
        {
            get
            {
                return _City;
            }
            set
            {
                _City = value;
            }
        }

        [Required(ErrorMessage = "State is required")]
        [StringLength(40)]
        public string State
        {
            get
            {
                return _State;
            }
            set
            {
                _State = value;
            }
        }

        [Required(ErrorMessage = "Postal Code is required")]
        [DisplayName("Postal Code")]
        [StringLength(10)]
        public string PostalCode
        {
            get
            {
                return _PostalCode;
            }
            set
            {
                _PostalCode = value;
            }
        }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(40)]
        public string Country
        {
            get
            {
                return _Country;
            }
            set
            {
                _Country = value;
            }
        }

        [Required(ErrorMessage = "Phone is required")]
        [StringLength(24)]
        public string Phone
        {
            get
            {
                return _Phone;
            }
            set
            {
                _Phone = value;
            }
        }

        [Required(ErrorMessage = "Email Address is required")]
        [DisplayName("Email Address")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Email is is not valid.")]
        [DataType(DataType.EmailAddress)]
        public string Email
        {
            get
            {
                return _Email;
            }
            set
            {
                _Email = value;
            }
        }

        [ScaffoldColumn(false)]
        public decimal Total
        {
            get
            {
                return _Total;
            }
            set
            {
                _Total = value;
            }
        }

        public List<OrderDetail> OrderDetails { get; set; }
        #endregion

        public Order() : base() { }
    }
}
#endif