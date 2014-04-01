using System;

namespace XamarinStore
{
	public class User
	{
		public User ()
		{

		}

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Email { get; set; }

		public string Token { get; set; }

		public string Phone {get;set;}

		public string Address { get; set; }

		public string Address2 { get; set; }

		public string City { get; set; }

		public string State { get; set; }

		public string Country { get; set; }

		public string ZipCode { get; set; }

		public Tuple<bool,string> IsInformationValid()
		{

			if (string.IsNullOrEmpty (Phone))
				return new Tuple<bool, string>(false,"Phone number is required");

			if (string.IsNullOrEmpty (FirstName))
				return new Tuple<bool, string>(false,"First name is required");

			if (string.IsNullOrEmpty (LastName))
				return new Tuple<bool, string>(false,"Last name is required");

			if(string.IsNullOrEmpty(Address))
				return new Tuple<bool, string>(false,"Address is required");

			if (string.IsNullOrEmpty (City))
				return new Tuple<bool, string>(false,"City is required");

			if (string.IsNullOrEmpty (State) && Country.ToLower () == "united states")
				return new Tuple<bool, string>(false,"State is required");

			if (string.IsNullOrEmpty (Country))
				return new Tuple<bool, string>(false,"Country is required");

			if (string.IsNullOrEmpty (ZipCode))
				return new Tuple<bool, string>(false,"ZipCode is required");


			return new Tuple<bool, string>(true,"");
		}
	}
}

