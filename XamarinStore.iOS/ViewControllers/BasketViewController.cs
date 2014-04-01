using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;
using MonoTouch.CoreAnimation;
using System.Linq;
using System.Threading.Tasks;

namespace XamarinStore
{
	public class BasketViewController : UITableViewController
	{
		Order order;
		EmptyBasketView EmptyCartImageView;
		BottomButtonView BottomView;
		UILabel totalAmount;

		public event EventHandler Checkout;

		public BasketViewController (Order order)
		{
			this.Title = "Your Basket";
			//This hides the back button text when you leave this View Controller
			this.NavigationItem.BackBarButtonItem = new UIBarButtonItem ("", UIBarButtonItemStyle.Plain, handler: null);
			TableView.Source = new TableViewSource (this.order = order) {
				RowDeleted = CheckEmpty,
			};
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			TableView.RowHeight = 75;
			TableView.TableFooterView = new UIView (new RectangleF (0, 0, 0, BottomButtonView.Height));
			this.View.AddSubview (BottomView = new BottomButtonView () {
				ButtonText = "Checkout",
				ButtonTapped = () => {
					if (Checkout != null)
						Checkout (this, EventArgs.Empty);
				}
			});
			CheckEmpty (false);
			totalAmount = new UILabel () {
				Text = "$1,000",
				TextColor = UIColor.White,
				TextAlignment = UITextAlignment.Center,
				Font = UIFont.BoldSystemFontOfSize (17),
			};
			totalAmount.SizeToFit ();
			this.NavigationItem.RightBarButtonItem = new UIBarButtonItem (totalAmount);
			UpdateTotals ();
		}

		public void UpdateTotals ()
		{
			if (order.Products.Count == 0) {
				totalAmount.Text = "";
				return;
			}
			var total = order.Products.Sum (x => x.Price);
			totalAmount.Text = total.ToString ("C");
		}

		public override void ViewDidLayoutSubviews ()
		{
			base.ViewDidLayoutSubviews ();
			var bound = View.Bounds;
			bound.Y = bound.Bottom - BottomButtonView.Height;
			bound.Height = BottomButtonView.Height;
			BottomView.Frame = bound;

			if (EmptyCartImageView == null)
				return;
			EmptyCartImageView.Frame = View.Bounds;
		}

		void CheckEmpty ()
		{
			UpdateTotals ();
			CheckEmpty (true);
		}

		protected void CheckEmpty (bool animate)
		{
			if (order.Products.Count == 0) {
				this.View.AddSubview (EmptyCartImageView = new EmptyBasketView () {
					Alpha = animate ? 0f : 1f,
				});
				this.View.BringSubviewToFront (EmptyCartImageView);
				if (animate)
					UIView.Animate (.25, () => EmptyCartImageView.Alpha = 1f);
				return;
			}
	
			if (EmptyCartImageView == null)
				return;
			EmptyCartImageView.RemoveFromSuperview ();
			EmptyCartImageView = null;
		}

		class TableViewSource : UITableViewSource
		{
			public Action RowDeleted { get; set; }

			Order order;

			public TableViewSource (Order order)
			{
				this.order = order;

			}

			#region implemented abstract members of UITableViewSource

			public override int RowsInSection (UITableView tableview, int section)
			{
				return order.Products.Count;
			}

			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell (ProductCell.Key) as ProductCell ?? new ProductCell ();
				//No need to wait to return the cell It will update when the data is ready
				#pragma warning disable 4014
				cell.Update (order.Products [indexPath.Row]);
				#pragma warning restore 4014
				return cell;
			}

