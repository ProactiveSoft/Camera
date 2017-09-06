namespace Plugin.Media.Abstractions.Custom
{
	public interface IVisitable
	{
		void Accept(IVisitor visitor);
	}

	public interface IVisitableReturns : IVisitable
	{
		T Accept<T>(IVisitor<T> visitor);
	}
}