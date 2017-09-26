using System;
using Android.Hardware.Camera2;
using Media.Plugin.Custom.Android.Abstractions;
using Media.Plugin.Custom.Android.Helpers.EventArgs;
using Media.Plugin.Custom.Android.Helpers.Parameters;
using Plugin.Media.Abstractions.Custom;

namespace Media.Plugin.Custom.Android.Handlers
{
	/// <summary>
	/// CameraCaptureSession Handler
	/// </summary>
	/// <seealso cref="CameraCaptureSession.StateCallback" />
	internal class CameraCaptureSessionStateHandler : CameraCaptureSession.StateCallback, ICameraVisitor
	{
		private readonly CameraCaptureSessionStateEventArgs _args = new CameraCaptureSessionStateEventArgs();

		internal event EventHandler<CameraCaptureSessionStateEventArgs> Configured;
		internal event EventHandler<CameraCaptureSessionStateEventArgs> ConfigureFailed;

		private readonly IVisitable _visitable;
		private CameraParameters _cameraParameters;

		public CameraCaptureSessionStateHandler(IVisitable visitable)
		{
			_visitable = visitable;
		}

		private void OnConfigured(object sender, CameraCaptureSessionStateEventArgs args) => Configured?.Invoke(sender, args);

		private void OnConfigureFailed(object sender, CameraCaptureSessionStateEventArgs args) =>
			ConfigureFailed?.Invoke(sender, args);

		public override void OnConfigured(CameraCaptureSession session)
		{
			_visitable.Accept(this);
			if (_cameraParameters.CameraDevice == null) return;

			_args.CameraCaptureSession = session;
			OnConfigured(this, _args);
		}

		public override void OnConfigureFailed(CameraCaptureSession session)
		{
			_args.CameraCaptureSession = session;
			OnConfigureFailed(this, _args);

			//Toast.MakeText(
			//	CrossCurrentActivity.Current.Activity, "CameraCaptureSession configuration failed.",
			//	ToastLength.Long);
		}

		#region Implementation of IVisitor

		/// <inheritdoc />
		public void Visit(IVisitable visitable) => throw new NotImplementedException();

		/// <inheritdoc />
		public void Visit(CameraParameters parameters) => _cameraParameters = parameters;

		#endregion
	}
}