using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Media.Plugin.Custom.Android.Handlers;
using DroidSize = Android.Util.Size;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;
using Camera = Media.Plugin.Custom.Android.Abstractions.Camera;

namespace Media.Plugin.Custom.Android
{
	/// <inheritdoc />
	/// <summary>
	/// Camera with photo related features.
	/// </summary>
	internal class PhotoCamera : Camera
	{
		//+ Camera properties
		internal Size LargestImageResolution { private set; get; }

		private ImageReader _imageReader;
		private readonly ImageReader.IOnImageAvailableListener _imageAvailableHandler;

		internal PhotoCamera(StoreMediaOptions storeOptions, IVisitable visitable) : base(storeOptions, visitable) =>
			_imageAvailableHandler = new ImageAvailableHandler(visitable);

		protected override void FindLargestResolution()
		{
			StreamConfigurationMap map =
				(StreamConfigurationMap)CameraCharacteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
			DroidSize[] imageSupportedSizesAndroid = map.GetOutputSizes((int)ImageFormatType.Jpeg);

			DroidSize largestSizeAndroid = imageSupportedSizesAndroid
				.OrderByDescending(droidSize => (long)droidSize.Height * droidSize.Width)
				.FirstOrDefault();

			LargestImageResolution = new Size(largestSizeAndroid.Width, largestSizeAndroid.Height);
		}

		//Undone: Call it from Service.Start()
		internal override void SetupMediaReader(Handler cameraBackgroundHandler)
		{
			_imageReader = ImageReader.NewInstance(LargestImageResolution.Width, LargestImageResolution.Height,
				ImageFormatType.Jpeg, 1);
			_imageReader.SetOnImageAvailableListener(_imageAvailableHandler, cameraBackgroundHandler);
		}

		#region Camera operations

		protected override void CreateCameraCaptureSession()
		{
			throw new System.NotImplementedException();
		}

		internal override void TakeMedia() => LockFocus();

		private void LockFocus()
		{
			
		}

		private void RunPreCaptureSequence()
		{
			
		}

		private void CaptureStillPhoto()
		{
			
		}

		private void UnlockFocus()
		{
			
		}

		#endregion
	}
}