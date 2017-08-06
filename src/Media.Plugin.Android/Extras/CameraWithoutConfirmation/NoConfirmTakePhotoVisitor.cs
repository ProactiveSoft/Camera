using System;
using System.Threading.Tasks;
using Android.Media;
using Plugin.Media.Abstractions;

namespace Plugin.Media.Extras.CameraWithoutConfirmation
{
	public class NoConfirmTakePhotoVisitor : AndroidBaseVisitor
	{
		private ImageReader _imageReader;

		public NoConfirmTakePhotoVisitor()
		{	
		}

		public NoConfirmTakePhotoVisitor(StoreMediaOptions options) : base(options)
		{
		}

		public override Task<MediaFile> Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data)
		{
			base.Visit(data);

			// ToDo: ImageReader configuration


			throw new NotImplementedException();   // ToDo: Return MediaFile after capturing pic
		}
	}
}