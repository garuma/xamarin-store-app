using System;
using System.Collections.Generic;

namespace XamarinStore
{
	public static class ListExtensions
	{
		static Random rng = new Random();
		public static List<T> Shuffle<T>(this List<T> list)  
		{  
			int n = list.Count;  
			while (n > 1) {  
				n--;  
				int k = rng.Next(n + 1);  
				T value = list[k];  
				list[k] = list[n];  
				list[n] = value;  
			}  
			return list;
		}


		public static T[] Shuffle<T>(this T[] list)  
		{  
			int n = list.Length;  
			while (n > 1) {  
				n--;  
				int k = rng.Next(n + 1);  
				T value = list[k];  
				list[k] = list[n];  
				list[n] = value;  
			}  
			return list;
		}


		public static int IndexOf(this Array array, object item)
		{
			return Array.IndexOf (array, item);
		}
	}
}

