using System;
using System.Collections;

namespace ProBuilder2.Common
{
	public class pb_WingedEdgeEnumerator : IEnumerator
	{
		private pb_WingedEdge _start;

		private pb_WingedEdge _current;

		object IEnumerator.Current => Current;

		public pb_WingedEdge Current
		{
			get
			{
				try
				{
					return _current;
				}
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();
				}
			}
		}

		public pb_WingedEdgeEnumerator(pb_WingedEdge start)
		{
			_start = start;
			_current = null;
		}

		public bool MoveNext()
		{
			if (_current == null)
			{
				_current = _start;
				return _current != null;
			}
			_current = _current.next;
			return _current != null && _current != _start;
		}

		public void Reset()
		{
			_current = null;
		}
	}
}
