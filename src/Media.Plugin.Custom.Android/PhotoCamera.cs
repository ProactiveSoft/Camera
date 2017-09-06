using System.Drawing;
using System.Linq;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using DroidSize = Android.Util.Size;
using Media.Plugin.Custom.Android.CameraWithoutConfirmation.Handlers;
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

		internal PhotoCamera(StoreMediaOptions storeOptions, IVisitable visitable) : base(storeOptions, visitable)
		{
			_imageAvailableHandler = new ImageAvailableHandler(visitable);
		}

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
	}
}