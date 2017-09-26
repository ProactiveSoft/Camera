// ***********************************************************************
// Assembly         : Media.Plugin.Custom.Android
// Author           : anila
// Created          : 09-06-2017
//
// Last Modified By : anila
// Last Modified On : 09-06-2017
// ***********************************************************************
// <copyright file="Camera.cs" company="Proso">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Media.Plugin.Custom.Android.Factories;
using Media.Plugin.Custom.Android.Handlers;
using Media.Plugin.Custom.Android.Helpers.EventArgs;
using Media.Plugin.Custom.Android.Helpers.Parameters;
using Media.Plugin.Custom.Android.States.Capture;
using Plugin.CurrentActivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;
using Boolean = Java.Lang.Boolean;
using CameraDevice = Android.Hardware.Camera2.CameraDevice;
using CameraChoice = Plugin.Media.Abstractions.CameraDevice;

namespace Media.Plugin.Custom.Android.Abstractions
{
	/// <inheritdoc cref="IVisitorRef" />
	/// <summary>
	/// Facade for Android's Camera2 Api.
	/// </summary>
	internal abstract class Camera : IVisitable, IVisitorRef
	{
		#region Fields & properties

		protected readonly IVisitable Visitable;

		// Undone: Make fields private & expose operations
		//++ Camera2's camera 
		/// <summary>
		/// The Camera's manager
		/// </summary>
		protected readonly CameraManager Manager;

		/// <summary>
		/// The camera device
		/// </summary>
		protected CameraDevice CameraDevice;

		protected CameraCaptureSession CameraCaptureSession;
		protected CaptureRequest.Builder CaptureRequestBuilder;

		//++ Camera handlers
		/// <summary>
		/// The camera device state handler
		/// </summary>
		protected readonly CameraDeviceStateHandler CameraDeviceStateHandler;
		protected readonly CameraCaptureSessionStateHandler CameraCaptureSessionStateHandler;
		protected readonly CameraCaptureSessionCaptureHandler<ICaptureState> CameraCaptureSessionCaptureHandler;
		protected readonly CameraCaptureSessionPhotoCaptureHandler CameraCaptureSessionPhotoCaptureHandler;

		//++ Camera properties
		/// <summary>
		/// The store options
		/// </summary>
		/// <value>The store options.</value>
		protected StoreCameraMediaOptions StoreOptions { get; }
		/// <summary>
		/// The camera identifier
		/// </summary>
		protected string CameraId;
		/// <summary>
		/// The camera characteristics
		/// </summary>
		protected CameraCharacteristics CameraCharacteristics;
		/// <summary>
		/// The flash supported
		/// </summary>
		protected bool FlashSupported;
		private readonly Dictionary<int, int> _orientations = new Dictionary<int, int>(4)
		{
			[(int)SurfaceOrientation.Rotation0] = 90,
			[(int)SurfaceOrientation.Rotation90] = 0,
			[(int)SurfaceOrientation.Rotation180] = 270,
			[(int)SurfaceOrientation.Rotation270] = 180
		};
		protected int SensorOrientation;
		protected CameraParameters CameraParameters;


		protected OperationType CameraOperationType;


		protected readonly MediaPickerActivity MediaPickerActivity = new MediaPickerActivity();

		#endregion


		#region Camera threading

		private HandlerThread _cameraThread;
		protected Handler CameraBackgroundHandler;

		protected readonly SemaphoreSlim CameraOpenCloseLock = new SemaphoreSlim(0, 1);

		/// <summary>
		/// Starts the back ground thread.
		/// </summary>
		private void StartBackGroundThread()
		{
			_cameraThread = new HandlerThread("CameraBackgroundThread");
			_cameraThread.Start();
			CameraBackgroundHandler = new Handler(_cameraThread.Looper);
		}

		/// <summary>
		/// Stops the background thread.
		/// </summary>
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

		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="Camera"/> class.
		/// </summary>
		/// <param name="storeOptions">The store options.</param>
		/// <param name="visitable">The visitable.</param>
		protected Camera(StoreMediaOptions storeOptions, IVisitable visitable)
		{
			Visitable = visitable;
			StoreOptions = storeOptions as StoreCameraMediaOptions;

			Manager = (CameraManager)CrossCurrentActivity.Current.Activity.GetSystemService(Context.CameraService);

			CameraDeviceStateHandler = new CameraDeviceStateHandler(this);
			CameraCaptureSessionStateHandler = new CameraCaptureSessionStateHandler(this);
			CameraCaptureSessionCaptureHandler = new CameraCaptureSessionCaptureHandler<ICaptureState>();
			CameraCaptureSessionPhotoCaptureHandler = new CameraCaptureSessionPhotoCaptureHandler(this, UnlockFocus);
		}

