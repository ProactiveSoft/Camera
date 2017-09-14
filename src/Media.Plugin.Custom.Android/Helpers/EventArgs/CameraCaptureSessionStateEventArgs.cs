using Android.Hardware.Camera2;

namespace Media.Plugin.Custom.Android.Helpers.EventArgs
{
	internal class CameraCaptureSessionStateEventArgs : System.EventArgs
	{
		public CameraCaptureSession CameraCaptureSession { get; set; }
	}
}