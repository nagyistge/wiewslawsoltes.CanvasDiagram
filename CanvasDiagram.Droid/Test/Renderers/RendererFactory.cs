
#region References

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using Android.Graphics;
using CanvasDiagram.Core.Test;

#endregion

namespace CanvasDiagram.Droid.Renderers
{
	#region Android RendererFactory

	public class RendererFactory : IRendererFactory
	{
		public string RenererSuffix { get; private set; }      // "Renderer"
		public string RenderersNamespace { get; private set; } // "CanvasDiagram.Droid.Renderers"

		public RendererFactory (string renererSuffix, string renderersNamespace)
		{
			RenererSuffix = renererSuffix;
			RenderersNamespace = renderersNamespace;
		}

		public Func<IRenderer<TElement, TStyle>> GetActivator<TElement, TStyle>()
		{
			Type renderer = Type.GetType(string.Concat(RenderersNamespace, '.', typeof(TElement).Name, RenererSuffix));
			ConstructorInfo constructor = renderer.GetConstructor(Type.EmptyTypes);

			Func<IRenderer<TElement, TStyle>> activator = (Func<IRenderer<TElement, TStyle>>)
			(
				Expression.Lambda 
				(
					Expression.Convert 
					(
						Expression.New (constructor), 
						typeof(IRenderer<TElement, TStyle>)
					)
				).Compile ()
			);

			return activator;
		}
	}

	#endregion
}
