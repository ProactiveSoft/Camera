using System;
using Android.Hardware.Camera2;

namespace Media.Plugin.Custom.Android.CameraWithoutConfirmation.Handlers
{
	internal class CameraDeviceStateEventArgs : EventArgs
	{
		public CameraDevice Camera { get; set; }
		public CameraError Error { get; set; }
	}
}