// ***********************************************************************
// Assembly         : Plugin.Media.Abstractions
// Author           : anila
// Created          : 07-29-2017
//
// Last Modified By : anila
// Last Modified On : 09-16-2017
// ***********************************************************************
// <copyright file="IVisitable.cs" company="Plugin.Media.Abstractions">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace Plugin.Media.Abstractions.Custom
{
	/// <summary>
	/// Interface IVisitable
	/// </summary>
	public interface IVisitable
	{
		/// <summary>
		/// Accepts the specified visitor.
		/// </summary>
		/// <param name="visitor">The visitor.</param>
		void Accept(IVisitor visitor);
	}

	/// <inheritdoc />
	/// <summary>
	/// Interface IVisitableReturns
	/// </summary>
	/// <seealso cref="T:Plugin.Media.Abstractions.Custom.IVisitable" />
	public interface IVisitableReturns : IVisitable
	{
		/// <summary>
		/// Accepts the specified visitor.
		/// </summary>
		/// <typeparam name="T">Data type of returned type.</typeparam>
		/// <param name="visitor">The visitor.</param>
		/// <returns>T.</returns>
		T Accept<T>(IVisitor<T> visitor);
	}
}