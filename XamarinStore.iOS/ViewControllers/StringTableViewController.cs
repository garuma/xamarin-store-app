using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Linq;

namespace XamarinStore
{
	public class StringTableViewController : UITableViewController
	{
		public Action<string> ItemSelected = (i)=> {};
		UISearchBar searchBar;
		IEnumerable<string> items = new List<string>();
		IEnumerable<string> filteredItems = new List<string>();
		TableViewSource source;
		public IEnumerable<string> Items {
			get {
				return items;
			}
			set {
				filteredItems = items = value;
				TableView.ReloadData ();
			}
		}
		public StringTableViewController ()
		{
			TableView.Source = source = new TableViewSource (){
				Parent = this,
			};
			searchBar = new UISearchBar ();
			searchBar.TextChanged += (object sender, UISearchBarTextChangedEventArgs e) => {
				filteredItems = items.Where(x=> x.IndexOf(searchBar.Text, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();
				TableView.ReloadData();
			};
			searchBar.SizeToFit ();
			TableView.TableHeaderView = searchBar;
		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			source.Parent = this;
		}
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			searchBar.BecomeFirstResponder ();
		}
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			source.Parent = null;
		}

		class TableViewSource : UITableViewSource
		{
			public StringTableViewController Parent;
			#region implemented abstract members of UITableViewSource

			public override int RowsInSection (UITableView tableview, int section)
			{
				if (Parent == null)
					return 0;
				return Parent.filteredItems.Count ();
			}

			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell ("stringCell") ?? new UITableViewCell (UITableViewCellStyle.Default, "stringCell");
				if(Parent != null)
					cell.TextLabel.Text = Parent.filteredItems.ElementAt (indexPath.Row);
				return cell;
			}

			#endregion

			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				if (Parent == null)
					return;
				var item = Parent.filteredItems.ElementAt (indexPath.Row);
				Parent.ItemSelected (item);
				Parent.NavigationController.PopViewControllerAnimated (true);
			}


		}
	}
}

