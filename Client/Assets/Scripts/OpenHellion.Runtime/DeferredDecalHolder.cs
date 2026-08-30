using System.Collections.Generic;

public class DeferredDecalHolder
{
	private static DeferredDecalHolder myDecalHolder;

	internal HashSet<Decal> myDecals = new HashSet<Decal>();

	internal static DeferredDecalHolder instance
	{
		get
		{
			if (myDecalHolder == null)
			{
				myDecalHolder = new DeferredDecalHolder();
			}

			return myDecalHolder;
		}
	}

	internal void AddDecal(Decal aDecal)
	{
		myDecals.Add(aDecal);
	}

	internal void RemoveDecal(Decal aDecal)
	{
		myDecals.Remove(aDecal);
	}
}
