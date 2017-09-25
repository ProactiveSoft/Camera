namespace Plugin.Media.Abstractions.Custom
{
	/// <summary>
	/// Visitor to collect <see cref="IVisitableRef{TRequiredData}" />'s private data by reference.
	/// </summary>
	/// <seealso cref="IVisitor" />
	public interface IVisitorRef : IVisitor
	{
		/// <summary>
		/// Collect <see cref="IVisitableRef{TRequiredData}" />'s private data by reference.
		/// </summary>
		/// <typeparam name="TRequiredData">The type of the required data.</typeparam>
		/// <param name="requiredData">The required data.</param>
		/// <returns>
		/// Required data.
		/// </returns>
		ref TRequiredData Visit<TRequiredData>(ref TRequiredData requiredData);
	}
}