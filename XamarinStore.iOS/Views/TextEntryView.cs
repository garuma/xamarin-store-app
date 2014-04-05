using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace XamarinStore
{
	public class TextEntryView : UIView
	{
		public Action<string> ValueChanged = (s)=> {};
		readonly protected UITextField textField;
		public TextEntryView () : base (new RectangleF(0,0,320,44))
		{
			AddSubview (textField = new UITextField {
				BorderStyle = UITextBorderStyle.RoundedRect,
				ShouldReturn = (tf)=>{
					tf.ResignFirstResponder();
					return true;
				},
			});
		}
		public UIKeyboardType KeyboardType
		{
			get{ return textField.KeyboardType; }
			set{ textField.KeyboardType = value; }
		}

		public UITextAutocapitalizationType AutocapitalizationType
		{
			get { return textField.AutocapitalizationType; }
			set { textField.AutocapitalizationType = value; }
		}

		public string PlaceHolder {
			get {
				return textField.Placeholder;
			}
			set {
				textField.Placeholder = value;
			}
		}

		public string Value {
			get {
				return textField.Text;
			}
			set {
				textField.Text = value;
				ValueChanged (textField.Text);
			}
		}

		public override void LayoutSubviews ()
		{
			const float sidePadding = 10f;
			const float topPadding = 5f;
			base.LayoutSubviews ();

			var width = (Bounds.Width - (sidePadding * 2));
			var height = Bounds.Height - (topPadding * 2);

			textField.Frame = new RectangleF (sidePadding, topPadding, width, height);
		}
	}
}

