using System;
using System.Threading.Tasks;
using Media.Plugin.Custom.Android.Factories;
using Media.Plugin.Custom.Android.Handlers;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;

namespace Media.Plugin.Custom.Android.Abstractions
{
	public abstract class AndroidBaseVisitor : BaseVisitor<Task<MediaFile>>, ICameraVisitor<Task<MediaFile>>,
		IVisitable
	{
		//+ Camera
		internal Camera Camera { get; }


		protected AndroidBaseVisitor()
		{
		}

		protected AndroidBaseVisitor(StoreMediaOptions options, OperationType cameraOperationType) :
			base(options, cameraOperationType) => Camera = CameraFactory.CreateCamera(CameraOperationType, options, this);

		/// <summary>
		/// Setup the camera.
		/// </summary>
		/// <param name="data">Data containing camera permission & camera options.</param>
		/// <returns>Doesn't return anything. Task&lt;MediaFile&gt; will be returned by child.</returns>
		/// <exception cref="System.NotSupportedException">Not supported exception when platform doesn't support camera.</exception>
		public virtual Task<MediaFile> Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data)
		{
			(bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) = data;
			

			// No need to return anything as it only prepares camera to be used by Child
			return Task.FromResult<MediaFile>(null);
		}

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
					// Passes AndroidBaseVisitor's private members to ImageAvailableHandler
					// ToDo: Pass AndroidBaseVisitor's private members to ImageAvailableHandler from Camera
					//pickerActivityVisitor.Visit(CameraBackgroundHandler, MediaPickerActivity);

					// Already called from ImageSaver.Run()
					// May cause bug as new URI will not be created for new image after getting URI for 1st image
					// MediaPickerActivity.Accept(pickerActivityVisitor); // Passes MediaPickerActivity's private members
					break;
				case CameraDeviceStateHandler cameraDeviceStateHandler:
					// ToDo: Pass lock to CameraDeviceStateHandler from Camera
					// Passes private members to CameraDeviceStateHandler
					// cameraDeviceStateHandler.Visit(CameraOpenCloseLock);
					break;
				default:
					visitor.Visit(this);
					break;
			}
		}

		#endregion
	}
}