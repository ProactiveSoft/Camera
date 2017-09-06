using System;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Media.Plugin.Custom.Android.CameraWithoutConfirmation.Handlers;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;

namespace Media.Plugin.Custom.Android.CameraWithoutConfirmation
{
	public class NoConfirmTakePhotoVisitor : AndroidBaseVisitor, IVisitable
	{
		private ImageReader _imageReader;
		private readonly ImageReader.IOnImageAvailableListener _imageAvailableHandler;

		public NoConfirmTakePhotoVisitor()
		{
		}

		public NoConfirmTakePhotoVisitor(StoreMediaOptions options, OperationType cameraOperationType) : base(options, cameraOperationType) =>
			_imageAvailableHandler = new ImageAvailableHandler(this);

		/// <summary>
		/// Saves the image and returns it.
		/// </summary>
		/// <param name="data">Data related to taking photo.</param>
		/// <returns>Task&lt;MediaFile&gt;: Picked image.</returns>
		/// <inheritdoc />
		public override async Task<MediaFile> Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data)
		{
			await base.Visit(data);

			int width = ((PhotoCamera)Camera).LargestImageResolution.Width,
				height = ((PhotoCamera)Camera).LargestImageResolution.Height;
			_imageReader = ImageReader.NewInstance(width, height, ImageFormatType.Jpeg, 1);
			_imageReader.SetOnImageAvailableListener(_imageAvailableHandler, CameraBackgroundHandler);

			return await GetSavedPhoto();


			#region Local functions

			Task<MediaFile> GetSavedPhoto()   // Gets image which has been saved after capturing
			{
				var tcs = new TaskCompletionSource<MediaFile>();

				MediaPickerActivity.MediaPicked += ImageStoredHandler;

				return tcs.Task;

				#region Local functions

				void ImageStoredHandler(object sender, MediaPickedEventArgs args)
				{
					try
					{
						if (args.IsCanceled) tcs.SetResult(default(MediaFile));
						else if (args.Error != null) tcs.SetException(args.Error);
						else tcs.SetResult(args.Media);
					}
					finally
					{
						MediaPickerActivity.MediaPicked -= ImageStoredHandler;
					}
				}

				#endregion
			}

			#endregion
		}

		public override Task<MediaFile> Visit(IVisitableReturns visitable) => throw new ArgumentException(
			$"Parameter should be of type {nameof(ValueTuple<bool, Action<StoreMediaOptions>, IMedia>)}.", nameof(visitable));

		public override void Visit(IVisitable visitable) => throw new ArgumentException(
			$"Parameter should be of type {nameof(ValueTuple<bool, Action<StoreMediaOptions>, IMedia>)}.", nameof(visitable));

		public new void Accept(IVisitor visitor)
		{
			switch (visitor)
			{
				case null:
					throw new ArgumentNullException(nameof(visitor),
						$"Pass a suitable visitor to {nameof(NoConfirmTakePhotoVisitor)} for getting its private members.");
				case ImageAvailableHandler imageAvailableHandler:
					imageAvailableHandler.Visit(Camera.StoreOptions);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(visitor));
			}
		}
	}
}