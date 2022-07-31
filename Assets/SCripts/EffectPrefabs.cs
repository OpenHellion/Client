using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectPrefabs", menuName = "Effects/EffectPrefabs")]
public class EffectPrefabs : ScriptableObject
{
	public List<BulletImpact> BulletHitsMetal;

	public List<BulletImpact> BulletHitsFlesh;

	public List<BulletImpact> BulletHitsObject;

	public BulletImpact BulletHitMetal
	{
		get
		{
			return BulletHitsMetal[Random.Range(0, BulletHitsMetal.Count)];
		}
	}

	public BulletImpact BulletHitFlesh
	{
		get
		{
			return BulletHitsFlesh[Random.Range(0, BulletHitsFlesh.Count)];
		}
	}

	public BulletImpact BulletHitObject
	{
		get
		{
			return BulletHitsObject[Random.Range(0, BulletHitsObject.Count)];
		}
	}
}
