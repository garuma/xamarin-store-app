using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace XamarinStore
{
	public class AutoCompleteTextEntry : TextEntryView
	{
		StringTableViewController controller;
		public string Title { get; set; }
		public AutoCompleteTextEntry ()
		{
			controller = new StringTableViewController () {
				ItemSelected = (item) => {
					Value = item;
				}
			};
			

			textField.Started += (object sender, EventArgs e) => {
				Search ();
			};
		}

		IEnumerable<string> items = new List<string>();

		public IEnumerable<string> Items {
			get {
				return items;
			}
			set {
				items = value;
			}
		}

		void Search ()
		{
			if (items.Count() == 0)
				return;

			textField.ResignFirstResponder();
			controller.Title = Title;
			controller.Items = items;
			if(PresenterView.NavigationController.TopViewController != controller)
				PresenterView.NavigationController.PushViewController (controller, true);


		}

		public UIViewController PresenterView {get;set;}
	}
}

