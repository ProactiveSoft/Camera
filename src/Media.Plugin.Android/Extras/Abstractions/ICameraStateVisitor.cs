using Android.Hardware.Camera2;
using Plugin.Media.Abstractions.Extras;

namespace Plugin.Media.Extras.Abstractions
{
	public interface ICameraStateVisitor : IVisitor
	{
		void Visit(CameraDevice cameraDevice);
	}
}