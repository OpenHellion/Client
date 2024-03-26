using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThreeEyedGames
{
	[ExecuteInEditMode]
	public class DecaliciousRenderer : MonoBehaviour
	{
		[HideInInspector] public bool UseInstancing = true;

		protected CommandBuffer _bufferDeferred;

		protected CommandBuffer _bufferUnlit;

		protected CommandBuffer _bufferLimitTo;

		protected SortedDictionary<int, Dictionary<Material, HashSet<Decalicious>>> _deferredDecals;

		protected SortedDictionary<int, Dictionary<Material, HashSet<Decalicious>>> _unlitDecals;

		protected List<MeshRenderer> _limitToMeshRenderers;

		protected List<SkinnedMeshRenderer> _limitToSkinnedMeshRenderers;

		protected HashSet<GameObject> _limitToGameObjects;

		protected List<Decal> _decalComponent;

		protected List<MeshFilter> _meshFilterComponent;

		protected Camera _camera;

		protected bool _camLastKnownHDR;

		protected static Mesh _cubeMesh = null;

		protected Matrix4x4[] _matrices;

		protected float[] _fadeValues;

		protected float[] _limitToValues;

		protected MaterialPropertyBlock _instancedBlock;

		protected MaterialPropertyBlock _directBlock;

		protected RenderTargetIdentifier[] _albedoRenderTarget;

		protected RenderTargetIdentifier[] _normalRenderTarget;

		protected Material _materialLimitToGameObjects;

		protected static Vector4[] _avCoeff = new Vector4[7];

		private void OnEnable()
		{
			_deferredDecals = new SortedDictionary<int, Dictionary<Material, HashSet<Decalicious>>>();
			_unlitDecals = new SortedDictionary<int, Dictionary<Material, HashSet<Decalicious>>>();
			_limitToMeshRenderers = new List<MeshRenderer>();
			_limitToSkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
			_limitToGameObjects = new HashSet<GameObject>();
			_decalComponent = new List<Decal>();
			_meshFilterComponent = new List<MeshFilter>();
			_matrices = new Matrix4x4[1023];
			_fadeValues = new float[1023];
			_limitToValues = new float[1023];
			_instancedBlock = new MaterialPropertyBlock();
			_directBlock = new MaterialPropertyBlock();
			_camera = GetComponent<Camera>();
			_cubeMesh = Resources.Load<Mesh>("DecalCube");
			_normalRenderTarget = new RenderTargetIdentifier[2]
			{
				BuiltinRenderTextureType.GBuffer1,
				BuiltinRenderTextureType.GBuffer2
			};
		}

		private void OnDisable()
		{
			if (_bufferDeferred != null)
			{
				GetComponent<Camera>().RemoveCommandBuffer(CameraEvent.BeforeReflections, _bufferDeferred);
				_bufferDeferred = null;
			}

			if (_bufferUnlit != null)
			{
				GetComponent<Camera>().RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, _bufferUnlit);
				_bufferUnlit = null;
			}

			if (_bufferLimitTo != null)
			{
				GetComponent<Camera>().RemoveCommandBuffer(CameraEvent.AfterGBuffer, _bufferLimitTo);
				_bufferLimitTo = null;
			}
		}

		private void OnPreRender()
		{
			if (!SystemInfo.supportsInstancing)
			{
				UseInstancing = false;
			}

			if (_albedoRenderTarget == null || _camera.allowHDR != _camLastKnownHDR)
			{
				_camLastKnownHDR = _camera.allowHDR;
				_albedoRenderTarget = new RenderTargetIdentifier[2]
				{
					BuiltinRenderTextureType.GBuffer0,
					(!_camLastKnownHDR) ? BuiltinRenderTextureType.GBuffer3 : BuiltinRenderTextureType.CameraTarget
				};
			}

			CreateBuffer(ref _bufferDeferred, _camera, "Decalicious - Deferred", CameraEvent.BeforeReflections);
			CreateBuffer(ref _bufferUnlit, _camera, "Decalicious - Unlit", CameraEvent.BeforeImageEffectsOpaque);
			CreateBuffer(ref _bufferLimitTo, _camera, "Decalicious - Limit To Game Objects", CameraEvent.AfterGBuffer);
			_bufferLimitTo.Clear();
			DrawLimitToGameObjects(_camera);
			_bufferDeferred.Clear();
			DrawDeferredDecals_Albedo(_camera);
			DrawDeferredDecals_NormSpecSmooth(_camera);
			_bufferUnlit.Clear();
			DrawUnlitDecals(_camera);
			SortedDictionary<int, Dictionary<Material, HashSet<Decalicious>>>.Enumerator enumerator =
				_deferredDecals.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Value.Clear();
			}

			enumerator = _unlitDecals.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Value.Clear();
			}

			_limitToGameObjects.Clear();
		}

		private void DrawLimitToGameObjects(Camera cam)
		{
			if (_limitToGameObjects.Count == 0)
			{
				return;
			}

			if (_materialLimitToGameObjects == null)
			{
				_materialLimitToGameObjects = new Material(Shader.Find("Hidden/Decalicious Game Object ID"));
			}

			int num = Shader.PropertyToID("_DecaliciousLimitToGameObject");
			_bufferLimitTo.GetTemporaryRT(num, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RFloat);
			_bufferLimitTo.SetRenderTarget(num, BuiltinRenderTextureType.CameraTarget);
			_bufferLimitTo.ClearRenderTarget(false, true, Color.black);
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
			foreach (GameObject limitToGameObject in _limitToGameObjects)
			{
				_bufferLimitTo.SetGlobalFloat("_ID", limitToGameObject.GetInstanceID());
				_limitToMeshRenderers.Clear();
				limitToGameObject.GetComponentsInChildren(_limitToMeshRenderers);
				foreach (MeshRenderer limitToMeshRenderer in _limitToMeshRenderers)
				{
					_decalComponent.Clear();
					limitToMeshRenderer.GetComponents(_decalComponent);
					if (_decalComponent.Count == 0 &&
					    GeometryUtility.TestPlanesAABB(planes, limitToMeshRenderer.bounds))
					{
						_meshFilterComponent.Clear();
						limitToMeshRenderer.GetComponents(_meshFilterComponent);
						if (_meshFilterComponent.Count == 1)
						{
							MeshFilter meshFilter = _meshFilterComponent[0];
							_bufferLimitTo.DrawMesh(meshFilter.sharedMesh,
								limitToMeshRenderer.transform.localToWorldMatrix, _materialLimitToGameObjects);
						}
					}
				}

				_limitToSkinnedMeshRenderers.Clear();
				limitToGameObject.GetComponentsInChildren(_limitToSkinnedMeshRenderers);
				foreach (SkinnedMeshRenderer limitToSkinnedMeshRenderer in _limitToSkinnedMeshRenderers)
				{
				}
			}
		}

		private static void SetLightProbeOnBlock(SphericalHarmonicsL2 probe, MaterialPropertyBlock block)
		{
			for (int i = 0; i < 3; i++)
			{
				_avCoeff[i].x = probe[i, 3];
				_avCoeff[i].y = probe[i, 1];
				_avCoeff[i].z = probe[i, 2];
				_avCoeff[i].w = probe[i, 0] - probe[i, 6];
			}

			for (int j = 0; j < 3; j++)
			{
				_avCoeff[j + 3].x = probe[j, 4];
				_avCoeff[j + 3].y = probe[j, 5];
				_avCoeff[j + 3].z = 3f * probe[j, 6];
				_avCoeff[j + 3].w = probe[j, 7];
			}

			_avCoeff[6].x = probe[0, 8];
			_avCoeff[6].y = probe[1, 8];
			_avCoeff[6].z = probe[2, 8];
			_avCoeff[6].w = 1f;
			block.SetVector("unity_SHAr", _avCoeff[0]);
			block.SetVector("unity_SHAg", _avCoeff[1]);
			block.SetVector("unity_SHAb", _avCoeff[2]);
			block.SetVector("unity_SHBr", _avCoeff[3]);
			block.SetVector("unity_SHBg", _avCoeff[4]);
			block.SetVector("unity_SHBb", _avCoeff[5]);
			block.SetVector("unity_SHC", _avCoeff[6]);
		}

		private void DrawDeferredDecals_Albedo(Camera cam)
		{
			if (_deferredDecals.Count == 0)
			{
				return;
			}

			_bufferDeferred.SetRenderTarget(_albedoRenderTarget, BuiltinRenderTextureType.CameraTarget);
			SortedDictionary<int, Dictionary<Material, HashSet<Decalicious>>>.Enumerator enumerator =
				_deferredDecals.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Dictionary<Material, HashSet<Decalicious>>.Enumerator enumerator2 =
					enumerator.Current.Value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					Material key = enumerator2.Current.Key;
					HashSet<Decalicious> value = enumerator2.Current.Value;
					int count = value.Count;
					int num = 0;
					HashSet<Decalicious>.Enumerator enumerator3 = value.GetEnumerator();
					while (enumerator3.MoveNext())
					{
						Decalicious current = enumerator3.Current;
						if (!(current != null) || !current.DrawAlbedo)
						{
							continue;
						}

						if (UseInstancing && !current.UseLightProbes)
						{
							_matrices[num] = current.transform.localToWorldMatrix;
							_fadeValues[num] = current.Fade;
							_limitToValues[num] = ((!current.LimitTo) ? 0f : current.LimitTo.GetInstanceID());
							num++;
							if (num == 1023)
							{
								_instancedBlock.Clear();
								_instancedBlock.SetFloatArray("_MaskMultiplier", _fadeValues);
								_instancedBlock.SetFloatArray("_LimitTo", _limitToValues);
								SetLightProbeOnBlock(RenderSettings.ambientProbe, _instancedBlock);
								_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, key, 0, _matrices, num,
									_instancedBlock);
								num = 0;
							}
						}
						else
						{
							_directBlock.Clear();
							_directBlock.SetFloat("_MaskMultiplier", current.Fade);
							_directBlock.SetFloat("_LimitTo",
								(!current.LimitTo) ? 0f : current.LimitTo.GetInstanceID());
							if (current.UseLightProbes)
							{
								SphericalHarmonicsL2 probe;
								LightProbes.GetInterpolatedProbe(current.transform.position,
									current.GetComponent<MeshRenderer>(), out probe);
								SetLightProbeOnBlock(probe, _directBlock);
							}

							_bufferDeferred.DrawMesh(_cubeMesh, current.transform.localToWorldMatrix, key, 0, 0,
								_directBlock);
						}
					}

					if (UseInstancing && num > 0)
					{
						_instancedBlock.Clear();
						_instancedBlock.SetFloatArray("_MaskMultiplier", _fadeValues);
						_instancedBlock.SetFloatArray("_LimitTo", _limitToValues);
						SetLightProbeOnBlock(RenderSettings.ambientProbe, _instancedBlock);
						_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, key, 0, _matrices, num, _instancedBlock);
					}
				}
			}
		}

		private void DrawDeferredDecals_NormSpecSmooth(Camera cam)
		{
			if (_deferredDecals.Count == 0)
			{
				return;
			}

			int num = Shader.PropertyToID("_CameraGBufferTexture1Copy");
			_bufferDeferred.GetTemporaryRT(num, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
			int num2 = Shader.PropertyToID("_CameraGBufferTexture2Copy");
			_bufferDeferred.GetTemporaryRT(num2, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
			SortedDictionary<int, Dictionary<Material, HashSet<Decalicious>>>.Enumerator enumerator =
				_deferredDecals.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Dictionary<Material, HashSet<Decalicious>>.Enumerator enumerator2 =
					enumerator.Current.Value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					Material key = enumerator2.Current.Key;
					HashSet<Decalicious> value = enumerator2.Current.Value;
					int num3 = 0;
					HashSet<Decalicious>.Enumerator enumerator3 = value.GetEnumerator();
					while (enumerator3.MoveNext())
					{
						Decalicious current = enumerator3.Current;
						if (!(current != null) || !current.DrawNormalAndGloss)
						{
							continue;
						}

						if (current.HighQualityBlending)
						{
							_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, num);
							_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, num2);
							_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
							_instancedBlock.Clear();
							_instancedBlock.SetFloat("_MaskMultiplier", current.Fade);
							_instancedBlock.SetFloat("_LimitTo",
								(!current.LimitTo) ? 0f : current.LimitTo.GetInstanceID());
							_bufferDeferred.DrawMesh(_cubeMesh, current.transform.localToWorldMatrix, key, 0, 1,
								_instancedBlock);
						}
						else if (UseInstancing)
						{
							_matrices[num3] = current.transform.localToWorldMatrix;
							_fadeValues[num3] = current.Fade;
							_limitToValues[num3] = ((!current.LimitTo) ? 0f : current.LimitTo.GetInstanceID());
							num3++;
							if (num3 == 1023)
							{
								_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, num);
								_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, num2);
								_bufferDeferred.SetRenderTarget(_normalRenderTarget,
									BuiltinRenderTextureType.CameraTarget);
								_instancedBlock.Clear();
								_instancedBlock.SetFloatArray("_MaskMultiplier", _fadeValues);
								_instancedBlock.SetFloatArray("_LimitTo", _limitToValues);
								_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, key, 1, _matrices, num3,
									_instancedBlock);
								num3 = 0;
							}
						}
						else
						{
							if (num3 == 0)
							{
								_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, num);
								_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, num2);
							}

							_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
							_instancedBlock.Clear();
							_instancedBlock.SetFloat("_MaskMultiplier", current.Fade);
							_instancedBlock.SetFloat("_LimitTo",
								(!current.LimitTo) ? 0f : current.LimitTo.GetInstanceID());
							_bufferDeferred.DrawMesh(_cubeMesh, current.transform.localToWorldMatrix, key, 0, 1,
								_instancedBlock);
							num3++;
						}
					}

					if (UseInstancing && num3 > 0)
					{
						_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, num);
						_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, num2);
						_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
						_instancedBlock.Clear();
						_instancedBlock.SetFloatArray("_MaskMultiplier", _fadeValues);
						_instancedBlock.SetFloatArray("_LimitTo", _limitToValues);
						_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, key, 1, _matrices, num3, _instancedBlock);
					}
				}
			}
		}

		private void DrawUnlitDecals(Camera cam)
		{
			if (_unlitDecals.Count == 0)
			{
				return;
			}

			_bufferUnlit.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
			SortedDictionary<int, Dictionary<Material, HashSet<Decalicious>>>.Enumerator enumerator =
				_unlitDecals.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Dictionary<Material, HashSet<Decalicious>>.Enumerator enumerator2 =
					enumerator.Current.Value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					Material key = enumerator2.Current.Key;
					HashSet<Decalicious> value = enumerator2.Current.Value;
					int num = 0;
					HashSet<Decalicious>.Enumerator enumerator3 = value.GetEnumerator();
					while (enumerator3.MoveNext())
					{
						Decalicious current = enumerator3.Current;
						if (!(current != null))
						{
							continue;
						}

						if (UseInstancing)
						{
							_matrices[num] = current.transform.localToWorldMatrix;
							_fadeValues[num] = current.Fade;
							_limitToValues[num] = ((!current.LimitTo) ? 0f : current.LimitTo.GetInstanceID());
							num++;
							if (num == 1023)
							{
								_instancedBlock.Clear();
								_instancedBlock.SetFloatArray("_MaskMultiplier", _fadeValues);
								_instancedBlock.SetFloatArray("_LimitTo", _limitToValues);
								_bufferUnlit.DrawMeshInstanced(_cubeMesh, 0, key, 0, _matrices, num, _instancedBlock);
								num = 0;
							}
						}
						else
						{
							_instancedBlock.Clear();
							_instancedBlock.SetFloat("_MaskMultiplier", current.Fade);
							_instancedBlock.SetFloat("_LimitTo",
								(!current.LimitTo) ? 0f : current.LimitTo.GetInstanceID());
							_bufferUnlit.DrawMesh(_cubeMesh, current.transform.localToWorldMatrix, key, 0, 0,
								_instancedBlock);
						}
					}

					if (UseInstancing && num > 0)
					{
						_instancedBlock.Clear();
						_instancedBlock.SetFloatArray("_MaskMultiplier", _fadeValues);
						_instancedBlock.SetFloatArray("_LimitTo", _limitToValues);
						_bufferUnlit.DrawMeshInstanced(_cubeMesh, 0, key, 0, _matrices, num, _instancedBlock);
					}
				}
			}
		}

		private static void CreateBuffer(ref CommandBuffer buffer, Camera cam, string name, CameraEvent evt)
		{
			if (buffer != null)
			{
				return;
			}

			CommandBuffer[] commandBuffers = cam.GetCommandBuffers(evt);
			foreach (CommandBuffer commandBuffer in commandBuffers)
			{
				if (commandBuffer.name == name)
				{
					buffer = commandBuffer;
					break;
				}
			}

			if (buffer == null)
			{
				buffer = new CommandBuffer();
				buffer.name = name;
				cam.AddCommandBuffer(evt, buffer);
			}
		}

		public void Add(Decalicious decal, GameObject limitTo)
		{
			if ((bool)limitTo)
			{
				_limitToGameObjects.Add(limitTo);
			}

			switch (decal.RenderMode)
			{
				case Decalicious.DecalRenderMode.Deferred:
					AddDeferred(decal);
					break;
				case Decalicious.DecalRenderMode.Unlit:
					AddUnlit(decal);
					break;
			}
		}

		protected void AddDeferred(Decalicious decal)
		{
			if (!_deferredDecals.ContainsKey(decal.RenderOrder))
			{
				_deferredDecals.Add(decal.RenderOrder, new Dictionary<Material, HashSet<Decalicious>>());
			}

			Dictionary<Material, HashSet<Decalicious>> dictionary = _deferredDecals[decal.RenderOrder];
			if (!dictionary.ContainsKey(decal.Material))
			{
				dictionary.Add(decal.Material, new HashSet<Decalicious> { decal });
			}
			else
			{
				dictionary[decal.Material].Add(decal);
			}
		}

		protected void AddUnlit(Decalicious decal)
		{
			if (!_unlitDecals.ContainsKey(decal.RenderOrder))
			{
				_unlitDecals.Add(decal.RenderOrder, new Dictionary<Material, HashSet<Decalicious>>());
			}

			Dictionary<Material, HashSet<Decalicious>> dictionary = _unlitDecals[decal.RenderOrder];
			if (!dictionary.ContainsKey(decal.Material))
			{
				dictionary.Add(decal.Material, new HashSet<Decalicious> { decal });
			}
			else
			{
				dictionary[decal.Material].Add(decal);
			}
		}
	}
}
