using System;
using System.Drawing;
using System.Linq;
using AndroidSize = Android.Util.Size;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.OS;
using Android.Widget;
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
	public abstract class AndroidBaseVisitor : BaseVisitor<Task<MediaFile>>, ICameraVisitor<Task<MediaFile>>,
		IVisitable
	{
		private readonly Activity _currentActivity = CrossCurrentActivity.Current.Activity;

		//+ Camera
		protected readonly CameraManager Manager;
		protected CameraDevice CameraDevice;
		private readonly CameraDeviceStateHandler _cameraDeviceStateHandler;

		//+ Camera properties
		protected readonly StoreCameraMediaOptions StoreOptions;
		protected string CameraId;
		protected CameraCharacteristics CameraCharacteristics;
		protected Size LargestImageResolution;

		//+ Camera background thread
		private HandlerThread _cameraThread;
		protected internal Handler CameraBackgroundHandler;

		internal static readonly MediaPickerActivity MediaPickerActivity = new MediaPickerActivity();

		private readonly Semaphore _cameraOpenCloseLock = new Semaphore(1, 1);

		protected AndroidBaseVisitor()
		{
		}

		protected AndroidBaseVisitor(StoreMediaOptions options) : base(options)
		{
			StoreOptions = (StoreCameraMediaOptions)Options;

			Manager = (CameraManager)_currentActivity.GetSystemService(Context.CameraService);

			// Handlers
			_cameraDeviceStateHandler = new CameraDeviceStateHandler(this);

			// Unsubscribe in Dispose()
			_cameraDeviceStateHandler.Opened += CameraDeviceStateHandler_CameraStateChanged;
			_cameraDeviceStateHandler.Disconnected += CameraDeviceStateHandler_CameraStateChanged;
			_cameraDeviceStateHandler.Error += CameraDeviceStateHandler_CameraStateChanged;
		}

		/// <summary>
		/// Setup the camera.
		/// </summary>
		/// <param name="data">Data containing camera permission & camera options.</param>
		/// <returns>Doesn't return anything. Task&lt;MediaFile&gt; will be returned by child.</returns>
		/// <exception cref="System.NotSupportedException">Not supported exception when platform doesn't support camera.</exception>
		public virtual Task<MediaFile> Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data)
		{
			(bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) = data;

			try
			{
				if (!media.IsCameraAvailable) throw new NotSupportedException("OS doesn't support Camera.");

				if (!permission) return Task.FromResult<MediaFile>(null);

				verifyOptions(Options);

				FindCameraProperties(StoreOptions.DefaultCamera);

				Manager.OpenCamera(CameraId, _cameraDeviceStateHandler, CameraBackgroundHandler);


				StreamConfigurationMap map =
					(StreamConfigurationMap)CameraCharacteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
				AndroidSize[] imageSupportedSizesAndroid = map.GetOutputSizes((int)ImageFormatType.Jpeg);

				AndroidSize largestSizeAndroid = imageSupportedSizesAndroid
					.OrderByDescending(droidSize => (long)droidSize.Height * droidSize.Width)
					.FirstOrDefault();

				LargestImageResolution = new Size(largestSizeAndroid.Width, largestSizeAndroid.Height);
			}
			catch (CameraAccessException cameraAccessException)
			{
				cameraAccessException.PrintStackTrace();
			}
			catch (NullPointerException nullPointerException)
			{
				Toast.MakeText(_currentActivity, "App cannot use Camera because camera driver is old.", ToastLength.Long);
			}

			// No need to return anything as it only prepares camera to be used by Child
			return Task.FromResult<MediaFile>(null);
		}

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

		/// <summary>
		/// Gets the camera.
		/// </summary>
		/// <returns>Task&lt;CameraDevice&gt;: CameraDevice.</returns>
		protected Task<CameraDevice> GetCamera()
		{
			var tcs = new TaskCompletionSource<CameraDevice>();

			_cameraDeviceStateHandler.Opened += CameraDeviceStateHandler;
			_cameraDeviceStateHandler.Error += CameraDeviceStateHandler;
			_cameraDeviceStateHandler.Disconnected += CameraDeviceStateHandler;

			// ToDo: CameraDevice.OpenCamera()

			return tcs.Task;

			void CameraDeviceStateHandler(object sender, CameraDeviceStateEventArgs args)
			{
				try
				{
					tcs.SetResult(args.Camera);
				}
				finally
				{
					_cameraDeviceStateHandler.Opened -= CameraDeviceStateHandler;
					_cameraDeviceStateHandler.Error -= CameraDeviceStateHandler;
					_cameraDeviceStateHandler.Disconnected -= CameraDeviceStateHandler;
				}
			}
		}

		private void CameraDeviceStateHandler_CameraStateChanged(object sender, CameraDeviceStateEventArgs args) =>
			CameraDevice = args.Camera;

		#region Visitable

		/// <summary>
		/// Passes private members to handlers.
		/// </summary>
		/// <param name="visitor">Handler.</param>
		public void Accept(IVisitor visitor)
		{
			switch (visitor)
			{
				case IPickerActivityVisitor pickerActivityVisitor:
					pickerActivityVisitor.Visit(CameraBackgroundHandler, MediaPickerActivity);

					MediaPickerActivity.Accept(pickerActivityVisitor); // Passes MediaPickerActivity's private members
					break;
				case CameraDeviceStateHandler cameraDeviceStateHandler:
					// Passes private members to CameraDeviceStateHandler
					cameraDeviceStateHandler.Visit(_cameraOpenCloseLock);
					break;
				default:
					visitor.Visit(this);
					break;
			}
		}

		#endregion
	}
}