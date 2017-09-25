namespace Plugin.Media.Abstractions.Custom
{
	/// <summary>
	/// Visitable containing required data by <see cref="IVisitorRef" />.
	/// </summary>
	/// <typeparam name="TRequiredData">The type of the required data.</typeparam>
	/// <seealso cref="IVisitable" />
	public interface IVisitableRef<TRequiredData> : IVisitable
	{
		/// <summary>
		/// Transfers required data to <see cref="IVisitorRef" />.
		/// </summary>
		/// <param name="visitor">
		///   <see cref="IVisitorRef" /> requiring private data.</param>
		/// <returns>
		/// Private data required by <see cref="IVisitorRef" />.
		/// </returns>
		ref TRequiredData Accept(IVisitorRef visitor);
	}
}