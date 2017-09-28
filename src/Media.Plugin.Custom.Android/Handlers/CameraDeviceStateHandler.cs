using System;
using System.Threading;
using Android.Hardware.Camera2;
using Media.Plugin.Custom.Android.Abstractions;
using Media.Plugin.Custom.Android.Helpers.EventArgs;
using Media.Plugin.Custom.Android.Helpers.Parameters;
using Plugin.Media.Abstractions.Custom;
using CameraDevice = Android.Hardware.Camera2.CameraDevice;

namespace Media.Plugin.Custom.Android.Handlers
{
	internal class CameraDeviceStateHandler : CameraDevice.StateCallback, ICameraVisitor
	{
		private SemaphoreSlim _cameraOpenCloseLock;
		private Action _createCameraCaptureSession;


		public CameraDeviceStateHandler(IVisitable visitable)
		{
			visitable.Accept(this);   // Gets AndroidVisitor1's private members
		}

		#region Camera States

		private readonly CameraDeviceStateEventArgs _cameraDeviceStateEventArgs = new CameraDeviceStateEventArgs();

		public override void OnOpened(CameraDevice camera)
		{
			_cameraOpenCloseLock.Release();

			_cameraDeviceStateEventArgs.Camera = camera;
			OnOpened(this, _cameraDeviceStateEventArgs);

			_createCameraCaptureSession();   // Creates CameraCaptureSession
		}

		public override void OnError(CameraDevice camera, CameraError error)
		{
			_cameraOpenCloseLock.Release();

			camera.Close();

			_cameraDeviceStateEventArgs.Camera = null;
			_cameraDeviceStateEventArgs.Error = error;
			OnError(this, _cameraDeviceStateEventArgs);

			// Undone: Stop Activity/Service
		}

		public override void OnDisconnected(CameraDevice camera)
		{
			_cameraOpenCloseLock.Release();

			camera.Close();

			_cameraDeviceStateEventArgs.Camera = null;
			OnDisconnedted(this, _cameraDeviceStateEventArgs);
		}

		public event EventHandler<CameraDeviceStateEventArgs> Opened;
		public event EventHandler<CameraDeviceStateEventArgs> Error;
		public event EventHandler<CameraDeviceStateEventArgs> Disconnected;

		public void OnOpened(object sender, CameraDeviceStateEventArgs args) => Opened?.Invoke(sender, args);
		public void OnError(object sender, CameraDeviceStateEventArgs args) => Error?.Invoke(sender, args);
		public void OnDisconnedted(object sender, CameraDeviceStateEventArgs args) => Disconnected?.Invoke(sender, args);

		#endregion

		#region Visitor

		/// <inheritdoc />
		/// <summary>
		/// Gets <see cref="Camera"/>'s private members.
		/// </summary>
		/// <param name="parameters"></param>
		public void Visit(CameraParameters parameters)
		{
			_cameraOpenCloseLock = parameters.CameraOpenCloseLock;
			_createCameraCaptureSession = parameters.CreateCameraCaptureSession;
		}

		public void Visit(IVisitable visitable)
		{

		}

		#endregion
	}
}