using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Views;
using Media.Plugin.Custom.Android.Factories;
using Media.Plugin.Custom.Android.Handlers;
using Media.Plugin.Custom.Android.Helpers.Parameters;
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
		private Size _largestImageResolution; 

		private ImageReader _imageReader;
		private readonly ImageReader.IOnImageAvailableListener _imageAvailableHandler;

		internal PhotoCamera(StoreMediaOptions storeOptions, IVisitable visitable) : base(storeOptions, visitable)
		{
			CameraOperationType = OperationType.Photo;

			_imageAvailableHandler = new ImageAvailableHandler(visitable);

			CameraParameters = ComputerParametersFactory.CreateCameraParameters(CameraOperationType);
		}

		protected override void FindLargestResolution()
		{
			StreamConfigurationMap map =
				(StreamConfigurationMap)CameraCharacteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
			DroidSize[] imageSupportedSizesAndroid = map.GetOutputSizes((int)ImageFormatType.Jpeg);

			DroidSize largestSizeAndroid = imageSupportedSizesAndroid
				.OrderByDescending(droidSize => (long)droidSize.Height * droidSize.Width)
				.FirstOrDefault();

			_largestImageResolution = new Size(largestSizeAndroid.Width, largestSizeAndroid.Height);
		}

		protected override void SetupMediaReader()
		{
			_imageReader = ImageReader.NewInstance(_largestImageResolution.Width, _largestImageResolution.Height,
				ImageFormatType.Jpeg, 1);
			_imageReader.SetOnImageAvailableListener(_imageAvailableHandler, CameraBackgroundHandler);
		}

		protected override void CreateCameraCaptureSession()
		{
			try
			{
				var surfaces = new List<Surface>
				{
					_imageReader.Surface
				};

				CameraDevice.CreateCaptureSession(surfaces, CameraCaptureSessionHandler, null);
			}
			catch (CameraAccessException e)
			{
				e.PrintStackTrace();
			}
		}

		#region Camera operations

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

		#region Visitables

		///// <inheritdoc />
		///// <summary>
		///// Sends private members to required classes.
		///// </summary>
		///// <param name="visitor">Class which wants private members.</param>
		//public override void Accept(IVisitor visitor)
		//{
		//	switch (visitor)
		//	{
		//		default:
		//			visitor.Visit(this);
		//			break;
		//	}

		//	base.Accept(visitor);   // Gets private members of Camera
		//}

		#endregion
	}
}