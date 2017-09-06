using System;
using Media.Plugin.Custom.Android.Abstractions;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;

namespace Media.Plugin.Custom.Android
{
	/// <summary>
	/// Camera with video related features.
	/// </summary>
	internal class VideoCamera : Camera
	{
		internal VideoCamera(StoreMediaOptions storeOptions, IVisitable visitable) : base(storeOptions, visitable)
		{
		}

		protected override void FindLargestResolution()
		{
			throw new NotImplementedException();
		}
	}
}