			#endregion

			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				return UITableViewCellEditingStyle.Delete;
			}

			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				if (editingStyle == UITableViewCellEditingStyle.Delete) {
					order.Remove (order.Products [indexPath.Row]);
					tableView.DeleteRows (new MonoTouch.Foundation.NSIndexPath[]{ indexPath }, UITableViewRowAnimation.Fade);
					if (RowDeleted != null)
						RowDeleted ();
				}
			
			}

			class ProductCell : UITableViewCell
			{
				static Lazy<UIImage> Image = new Lazy<UIImage> (() => UIImage.FromBundle ("shirt_image"));
				public const string Key = "productCell";
				static SizeF ImageSize = new SizeF (55, 55);
				UILabel NameLabel;
				UILabel SizeLabel;
				UILabel ColorLabel;
				UILabel PriceLabel;
				UIView LineView;

				public ProductCell () : base (UITableViewCellStyle.Default, Key)
				{
					SelectionStyle = UITableViewCellSelectionStyle.None;
					ContentView.BackgroundColor = UIColor.Clear;
					ImageView.Image = Image.Value;
					ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
					ImageView.Frame = new RectangleF (PointF.Empty, ImageSize);
					ImageView.BackgroundColor = UIColor.Clear;
					ImageView.Layer.CornerRadius = 5f;
					ImageView.Layer.MasksToBounds = true;
					NameLabel = new UILabel () {
						Text = "Name",
						Font = UIFont.BoldSystemFontOfSize (17f),
						BackgroundColor = UIColor.Clear,
						TextColor = UIColor.Black,
					};
					NameLabel.SizeToFit ();
					ContentView.AddSubview (NameLabel);

					SizeLabel = new UILabel () {
						Text = "Size",
						Font = UIFont.BoldSystemFontOfSize (12f),
						BackgroundColor = UIColor.Clear,
						TextColor = UIColor.LightGray,
					};
					SizeLabel.SizeToFit ();
					ContentView.Add (SizeLabel);

					ColorLabel = new UILabel () {
						Text = "Color",
						Font = UIFont.BoldSystemFontOfSize (12f),
						BackgroundColor = UIColor.Clear,
						TextColor = UIColor.LightGray,
					};
					ColorLabel.SizeToFit ();
					ContentView.AddSubview (ColorLabel);

					PriceLabel = new UILabel () {
						Text = "Price",
						Font = UIFont.BoldSystemFontOfSize (15f),
						BackgroundColor = UIColor.Clear,
						TextAlignment = UITextAlignment.Right,
						TextColor = Color.Blue,
					};
					PriceLabel.SizeToFit ();
					AccessoryView = new UIView (new RectangleF (0, 0, PriceLabel.Frame.Width + 10, 54)) {
						BackgroundColor = UIColor.Clear,
					};
					AccessoryView.AddSubview (PriceLabel);

					ContentView.AddSubview (LineView = new UIView () {
						BackgroundColor = UIColor.LightGray,
					});

				}

				const float leftPadding = 15f;
				const float topPadding = 5f;

				public override void LayoutSubviews ()
				{
					base.LayoutSubviews ();

					var bounds = ContentView.Bounds;
					var midY = bounds.GetMidY ();

					var center = new PointF (ImageSize.Width / 2 + leftPadding, midY);
					ImageView.Frame = new RectangleF (PointF.Empty, ImageSize);
					ImageView.Center = center;


					var x = ImageView.Frame.Right + leftPadding;
					var y = ImageView.Frame.Top;					
					var labelWidth = bounds.Width - (x + (leftPadding * 2));


					NameLabel.Frame = new RectangleF (x, y, labelWidth, NameLabel.Frame.Height);
					y = NameLabel.Frame.Bottom;

					SizeLabel.Frame = new RectangleF (x, y, labelWidth, SizeLabel.Frame.Height);
					y = SizeLabel.Frame.Bottom;

					ColorLabel.Frame = new RectangleF (x, y, labelWidth, ColorLabel.Frame.Height);
					y = ColorLabel.Frame.Bottom + topPadding;
					LineView.Frame = new RectangleF (0, Bounds.Height - .5f, Bounds.Width, .5f);

				}

				public async Task Update (Product product)
				{
					NameLabel.Text = product.Name;
					SizeLabel.Text = product.Size.Description;
					ColorLabel.Text = product.Color.Name;
					PriceLabel.Text = product.PriceDescription;
					var imageTask = FileCache.Download (product.ImageForSize (320));
					if(!imageTask.IsCompleted)
						//Put default before doing the web request;
						ImageView.Image = Image.Value;
					var image = await imageTask;
					ImageView.Image = UIImage.FromFile (image);
				}
			}
		}
	}
}

