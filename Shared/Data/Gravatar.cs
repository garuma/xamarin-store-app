using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

#if __IOS__
using MonoTouch.Foundation;
#endif

namespace XamarinStore
{
	class Gravatar
	{
		public enum Rating { G, PG, R, X }

		const string _url = "http://www.gravatar.com/avatar.php?gravatar_id=";

		public static string GetURL (string email, int size, Rating rating = Rating.PG)
		{
			var hash = MD5Hash (email.ToLower ());

			if (size < 1 | size > 600) {
				throw new ArgumentOutOfRangeException("size", "The image size should be between 20 and 80");
			}

			return _url + hash + "&s=" + size.ToString () + "&r=" + rating.ToString ().ToLower ();
		}

		public static async Task<byte[]> GetImageBytes (string email, int size, Rating rating = Rating.PG)
		{
			var url = GetURL (email, size, rating);
			var client = new WebClient ();
			return await client.DownloadDataTaskAsync (url);
		}

#if __IOS__
		public static async Task<NSData> GetImageData (string email, int size, Rating rating = Rating.PG)
		{
			byte[] data = await GetImageBytes (email, size, rating);
			return NSData.FromStream (new System.IO.MemoryStream (data));
		}
#endif

		static string MD5Hash (string input)
		{
			var hasher = MD5.Create ();
			var builder = new StringBuilder ();
			byte[] data = hasher.ComputeHash (Encoding.Default.GetBytes (input));

			foreach (byte datum in data)
				builder.Append (datum.ToString ("x2"));

			return builder.ToString ();
		}
	}
}

