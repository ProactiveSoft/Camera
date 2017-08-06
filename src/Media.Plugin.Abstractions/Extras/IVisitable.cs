using System.Threading.Tasks;

namespace Plugin.Media.Abstractions.Extras
{
	public interface IVisitable
	{
		Task<MediaFile> Accept(IVisitor visitor);
	}
}