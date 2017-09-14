using System;
using System.Threading;

namespace Media.Plugin.Custom.Android.Helpers.Parameters
{
	internal abstract class CameraParameters
	{
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