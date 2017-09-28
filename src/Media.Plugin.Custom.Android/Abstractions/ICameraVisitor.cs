// ***********************************************************************
// Assembly         : Plugin.Media
// Author           : anila
// Created          : 09-01-2017
//
// Last Modified By : anila
// Last Modified On : 09-01-2017
// ***********************************************************************
// <copyright file="ICameraVisitor.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Media.Plugin.Custom.Android.Helpers.Parameters;
using Plugin.Media.Abstractions.Custom;

namespace Media.Plugin.Custom.Android.Abstractions
{
	/// <summary>
	/// Visitor to collect <see cref="Camera"/>'s private members
	/// </summary>
	/// <seealso cref="IVisitor" />
	internal interface ICameraVisitor : IVisitor
	{
		/// <summary>
		/// Gets <see cref="AndroidVisitor1"/>'s private members.
		/// </summary>
		/// <param name="parameters">Camera parameters.</param>
		void Visit(CameraParameters parameters);
	}
}