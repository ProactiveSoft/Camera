using System;
using System.Threading.Tasks;
using Android.OS;
using Media.Plugin.Custom.Android.Abstractions;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;

namespace Media.Plugin.Custom.Android
{
	/// <inheritdoc />
	/// <summary>
	/// Camera with video related features.
	/// </summary>
	internal class VideoCamera : Camera
	{
		internal VideoCamera(StoreMediaOptions storeOptions, IVisitable visitable) : base(storeOptions, visitable)
		{
		}

		protected override void FindLargestResolution()
		{
			throw new NotImplementedException();
		}

		internal override void SetupMediaReader(Handler cameraBackgroundHandler)
		{
			throw new NotImplementedException();
		}

		protected override void CreateCameraCaptureSession()
		{
			throw new NotImplementedException();
		}

		internal override void TakeMedia()
		{
			throw new NotImplementedException();
		}
	}
}