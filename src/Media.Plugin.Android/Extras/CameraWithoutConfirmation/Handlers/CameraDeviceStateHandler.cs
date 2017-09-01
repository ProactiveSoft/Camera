using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Hardware.Camera2;
using Plugin.Media.Abstractions.Extras;
using Plugin.Media.Extras.Abstractions;
using CameraDevice = Android.Hardware.Camera2.CameraDevice;

namespace Plugin.Media.Extras.CameraWithoutConfirmation.Handlers
{
	internal class CameraDeviceStateHandler : CameraDevice.StateCallback, IAndroidBaseVisitor
	{
		private readonly IVisitable _visitable;

		private Semaphore _cameraOpenCloseLock;

		public CameraDeviceStateHandler(IVisitable visitable)
		{
			_visitable = visitable;

			((AndroidBaseVisitor)_visitable).Accept(this);   // Gets AndroidBaseVisitor's private members
		}

		#region Camera States

		private readonly CameraDeviceStateEventArgs _cameraDeviceStateEventArgs = new CameraDeviceStateEventArgs();

		public override void OnOpened(CameraDevice camera)
		{
			_cameraOpenCloseLock.Release();

			_cameraDeviceStateEventArgs.Camera = camera;
			OnOpened(this, _cameraDeviceStateEventArgs);
		}

		public override void OnError(CameraDevice camera, CameraError error)
		{
			_cameraOpenCloseLock.Release();

			camera.Close();

			_cameraDeviceStateEventArgs.Camera = null;
			_cameraDeviceStateEventArgs.Error = error;
			OnError(this, _cameraDeviceStateEventArgs);
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
		/// Gets <see cref="T:Plugin.Media.Extras.CameraWithoutConfirmation.AndroidBaseVisitor" />'s private members.
		/// </summary>
		/// <param name="cameraOpenCloseLock">The camera open close lock.</param>
		public void Visit(Semaphore cameraOpenCloseLock) => _cameraOpenCloseLock = cameraOpenCloseLock;

		public void Visit(IVisitable visitable)
		{

		}		
		
		#endregion
	}

	internal class CameraDeviceStateEventArgs : EventArgs
	{
		public CameraDevice Camera { get; set; }
		public CameraError Error { get; set; }
	}
}