		#region Camera properties 

		/// <summary>
		/// Finds the camera properties.
		/// </summary>
		/// <param name="defaultCamera">The default camera.</param>
		/// <exception cref="ArgumentOutOfRangeException">defaultCamera</exception>
		private void FindCameraProperties(CameraChoice defaultCamera)
		{
			switch (defaultCamera)
			{
				case CameraChoice.Rear:
					Helper(LensFacing.Back);
					break;
				case CameraChoice.Front:
					Helper(LensFacing.Front);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(defaultCamera), defaultCamera,
						$"Only {CameraChoice.Front} & {CameraChoice.Rear} camera is supported.");
			}

			FindLargestResolution();


			void Helper(LensFacing lensFacing)
			{
				foreach (string camId in Manager.GetCameraIdList())
				{
					CameraCharacteristics cameraCharacteristics = Manager.GetCameraCharacteristics(camId);
					Integer facing = (Integer)cameraCharacteristics.Get(CameraCharacteristics.LensFacing);
					if (facing != null && facing == Integer.ValueOf((int)lensFacing))
					{
						CameraCharacteristics = cameraCharacteristics;
						CameraId = camId;
						SensorOrientation = (int)CameraCharacteristics.Get(CameraCharacteristics.SensorOrientation);

						// Check if flash is supported
						Boolean flashAvailable = (Boolean)CameraCharacteristics.Get(CameraCharacteristics.FlashInfoAvailable);
						FlashSupported = flashAvailable != null && (bool)flashAvailable;
					}
				}
			}
		}

		/// <summary>
		/// Finds the largest resolution.
		/// </summary>
		protected abstract void FindLargestResolution();

		/// <summary>
		/// Opens requested camera and creates <see cref="CameraCaptureSession"/>.
		/// </summary>
		/// <returns>Task&lt;CameraDevice&gt;: CameraDevice.</returns>
		private async Task<CameraDevice> OpenCamera()
		{
			var tcs = new TaskCompletionSource<CameraDevice>();

			CameraDeviceStateHandler.Opened += CameraStateHandler;
			CameraDeviceStateHandler.Error += CameraStateHandler;
			CameraDeviceStateHandler.Disconnected += CameraStateHandler;

			try
			{
				if (!await CameraOpenCloseLock.WaitAsync(2500))
					tcs.SetException(new RuntimeException("Time out waiting to lock camera opening."));
				else
					Manager.OpenCamera(CameraId, CameraDeviceStateHandler, CameraBackgroundHandler);
			}
			catch (CameraAccessException e)
			{
				e.PrintStackTrace();
				tcs.SetException(e);
			}
			catch (InterruptedException e)
			{
				tcs.SetException(new RuntimeException("Interrupted while trying to lock camera opening.", e));
			}

			return await tcs.Task;

			void CameraStateHandler(object sender, CameraDeviceStateEventArgs args)
			{
				try
				{
					tcs.SetResult(args.Camera);
				}
				finally
				{
					CameraDeviceStateHandler.Opened -= CameraStateHandler;
					CameraDeviceStateHandler.Error -= CameraStateHandler;
					CameraDeviceStateHandler.Disconnected -= CameraStateHandler;
				}
			}
		}

		protected abstract void CreateCameraCaptureSession();

		private Task<CameraCaptureSession> GetCameraCaptureSession()
		{
			var tcs = new TaskCompletionSource<CameraCaptureSession>();

			CameraCaptureSessionStateHandler.Configured += CameraCaptureSession_State;

			return tcs.Task;


			void CameraCaptureSession_State(object sender, CameraCaptureSessionStateEventArgs args)
			{
				try
				{
					tcs.SetResult(args.CameraCaptureSession);
				}
				finally
				{
					CameraCaptureSessionStateHandler.Configured -= CameraCaptureSession_State;
				}
			}
		}

		#endregion

		#region Camera preparations

		protected abstract void SetupMediaReader();

