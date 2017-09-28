using System;
using System.Threading.Tasks;
using Media.Plugin.Custom.Android.Abstractions;
using Media.Plugin.Custom.Android.Factories;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;

namespace Media.Plugin.Custom.Android
{
	public class AndroidVisitor1 : BaseVisitor<Task<MediaFile>>, IMediaVisitor<Task<MediaFile>>
	{
		internal Camera Camera { get; }
		
		protected AndroidVisitor1(StoreMediaOptions options, OperationType cameraOperationType) :
			base(options, cameraOperationType) => Camera = CameraFactory.CreateCamera(CameraOperationType, options);

		#region Visitors

		/// <summary>
		/// Takes photo or video as requested.
		/// </summary>
		/// <param name="data">Data containing camera permission & camera options.</param>
		/// <returns>Doesn't return anything. Task&lt;MediaFile&gt; will be returned by child.</returns>
		/// <exception cref="System.NotSupportedException">Not supported exception when platform doesn't support camera.</exception>
		public virtual Task<MediaFile> Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data) =>
			Camera.TakeMedia(data);

		/// <inheritdoc cref="IVisitor{T}.Visit(IVisitableReturns)" />
		public override Task<MediaFile> Visit(IVisitableReturns visitable)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}