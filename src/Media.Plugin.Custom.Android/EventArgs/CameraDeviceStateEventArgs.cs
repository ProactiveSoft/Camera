using Android.Hardware.Camera2;

namespace Media.Plugin.Custom.Android.EventArgs
{
	internal class CameraDeviceStateEventArgs : System.EventArgs
	{
		public CameraDevice Camera { get; set; }
		public CameraError Error { get; set; }
	}
}