using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Hardware.Camera2;
using Android.OS;
using Java.Lang;
using Plugin.CurrentActivity;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Extras;
using Plugin.Media.Extras.Abstractions;
using Plugin.Media.Extras.CameraWithoutConfirmation.Handlers;
using CameraChoice = Plugin.Media.Abstractions.CameraDevice;
using CameraDevice = Android.Hardware.Camera2.CameraDevice;

namespace Plugin.Media.Extras.CameraWithoutConfirmation
{
	public abstract class AndroidBaseVisitor : BaseVisitor, ICameraStateVisitor, ICameraActionVisitable
	{
		private readonly Activity _currentActivity = CrossCurrentActivity.Current.Activity;

		//+ Camera
		protected readonly CameraManager Manager;
		protected CameraDevice CameraDevice;
		private readonly CameraDeviceStateHandlers _cameraDeviceStateHandlers;

		//+ Camera properties
		protected readonly StoreCameraMediaOptions StoreOptions;
		protected string CameraId;
		protected CameraCharacteristics CameraCharacteristics;

		//+ Camera background threads
		private HandlerThread _cameraThread;
		protected internal Handler CameraBackgroundHandler;

		private readonly Semaphore _cameraOpenCloseLock = new Semaphore(1, 1);

		protected AndroidBaseVisitor()
		{
		}

		protected AndroidBaseVisitor(StoreMediaOptions options) : base(options)
		{
			StoreOptions = (StoreCameraMediaOptions)Options;

			Manager = (CameraManager)_currentActivity.GetSystemService(Context.CameraService);

			// Handlers
			_cameraDeviceStateHandlers = new CameraDeviceStateHandlers(this);
		}

		public override Task<MediaFile> Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data)
		{
			(bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) = data;

			if (!media.IsCameraAvailable) throw new NotSupportedException();

			if (!permission) return Task.FromResult<MediaFile>(null);

			verifyOptions(Options);

			FindCameraProperties(StoreOptions.DefaultCamera);

			Manager.OpenCamera(CameraId, _cameraDeviceStateHandlers, CameraBackgroundHandler);

			// ToDo: CameraDevice configuration

			// No need to return anything as it only prepares camera to be used by Child
			return Task.FromResult<MediaFile>(null);
		}

		public void Visit(CameraDevice cameraDevice) => CameraDevice = cameraDevice;

		private void FindCameraProperties(CameraChoice defaultCamera)
		{
			switch (defaultCamera)
			{
				case CameraChoice.Rear:
					FindCamCharacteristicsAndId(LensFacing.Back);
					break;
				case CameraChoice.Front:
					FindCamCharacteristicsAndId(LensFacing.Front);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(defaultCamera), defaultCamera,
						$"Only {CameraChoice.Front} & {CameraChoice.Rear} camera is supported.");
			}


			void FindCamCharacteristicsAndId(LensFacing lensFacing)
			{
				foreach (string camId in Manager.GetCameraIdList())
				{
					CameraCharacteristics cameraCharacteristics = Manager.GetCameraCharacteristics(camId);
					Integer facing = (Integer)cameraCharacteristics.Get(CameraCharacteristics.LensFacing);
					if (facing != null && facing == Integer.ValueOf((int)lensFacing))
					{
						CameraCharacteristics = cameraCharacteristics;
						CameraId = camId;
					}
				}
			}
		}

		private void StartBackGroundThread()
		{
			_cameraThread = new HandlerThread("CameraBackgroundThread");
			_cameraThread.Start();
			CameraBackgroundHandler = new Handler(_cameraThread.Looper);
		}

		private void StopBackgroundThread()
		{
			_cameraThread.QuitSafely();
			try
			{
				_cameraThread.Join();
				_cameraThread = null;
				CameraBackgroundHandler = null;
			}
			catch (InterruptedException e)
			{
				e.PrintStackTrace();
			}
		}

		public void Accept(ICameraActionVisitor visitor)
		{
			switch (visitor)
			{
				case CameraDeviceStateHandlers cameraDeviceStateHandlers:
					cameraDeviceStateHandlers.Visit(_cameraOpenCloseLock);
					break;
				default:
					visitor.Visit(this);
					break;
			}
		}
	}
}