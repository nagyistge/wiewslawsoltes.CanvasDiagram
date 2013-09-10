
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
	#region IRenderer

	public interface IRenderer<TElement, TStyle>
	{
		void Initialize(TStyle style);
		void Render (object canvas, TElement element);
	}

	#endregion
}
