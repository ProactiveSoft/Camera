using System;
using Android.Hardware.Camera2;
using Android.Widget;
using Media.Plugin.Custom.Android.Helpers.EventArgs;
using Plugin.CurrentActivity;

namespace Media.Plugin.Custom.Android.Handlers
{
	/// <summary>
	/// CameraCaptureSession Handler
	/// </summary>
	/// <seealso cref="CameraCaptureSession.StateCallback" />
	internal class CameraCaptureSessionHandler : CameraCaptureSession.StateCallback
	{
		private readonly CameraCaptureSessionStateEventArgs _args = new CameraCaptureSessionStateEventArgs();

		internal event EventHandler<CameraCaptureSessionStateEventArgs> Configured;
		internal event EventHandler<CameraCaptureSessionStateEventArgs> ConfigureFailed;

		private void OnConfigured(object sender, CameraCaptureSessionStateEventArgs args) => Configured?.Invoke(sender, args);

		private void OnConfigureFailed(object sender, CameraCaptureSessionStateEventArgs args) =>
			ConfigureFailed?.Invoke(sender, args);

		public override void OnConfigured(CameraCaptureSession session)
		{
			_args.CameraCaptureSession = session;
			OnConfigured(this, _args);
		}

		public override void OnConfigureFailed(CameraCaptureSession session)
		{
			_args.CameraCaptureSession = session;
			OnConfigureFailed(this, _args);

			Toast.MakeText(
				CrossCurrentActivity.Current.Activity, "CameraCaptureSession configuration failed.",
				ToastLength.Long);
		}
	}
}