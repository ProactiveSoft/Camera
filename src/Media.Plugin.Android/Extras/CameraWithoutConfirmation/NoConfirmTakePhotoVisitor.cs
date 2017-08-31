using System;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Extras;
using Plugin.Media.Extras.CameraWithoutConfirmation.Handlers;

namespace Plugin.Media.Extras.CameraWithoutConfirmation
{
	public class NoConfirmTakePhotoVisitor : AndroidBaseVisitor, IVisitable
	{
		private ImageReader _imageReader;
		private readonly ImageReader.IOnImageAvailableListener _imageAvailableHandler;

		public NoConfirmTakePhotoVisitor()
		{
		}

		public NoConfirmTakePhotoVisitor(StoreMediaOptions options) : base(options) =>
			_imageAvailableHandler = new ImageAvailableHandler(this);


		public override Task<MediaFile> Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data)
		{
			base.Visit(data);

			_imageReader = ImageReader.NewInstance(LargestImageResolution.Width, LargestImageResolution.Height,
				ImageFormatType.Jpeg, 1);
			_imageReader.SetOnImageAvailableListener(_imageAvailableHandler, CameraBackgroundHandler);

			return TakePhoto();


			#region Local functions
			
			Task<MediaFile> TakePhoto()
			{
				// ToDo: Send Capture command to camera
				// CameraCaptureSession.Capture(CaptureRequest)

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
					imageAvailableHandler.Visit(StoreOptions);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(visitor));
			}
		}
	}
}