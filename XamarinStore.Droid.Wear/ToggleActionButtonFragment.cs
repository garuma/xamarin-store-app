
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;

using Android.Support.Wearable.Views;

namespace XamarinStore.Droid.Wear
{
	public class ToggleActionButtonFragment : Fragment
	{
		Tuple<string, string> texts;
		int buttonDrawableId;
		bool initiallyEnabled;

		public event Action<bool> Toggled;

		public static ToggleActionButtonFragment WithAction (Tuple<string, string> texts, int buttonDrawableId, bool initiallyEnabled = false)
		{
			var r = new ToggleActionButtonFragment ();
			r.texts = texts;
			r.buttonDrawableId = buttonDrawableId;
			r.initiallyEnabled = initiallyEnabled;
			return r;
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate (Resource.Layout.ActionButtonLayout, container, false);
		}

		public override void OnViewCreated (View view, Bundle savedInstanceState)
		{
			base.OnViewCreated (view, savedInstanceState);
			var label = view.FindViewById<TextView> (Resource.Id.actionText);
			label.Text = !initiallyEnabled ? texts.Item1 : texts.Item2;

			var btn = view.FindViewById<ToggleButton> (Resource.Id.actionButton);
			btn.Checked = initiallyEnabled;
			btn.SetButtonDrawable (buttonDrawableId);

			btn.CheckedChange += (sender, e) => {
				var cked = e.IsChecked;
				label.Text = cked ? texts.Item2 : texts.Item1;
				var evt = Toggled;
				if (evt != null)
					evt (cked);
			};
		}
	}
}

