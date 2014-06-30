using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Linq;

namespace XamarinStore
{
	public class StringSelectionCell : UITableViewCell
	{
		public const string Key = "SelectionCell";

		public Action SelectionChanged;

		readonly UIView ViewForPicker;
		readonly StringUIPicker picker;

		public IEnumerable<string> Items {
			get { return picker.Items; }
			set {
				picker.Items = value;

				switch (value.Count ()) {
				case 1:
					DetailTextLabel.TextColor = UIColor.Gray;
					//UserInteractionEnabled = false;
					Accessory = UITableViewCellAccessory.None;
					break;
				default:
					DetailTextLabel.TextColor = UIColor.Black;
					//UserInteractionEnabled = true;
					Accessory = UITableViewCellAccessory.DisclosureIndicator;
					break;
				}
			}
		}

		public string SelectedItem {
			get { return picker.SelectedItem; }
			set { DetailText =picker.SelectedItem = value; }
		}

		public int SelectedIndex {
			get { return picker.SelectedIndex; }
			set { picker.SelectedIndex = value; }
		}

		public string Text {
			get{ return TextLabel.Text; }
			set{ TextLabel.Text = value; }
		}

		public string DetailText {
			get{ return DetailTextLabel.Text; }
			set{ DetailTextLabel.Text = value; }
		}

		public StringSelectionCell (UIView viewForPicker)
			: base (UITableViewCellStyle.Value1, Key)
		{
			SelectionStyle = UITableViewCellSelectionStyle.None;
			TextLabel.TextColor = Color.Purple;
			ViewForPicker = viewForPicker;
			Accessory = UITableViewCellAccessory.DisclosureIndicator;

			picker = new StringUIPicker ();
			picker.SelectedItemChanged += (object sender, EventArgs e) => {
				DetailTextLabel.Text = picker.SelectedItem;
				if (SelectionChanged != null)
					SelectionChanged();
			};

			Items = new string[0];
		}

		public void Tap ()
		{
			// Don't show the picker when we don't have options.
			if (Items.Count () == 1)
				return;

			picker.SelectedIndex = Items.ToList().IndexOf (DetailText);
			picker.ShowPicker (ViewForPicker);
		}
	}
}

