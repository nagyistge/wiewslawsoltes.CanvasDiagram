
#region References

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

#endregion

namespace CanvasDiagram.Core.Test
{
	#region IRendererFactory

	public interface IRendererFactory
	{
		Func<IRenderer<TElement, TStyle>> GetActivator<TElement, TStyle>();
	}

	#endregion
}
