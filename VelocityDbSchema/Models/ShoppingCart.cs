using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcMusicStore.Models
{
    public class ShoppingCart
    {
        string ShoppingCartId { get; set; }

        public const string CartSessionKey = "CartId";

        public static ShoppingCart GetCart(HttpContextBase context)
        {
            var cart = new ShoppingCart();
            cart.ShoppingCartId = cart.GetCartId(context);
            return cart;
        }

        // Helper method to simplify shopping cart calls
        public static ShoppingCart GetCart(Controller controller)
        {
            return GetCart(controller.HttpContext);
        }

        public void AddToCart(Album album)
        {
            // Get the matching cart and album instances
            Cart cartItem = Cart.FindByCartIdAlbum(ShoppingCartId, album);

            if (cartItem == null)
            {
                // Create a new cart item if no cart item exists
                cartItem = new Cart
                {
                    Album = album,
                    CartId = ShoppingCartId,
                    Count = 1,
                    DateCreated = DateTime.Now
                };
            }
            else
            {
                // If the item does exist in the cart, then add one to the quantity
                cartItem.Count++;
            }

            // Save changes
            cartItem.Save();
        }

        public int RemoveFromCart(ulong id)
        {
            // Get the cart
            Cart cartItem = Cart.FindById(id);

            int itemCount = 0;

            if (cartItem != null)
            {
                if (cartItem.Count > 1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                }

                // Save changes
                //cartItem.Delete();
            }

            return itemCount;
        }

        public void EmptyCart()
        {
            Cart.DeleteByCartId(ShoppingCartId);
        }

        public IEnumerable<Cart> GetCartItems()
        {
            return Cart.FindByCartId(ShoppingCartId);
        }

        public int GetCount()
        {
            /*
            // Get the count of each item in the cart and sum them up
            int? count = (from cartItems in storeDB.Carts
                          where cartItems.CartId == ShoppingCartId
                          select (int?)cartItems.Count).Sum();

            // Return 0 if all entries are null
            return count ?? 0;*/
            return GetCartItems().Count();
        }

        public decimal GetTotal()
        {
            /*
            // Multiply album price by count of that album to get 
            // the current price for each of those albums in the cart
            // sum all album price totals to get the cart total
            decimal? total = (from cartItems in storeDB.Carts
                              where cartItems.CartId == ShoppingCartId
                              select (int?)cartItems.Count * cartItems.Album.Price).Sum();
            return total ?? decimal.Zero;
             */

            decimal? total = (from c in GetCartItems() select (c.Count * c.Album.Price)).Sum();
            return total ?? decimal.Zero;
        }

        #region Documentation
        ///<summary>
        /// Retorna o Id de uma compra, passada no parâmetro, como confirmação.
        ///</summary>
        #endregion
        public ulong CreateOrder(Order order)
        {
            decimal orderTotal = 0;

            var cartItems = GetCartItems();

            // Iterate over the items in the cart, adding the order details for each
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    Album = item.Album,
                    Order = order,
                    UnitPrice = item.Album.Price,
                    Quantity = item.Count
                };

                // Set the order total of the shopping cart
                orderTotal += (item.Count * item.Album.Price);

                orderDetail.Save();

            }

            // Set the order's total to the orderTotal count
            order.Total = orderTotal;

            // Save the order
            order.Save();

            // Empty the shopping cart
            EmptyCart();

            // Return the OrderId as the confirmation number
            return order.Id;
        }

        // We're using HttpContextBase to allow access to cookies.
        public string GetCartId(HttpContextBase context)
        {
            if (context.Session[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    context.Session[CartSessionKey] = context.User.Identity.Name;
                }
                else
                {
                    // Generate a new random GUID using System.Guid class
                    Guid tempCartId = Guid.NewGuid();

                    // Send tempCartId back to client as a cookie
                    context.Session[CartSessionKey] = tempCartId.ToString();
                }
            }

            return context.Session[CartSessionKey].ToString();
        }

        // When a user has logged in, migrate their shopping cart to
        // be associated with their username
        public void MigrateCart(string userName)
        {
            //var shoppingCart = storeDB.Carts.Where(c => c.CartId == ShoppingCartId);

            var shoppingCart = Cart.FindByCartId(ShoppingCartId);

            foreach (Cart item in shoppingCart)
            {
                item.CartId = userName;
                item.Save();
            }
            
        }
    }
}