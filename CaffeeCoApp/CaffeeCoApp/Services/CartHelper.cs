using System.Text.Json;

namespace CaffeeCoApp.Services
{
    public class CartHelper
    {
        public static Dictionary<int,int> GetCartDictionary(HttpRequest request, HttpResponse response)
        {
            string cookieValue = request.Cookies["shopping_cart"] ?? "";
            try
            {
                var cart = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cookieValue));
                var dictionary = JsonSerializer.Deserialize<Dictionary<int, int>>(cart);
                if (dictionary != null)
                {
                    return dictionary;
                }
            } catch (Exception)
            {
            }

            if (cookieValue.Length > 0)
            {
                response.Cookies.Delete("shopping_cart");
            }

            return new Dictionary<int, int>();
        }

        public static int GetCartSize(HttpRequest request, HttpResponse response)
        {
            int cartSize = 0;
            var cartDictionary = GetCartDictionary(request, response);
            foreach (var pair in cartDictionary)
            {
                cartSize += pair.Value;
            }
            return cartSize;
        }

    }
}
