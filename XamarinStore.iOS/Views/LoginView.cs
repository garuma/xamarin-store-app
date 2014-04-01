using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace XamarinStore
{
	class LoginView : UIView
	{
		public readonly UIImageView GravatarView;
		static readonly SizeF GravatarSize = new SizeF (85f, 85f);

		public readonly UITextField EmailField;
		public readonly UITextField PasswordField;

		public event Action<LoginView> UserDidLogin = delegate{};

		public LoginView (string xamarinAccountEmail)
		{
			BackgroundColor = UIColor.White;

			Add (GravatarView = new UIImageView (new RectangleF (PointF.Empty, GravatarSize)) {
				TranslatesAutoresizingMaskIntoConstraints = false,
				Image = UIImage.FromBundle ("user-default-avatar"),
			});

			GravatarView.Layer.CornerRadius = GravatarSize.Width / 2;
			GravatarView.Layer.MasksToBounds = true;

			DisplayGravatar (xamarinAccountEmail);

			AddConstraint (NSLayoutConstraint.Create (
				GravatarView,
				NSLayoutAttribute.Top,
				NSLayoutRelation.Equal,
				this,
				NSLayoutAttribute.Top,
				1f, 90f
			));

			AddConstraint (NSLayoutConstraint.Create (
				GravatarView,
				NSLayoutAttribute.CenterX,
				NSLayoutRelation.Equal,
				this,
				NSLayoutAttribute.CenterX,
				1f, 0
			));

			AddConstantSizeConstraints (GravatarView, GravatarSize);

			Add (EmailField = new UITextField (new RectangleF (10, 10, 300, 30)) {
				BorderStyle = UITextBorderStyle.RoundedRect,
				Text = xamarinAccountEmail,
				TranslatesAutoresizingMaskIntoConstraints = false,
				Delegate = new EmailFieldDelegate ()
			});

			AddConstraint (NSLayoutConstraint.Create (
				EmailField,
				NSLayoutAttribute.Top,
				NSLayoutRelation.Equal,
				GravatarView,
				NSLayoutAttribute.Bottom,
				1f, 30f
			));

			AddConstraint (NSLayoutConstraint.Create (
				EmailField,
				NSLayoutAttribute.CenterX,
				NSLayoutRelation.Equal,
				GravatarView,
				NSLayoutAttribute.CenterX,
				1f, 0
			));

			var textSize = new NSString ("hello").StringSize (UIFont.SystemFontOfSize (12f));

			AddConstantSizeConstraints (EmailField, new SizeF (260, textSize.Height + 16));

			Add (PasswordField = new UITextField (new RectangleF (10, 10, 300, 30)) {
				BorderStyle = UITextBorderStyle.RoundedRect,
				SecureTextEntry = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
				ReturnKeyType = UIReturnKeyType.Go,
				Placeholder = "Password"
			});

			AddConstraint (NSLayoutConstraint.Create (
				PasswordField,
				NSLayoutAttribute.Top,
				NSLayoutRelation.Equal,
				EmailField,
				NSLayoutAttribute.Bottom,
				1f, 10f
			));

			AddConstraint (NSLayoutConstraint.Create (
				PasswordField,
				NSLayoutAttribute.CenterX,
				NSLayoutRelation.Equal,
				EmailField,
				NSLayoutAttribute.CenterX,
				1f, 0
			));

			AddConstantSizeConstraints (PasswordField, new SizeF (260, textSize.Height + 16));

			PasswordField.ShouldReturn = field => {
				field.ResignFirstResponder ();
				UserDidLogin (this);
				return true;
			};

			PasswordField.BecomeFirstResponder ();
		}

		async void DisplayGravatar (string email)
		{
			NSData data;

			try {
				data = await Gravatar.GetImageData (email, (int) GravatarSize.Width * 2);
			} catch {
				return;
			}

			GravatarView.Image = UIImage.LoadFromData (data);
		}

		void AddConstantSizeConstraints (UIView view, SizeF size)
		{
			AddConstraint (NSLayoutConstraint.Create (
				view,
				NSLayoutAttribute.Width,
				NSLayoutRelation.Equal,
				null,
				NSLayoutAttribute.NoAttribute,
				1f, size.Width
			));

			AddConstraint (NSLayoutConstraint.Create (
				view,
				NSLayoutAttribute.Height,
				NSLayoutRelation.Equal,
				null,
				NSLayoutAttribute.NoAttribute,
				1f, size.Height
			));
		}

		class EmailFieldDelegate : UITextFieldDelegate
		{
			public override bool ShouldBeginEditing (UITextField textField)
			{
				return false;
			}
		}
	}
}