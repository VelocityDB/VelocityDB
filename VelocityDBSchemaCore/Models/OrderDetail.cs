#if !NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VelocityDb;
using VelocityDb.Session;

namespace MvcMusicStore.Models
{
    public class OrderDetail : VelocityClass<OrderDetail>
    {
        #region Fields

        private Order _Order;

        private Album _Album;

        private int _Quantity;

        private decimal _UnitPrice;
        #endregion

        #region Properties
        public Order Order
        {
            get
            {
                return _Order;
            }
            set
            {
                _Order = value;
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
        public int Quantity
        {
            get
            {
                return _Quantity;
            }
            set
            {
                _Quantity = value;
            }
        }
        public decimal UnitPrice
        {
            get
            {
                return _UnitPrice;
            }
            set
            {
                _UnitPrice = value;
            }
        }
        #endregion

        #region Constructors
        public OrderDetail() : base() { }
        #endregion

    }
}
#endif