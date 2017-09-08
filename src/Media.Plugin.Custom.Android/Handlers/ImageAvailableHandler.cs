using System;
using System.Threading.Tasks;
using Android.App;
using Android.Media;
using Android.OS;
using Java.IO;
using Java.Lang;
using Java.Nio;
using Media.Plugin.Custom.Android.Abstractions;
using Plugin.CurrentActivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;
using JavaObject = Java.Lang.Object;
using DroidUri = Android.Net.Uri;
using File = System.IO.File;

namespace Media.Plugin.Custom.Android.Handlers
{
	internal class ImageAvailableHandler : JavaObject, ImageReader.IOnImageAvailableListener, ICameraOptionsVisitor,
		IPickerActivityVisitor
	{
		private StoreCameraMediaOptions _storeOptions;

		#region AndroidBaseVisitor's members

		private MediaPickerActivity _mediaPickerActivity;
		private Handler _cameraBackgroundHandler;

		#endregion

		#region MediaPickerActivity's members

		private DroidUri _newMediaFileUri;
		private Action _createMediaFile;
		private Func<DroidUri, string> _convertUriToPath;
		private Action<MediaPickedEventArgs> _onMediaPicked;

		#endregion

		public ImageAvailableHandler(IVisitable visitable)
		{
			visitable.Accept(this); // Gets camera options from NoConfirmTakePhotoVisitor
			((AndroidBaseVisitor)visitable).Accept(this); // Gets private members of AndroidBaseVisitor
		}

		public void OnImageAvailable(ImageReader reader) =>
			_cameraBackgroundHandler.Post(new ImageSaver(this, reader.AcquireNextImage()));

		#region Visitors

		public void Visit(IVisitable visitable)
		{

		}

		/// <summary>
		/// Gets private members of <see cref="AndroidBaseVisitor" />.
		/// </summary>
		/// <param name="cameraBackgroundHandler">The camera background handler.</param>
		/// <param name="mediaPickerActivity">Activity to handle media picking.</param>
		/// <inheritdoc />
		public void Visit(Handler cameraBackgroundHandler, Activity mediaPickerActivity)
		{
			_cameraBackgroundHandler = cameraBackgroundHandler;
			_mediaPickerActivity = (MediaPickerActivity)mediaPickerActivity;
		}

		/// <inheritdoc />
		/// <summary>
		/// Gets <see cref="T:Plugin.Media.MediaPickerActivity"/>'s private members.
		/// </summary>
		/// <param name="newMediaFileUri">Uri of new file for storing media.</param>
		/// <param name="createMediaFile">Method to create media file.</param>
		/// <param name="convertUriToPath"></param>
		/// <param name="onMediaPicked">Method to raise Media picked event.</param>
		public void Visit(ref DroidUri newMediaFileUri, Action createMediaFile, Func<DroidUri, string> convertUriToPath,
			Action<MediaPickedEventArgs> onMediaPicked)
		{
			newMediaFileUri = MediaPickerActivity.GetOutputMediaFile(CrossCurrentActivity.Current.Activity,
				_storeOptions.Directory, _storeOptions.Name, true, false);
			_newMediaFileUri = newMediaFileUri;
			_createMediaFile = createMediaFile;
			_convertUriToPath = convertUriToPath;
			_onMediaPicked = onMediaPicked;
		}

		/// <summary>
		/// Gets private <see cref="T:Plugin.Media.Abstractions.StoreCameraMediaOptions" /> of classes.
		/// </summary>
		/// <param name="options">Camera options.</param>
		/// <inheritdoc />
		public void Visit(StoreMediaOptions options) => _storeOptions = options as StoreCameraMediaOptions;

		#endregion

		private class ImageSaver : JavaObject, IRunnable
		{
			private readonly ImageAvailableHandler _imageAvailableHandler;
			private readonly Image _image;

			public ImageSaver(ImageAvailableHandler imageAvailableHandler, Image image)
			{
				_imageAvailableHandler = imageAvailableHandler;
				_image = image;
			}

			public void Run()
			{
				// Gets MediaPickerActivity's private members
				_imageAvailableHandler._mediaPickerActivity.Accept(_imageAvailableHandler);
				_imageAvailableHandler._createMediaFile();

				StoreImage();

				NotifyImageStored();

				#region Local functions

				void StoreImage() // Stores captured image to specified location
				{
					ByteBuffer buffer = _image.GetPlanes()[0].Buffer;
					byte[] bytes = new byte[buffer.Remaining()];
					buffer.Get(bytes);
					using (var fileOutputStream =
						new FileOutputStream(_imageAvailableHandler._convertUriToPath(_imageAvailableHandler._newMediaFileUri), true))
					{
						try
						{
							fileOutputStream.Write(bytes);
						}
						catch (IOException e)
						{
							e.PrintStackTrace();
						}
						finally
						{
							_image.Close();
						}
					}
				}

				async Task NotifyImageStored()
				{
					MediaPickedEventArgs args = GetMediaFile();

					await Task.Delay(1).ConfigureAwait(true);

					_imageAvailableHandler._onMediaPicked(args);
				}

				MediaPickedEventArgs GetMediaFile()
				{
					string albumPath = _imageAvailableHandler._newMediaFileUri.ToString(),
						resultPath = _imageAvailableHandler._convertUriToPath(_imageAvailableHandler._newMediaFileUri);

					if (resultPath != null && File.Exists(resultPath))
					{
						var mediaFile = new MediaFile(resultPath, () => File.OpenRead(resultPath), albumPath);
						return new MediaPickedEventArgs(default, false, mediaFile);
					}

					return new MediaPickedEventArgs(default, new MediaFileNotFoundException(albumPath));
				}

				#endregion
			}
		}
	}
}