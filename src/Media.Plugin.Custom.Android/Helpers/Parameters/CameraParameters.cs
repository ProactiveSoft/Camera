using System;
using System.Threading;
using Android.Hardware.Camera2;

namespace Media.Plugin.Custom.Android.Helpers.Parameters
{
	internal abstract class CameraParameters
	{
		public CameraDevice CameraDevice { get; set; }
		internal SemaphoreSlim CameraOpenCloseLock { get; set; }
		internal Action CreateCameraCaptureSession { get; set; }
	}

	internal class PhotoCameraParameters : CameraParameters
	{	
	}

	internal class VideoCameraParameters : CameraParameters
	{
	}
}