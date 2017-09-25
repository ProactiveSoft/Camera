using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.Views;
using Java.Lang;
using Media.Plugin.Custom.Android.Abstractions;
using Media.Plugin.Custom.Android.Factories;
using Media.Plugin.Custom.Android.Handlers;
using Media.Plugin.Custom.Android.States.Capture;
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

			// Factory's instance created to store itself in static field
			new CaptureStateFactory(RunPreCaptureSequence, CaptureStillPhoto);

			_imageAvailableHandler = new ImageAvailableHandler(StoreOptions, CameraBackgroundHandler, MediaPickerActivity);

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

		#region Camera preparations

		private void LockFocus()
		{
			try
			{
				CaptureRequestBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Start);
				// Tell CameraCaptureSessionCaptureHandler to wait for the lock
				ref ICaptureState captureState = ref CameraCaptureSessionCaptureHandler.Accept(this);
				captureState = CaptureStateFactory.GetCaptureState(CaptureStates.WaitingLock);
				CameraCaptureSession.Capture(CaptureRequestBuilder.Build(), CameraCaptureSessionCaptureHandler,
					CameraBackgroundHandler);
			}
			catch (CameraAccessException e)
			{
				e.PrintStackTrace();
			}
		}

		private void RunPreCaptureSequence()
		{
			try
			{
				CaptureRequestBuilder.Set(CaptureRequest.ControlAePrecaptureTrigger, (int)ControlAEPrecaptureTrigger.Start);
				// Tell CameraCaptureSessionCaptureHandler to wait for precapture sequence to be set
				ref ICaptureState captureState = ref CameraCaptureSessionCaptureHandler.Accept(this);
				captureState = CaptureStateFactory.GetCaptureState(CaptureStates.WaitingPrecapture);
				CameraCaptureSession.Capture(CaptureRequestBuilder.Build(), CameraCaptureSessionCaptureHandler,
					CameraBackgroundHandler);
			}
			catch (CameraAccessException e)
			{
				e.PrintStackTrace();
			}
		}

		private void CaptureStillPhoto()
		{
			try
			{
				if (CameraDevice == null) return;

				CaptureRequest.Builder requestBuilder = CameraDevice.CreateCaptureRequest(CameraTemplate.StillCapture);

				requestBuilder.AddTarget(_imageReader.Surface);   // Add surface to request

				// Add settings to request
				requestBuilder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.ContinuousPicture);
				SetAutoFlash(requestBuilder);
				requestBuilder.Set(CaptureRequest.JpegOrientation, GetOrientation());

				CameraCaptureSession.Capture(requestBuilder.Build(), CameraCaptureSessionPhotoCaptureHandler, null);
			}
			catch (CameraAccessException e)
			{
				e.PrintStackTrace();
			}
		}

		// Undone: Call from Dispose()
		/// <inheritdoc />
		protected override async Task CloseCamera()
		{
			try
			{
				await CameraOpenCloseLock.WaitAsync().ConfigureAwait(false);

				await base.CloseCamera().ConfigureAwait(false);

				if (_imageReader != null)
				{
					_imageReader.Close();
					_imageReader = null;
				}
			}
			catch (InterruptedException e)
			{
				throw new RuntimeException("Interrupted while trying to lock camera closing.", e);
			}
			finally
			{
				CameraOpenCloseLock.Release();
			}
		}

		#region Overrides of Camera

		#endregion

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