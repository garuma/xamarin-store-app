using System;
using System.Linq;
using System.Collections.Generic;
using MonoTouch.UIKit;
using System.Drawing;

namespace XamarinStore
{
	public class StringUIPicker : UIPickerView
	{
		public event EventHandler SelectedItemChanged;
		public StringUIPicker ()
		{
		}
		string[] items;
		public IEnumerable<string> Items
		{
			get{ return items; }
			set{ 
				items = value.ToArray ();
				Model = new PickerModel {
					Items = items,
					Parent = this,
				};
			}
		}
		int currentIndex;
		public int SelectedIndex {
			get{ return currentIndex; }
			set{
				if (currentIndex == value)
					return;
				currentIndex = value;
				this.Select (currentIndex, 0, true);
				if (SelectedItemChanged != null)
					SelectedItemChanged (this, EventArgs.Empty);
			}
		}
		public string SelectedItem
		{
			get { 
				return items.Length <= currentIndex ? "" : items [currentIndex];
			}
			set {
				if(!items.Contains(value))
					return;
				currentIndex = Array.IndexOf (items, value);
			}
		}

		UIActionSheet sheet;
		public void ShowPicker(UIView viewForPicker)
		{
			sheet = new UIActionSheet();

			sheet.AddSubview(this);

			var toolbarPicker = new UIToolbar (new RectangleF (0, 0, viewForPicker.Frame.Width, 44)) {
				Items = new UIBarButtonItem[] {
					new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace), 
					new UIBarButtonItem (UIBarButtonSystemItem.Done, (sender, args) => sheet.DismissWithClickedButtonIndex (0, true)), 
				},
				BarTintColor = this.BackgroundColor,
			};

			sheet.AddSubviews(toolbarPicker);

			sheet.BackgroundColor = UIColor.Clear;
			sheet.ShowInView(viewForPicker);
			UIView.Animate(.25, () => sheet.Bounds = new RectangleF (0, 0, viewForPicker.Frame.Width, 485));

		}


		class PickerModel : UIPickerViewModel
		{
			public StringUIPicker Parent { get; set; }
			public string[] Items = new string[0];

			public override int GetComponentCount (UIPickerView picker)
			{
				return 1;
			}

			public override int GetRowsInComponent (UIPickerView picker, int component)
			{
				return Items.Length;
			}

			public override string GetTitle (UIPickerView picker, int row, int component)
			{

				return Items [row];
			}

			public override void Selected (UIPickerView picker, int row, int component)
			{
				if (Parent != null)
					Parent.SelectedIndex = row;
			}

		}

	}
}