		protected void UnlockFocus()
		{
			try
			{
				CaptureRequestBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Cancel);
				SetAutoFlash(CaptureRequestBuilder);
				CameraCaptureSession.Capture(CaptureRequestBuilder.Build(), CameraCaptureSessionCaptureHandler,
					CameraBackgroundHandler);

				// Accepts current CaptureState & sets it to WaitingLock
				//-- Similar to:
				//-- ref ICaptureState captureState = ref CameraCaptureSessionCaptureHandler.Accept(this);
				//-- captureState = CaptureStateFactory.GetCaptureState(CaptureStates.WaitingLock);
				CameraCaptureSessionCaptureHandler.Accept(this) = CaptureStateFactory.GetCaptureState(CaptureStates.WaitingLock);
			}
			catch (CameraAccessException e)
			{
				e.PrintStackTrace();
			}
		}

		/// <summary>
		/// Closes the camera.
		/// </summary>
		/// <returns>Completed Task.</returns>
		protected virtual Task CloseCamera()
		{
			if (CameraCaptureSession != null)
			{
				CameraCaptureSession.Close();
				CameraCaptureSession = null;
			}

			if (CameraDevice != null)
			{
				CameraDevice.Close();
				CameraDevice = null;
			}

			return Task.CompletedTask;
		}

		#endregion

		#region Camera operations

		/// <summary>
		/// Takes photo or video depending on <see cref="OperationType"/>.
		/// </summary>
		/// <param name="data">Data for setting up camera.</param>
		/// <returns>Captured photo or video.</returns>
		internal virtual async Task<MediaFile> TakeMedia(
			(bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data)
		{
			(bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) = data;

			if (CameraCaptureSession == null)
			{
				(CameraDevice camera, CameraCaptureSession session) = await SetupCamera().ConfigureAwait(false);
				CameraDevice = camera;
				CameraCaptureSession = session;
			}

			return default; // No need to return anything as it only prepares camera to be used by Child

			// Camera setup using Template Method pattern
			async Task<(CameraDevice camera, CameraCaptureSession session)> SetupCamera()
			{
				#region Early camera setup

				if (!media.IsCameraAvailable) throw new NotSupportedException("OS doesn't support camera.");

				if (!permission) return default;

				verifyOptions(StoreOptions);

				try
				{
					FindCameraProperties(StoreOptions.DefaultCamera);

					FindLargestResolution();

					SetupMediaReader();
				}
				catch (CameraAccessException e)
				{
					e.PrintStackTrace();
				}
				catch (NullPointerException)
				{
					// Currently an NPE is thrown when the Camera2API is used but not supported on the
					// device this code runs.
					Toast.MakeText(CrossCurrentActivity.Current.Activity, "App cannot use Camera because camera's driver is old.",
						ToastLength.Long);
				}

				#endregion

				#region Camera setup

				CameraDevice camera = await OpenCamera().ConfigureAwait(false);
				CameraCaptureSession session = await GetCameraCaptureSession();

				return (camera, session);

				#endregion
			}
		}

		protected void SetAutoFlash(CaptureRequest.Builder captureRequestBuilder)
		{
			if (FlashSupported)
				captureRequestBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.OnAutoFlash);
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Retrieves the JPEG orientation from default screen rotation.
		/// </summary>
		/// <returns>Jpeg orientation.</returns>
		protected int GetOrientation()
		{
			// Sensor orientation is 90 for most devices, or 270 for some devices (eg. Nexus 5X)
			// We have to take that into account and rotate JPEG properly.
			// For devices with orientation of 90, we simply return our mapping from _orientations.
			// For devices with orientation of 270, we need to rotate the JPEG 180 degrees.
			int displayRotation = (int)CrossCurrentActivity.Current.Activity.WindowManager.DefaultDisplay.Rotation;
			return (_orientations[displayRotation] + SensorOrientation + 270) % 360;
		}

		#endregion

		#region Visitables

		/// <summary>
		/// Sends private members to required classes.
		/// </summary>
		/// <param name="visitor">Class which wants private members.</param>
		public virtual void Accept(IVisitor visitor)
		{
			switch (visitor)
			{
				case ICameraVisitor cameraVisitor:

					CameraParameters.CameraDevice = CameraDevice;
					CameraParameters.CameraOpenCloseLock = CameraOpenCloseLock;
					CameraParameters.CreateCameraCaptureSession = CreateCameraCaptureSession;

					cameraVisitor.Visit(CameraParameters);

					break;
				default:
					visitor.Visit(this);
					break;
			}
		}

		#endregion

		#region Visitors

		public void Visit(IVisitable visitable)
		{
		}

		/// <inheritdoc />
		/// <summary>
		/// Collect <see cref="T:Plugin.Media.Abstractions.Custom.IVisitableRef`1" />'s private data by reference.
		/// </summary>
		/// <typeparam name="TRequiredData">The type of the required data.</typeparam>
		/// <param name="requiredData">The required data.</param>
		/// <returns>
		/// Required data.
		/// </returns>
		public ref TRequiredData Visit<TRequiredData>(ref TRequiredData requiredData) => ref requiredData;

		#endregion
	}
}