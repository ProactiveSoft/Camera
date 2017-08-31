using System.Threading.Tasks;

namespace Plugin.Media.Abstractions.Extras
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