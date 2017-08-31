using System;
using System.Threading;
using Android.Hardware.Camera2;
using Plugin.Media.Abstractions.Extras;
using Plugin.Media.Extras.Abstractions;
using CameraDevice = Android.Hardware.Camera2.CameraDevice;

namespace Plugin.Media.Extras.CameraWithoutConfirmation.Handlers
{
	public class CameraDeviceStateHandler : CameraDevice.StateCallback, IVisitableReturns, IVisitor
	{
		private CameraDevice _cameraDevice;
		private AndroidBaseVisitor _androidBaseVisitor;

		private Semaphore _cameraOpenCloseLock;

		public CameraDeviceStateHandler(ICameraStateVisitor cameraStateVisitor)
		{
			Visit(cameraStateVisitor as AndroidBaseVisitor);

			_androidBaseVisitor.Accept(this);   // Gets AndroidBaseVisitor's private members
		}

		public override void OnDisconnected(CameraDevice camera)
		{
			throw new System.NotImplementedException();
		}

		public override void OnError(CameraDevice camera, CameraError error)
		{
			throw new System.NotImplementedException();
		}

		public override void OnOpened(CameraDevice camera)
		{
			_cameraDevice = camera;

			Accept(_androidBaseVisitor);
		}

		public T Accept<T>(IVisitor<T> visitor)
		{
			(visitor as ICameraStateVisitor).Visit(_cameraDevice);

			// ToDo: What to do about return type?
			// return T;
			throw new NotImplementedException();
		}

		public void Visit(ICameraActionVisitable visitable) => _androidBaseVisitor = visitable as AndroidBaseVisitor;
		public void Visit(Semaphore cameraOpenCloseLock) => _cameraOpenCloseLock = cameraOpenCloseLock;
		public void Accept(IVisitor visitor)
		{
			throw new System.NotImplementedException();
		}

		public void Visit(IVisitable visitable)
		{
			
		}
	}
}