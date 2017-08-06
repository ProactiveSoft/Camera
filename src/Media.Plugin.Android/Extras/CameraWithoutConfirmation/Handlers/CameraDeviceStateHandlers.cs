using System.Threading;
using System.Threading.Tasks;
using Android.Hardware.Camera2;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Extras;
using Plugin.Media.Extras.Abstractions;
using CameraDevice = Android.Hardware.Camera2.CameraDevice;

namespace Plugin.Media.Extras.CameraWithoutConfirmation.Handlers
{
	public class CameraDeviceStateHandlers : CameraDevice.StateCallback, IVisitable, ICameraActionVisitor
	{
		private CameraDevice _cameraDevice;
		private AndroidBaseVisitor _noConfirmTakePhotoVisitor;

		private Semaphore _cameraOpenCloseLock;

		public CameraDeviceStateHandlers(ICameraStateVisitor cameraStateVisitor) => Visit(
			cameraStateVisitor as AndroidBaseVisitor);

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

			_noConfirmTakePhotoVisitor.Accept(this);

			Accept(_noConfirmTakePhotoVisitor);
		}

		public Task<MediaFile> Accept(IVisitor visitor)
		{
			(visitor as ICameraStateVisitor).Visit(_cameraDevice);

			// ToDo: What to do about return type?
			return Task.FromResult<MediaFile>(null);
		}

		public void Visit(ICameraActionVisitable visitable) => _noConfirmTakePhotoVisitor = visitable as AndroidBaseVisitor;
		public void Visit(Semaphore cameraOpenCloseLock) => _cameraOpenCloseLock = cameraOpenCloseLock;
	}
}