﻿using System;
using Plugin.Media.Abstractions.Custom;
using DroidUri = Android.Net.Uri;

namespace Plugin.Media.Abstractions
{
	internal interface IPickerActivityVisitor : IVisitor
	{
		/// <summary>
		/// Gets <see cref="MediaPickerActivity"/> private members.
		/// </summary>
		/// <param name="newMediaFileUri">The new media file URI.</param>
		/// <param name="createMediaFile">The create media file.</param>
		/// <param name="convertUriToPath">The convert URI to path.</param>
		/// <param name="onMediaPicked">Method to raise Media picked event.</param>
		void Visit(out DroidUri newMediaFileUri, Action createMediaFile, Func<DroidUri, string> convertUriToPath,
			Action<MediaPickedEventArgs> onMediaPicked);
	}
}