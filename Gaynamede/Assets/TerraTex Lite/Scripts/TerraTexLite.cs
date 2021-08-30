using System.Collections.Generic;
using UnityEngine;

namespace TerraTexLite
{
	[System.Serializable]
	public class SurfaceLayers
	{
		public RockLayer Rock;
		public GrassLayer Grass;
		public SandLayer Sand;
	}

	[System.Serializable]
	public class RockLayer
	{
		public List<SurfaceTexture> SurfaceTextures = new List<SurfaceTexture>();
		//public Vector2 textureTileSize = new Vector2(10, 10);
		public float rockTerrainAngleLower = 15;
		public float rockTerrainAngleUpper = 65;
	}

	[System.Serializable]
	public class GrassLayer
	{
		public List<SurfaceTexture> SurfaceTextures = new List<SurfaceTexture>();
		//public Vector2 textureTileSize = new Vector2(10, 10);
		public float grassMaxAltitude = 900;
		public float grassBoundaryHeight = 100;
	}

	[System.Serializable]
	public class SandLayer
	{
		public List<SurfaceTexture> SurfaceTextures = new List<SurfaceTexture>();
		//public Vector2 textureTileSize = new Vector2(10, 10);
	}


	public enum TextureModes {Perlin, Fixed };

	[System.Serializable]
	public class SurfaceTexture
	{
		public TextureModes TextureMode = TextureModes.Perlin;
		public Vector2 textureTileSize = new Vector2(1, 1);
		public Texture2D Texture;
		public Texture2D NormalMap;
		public float TextureOpacity = 1;
		public bool minHeightOn = false;
		public bool maxHeightOn = false;
	}

	[System.Serializable]
	public class Water
	{
		public Transform waterTransform;
		public float waterLevel = 100;
		public float waterBankHeight = 30;
		public Vector2 waterObjectScale = new Vector2(1, 1);
		public Vector2 waterPositionOffset = new Vector2(0, 0);
	}

	[System.Serializable]
	public class UI
	{
		public int tab;
		public int subTab;
		public int[] textureIndex = new int[4];
	}

	public class TerraTexLite : MonoBehaviour
	{

		public Terrain terrain;
		public Water water;
		public SurfaceLayers surfaceLayers;
		public UI ui;
		public bool runOnGameStart;
		
		private TerrainData terrainData;
		[HideInInspector]
		public string[,] splatTextureIndex;
		

		void Start()
		{
			if (runOnGameStart) runScript();
		}

		[ContextMenu("Generate Terrain Texture")]
		public void runScript()
		{
			terrainData = new TerrainData();
			int maxLength = surfaceLayers.Rock.SurfaceTextures.Count;
			if (surfaceLayers.Grass.SurfaceTextures.Count > maxLength) maxLength = surfaceLayers.Grass.SurfaceTextures.Count;
			if (surfaceLayers.Sand.SurfaceTextures.Count > maxLength) maxLength = surfaceLayers.Sand.SurfaceTextures.Count;

			SplatPrototype[,] splatTextures = new SplatPrototype[4, maxLength];
			splatTextureIndex = new string[4, maxLength];

			for (int surfaceVariant = 0; surfaceVariant < splatTextures.GetLength(1); surfaceVariant++)
			{
				splatTextureIndex[0, surfaceVariant] = "Texture Doesn't Exist";
				splatTextureIndex[1, surfaceVariant] = "Texture Doesn't Exist";
				splatTextureIndex[2, surfaceVariant] = "Texture Doesn't Exist";
				splatTextureIndex[3, surfaceVariant] = "Texture Doesn't Exist";
				splatTextureIndex[3, surfaceVariant] = "Texture Doesn't Exist";

				if (surfaceVariant < surfaceLayers.Rock.SurfaceTextures.Count && surfaceLayers.Rock.SurfaceTextures[surfaceVariant] != null)
				{
					splatTextures[0, surfaceVariant] = new SplatPrototype();
					splatTextures[0, surfaceVariant].texture = surfaceLayers.Rock.SurfaceTextures[surfaceVariant].Texture;
					splatTextures[0, surfaceVariant].normalMap = surfaceLayers.Rock.SurfaceTextures[surfaceVariant].NormalMap;
					splatTextures[0, surfaceVariant].tileSize = new Vector2(surfaceLayers.Rock.SurfaceTextures[surfaceVariant].textureTileSize.x, surfaceLayers.Rock.SurfaceTextures[surfaceVariant].textureTileSize.y);
				}
				if (surfaceVariant < surfaceLayers.Grass.SurfaceTextures.Count && surfaceLayers.Grass.SurfaceTextures[surfaceVariant] != null)
				{
					splatTextures[1, surfaceVariant] = new SplatPrototype();
					splatTextures[1, surfaceVariant].texture = surfaceLayers.Grass.SurfaceTextures[surfaceVariant].Texture;
					splatTextures[1, surfaceVariant].normalMap = surfaceLayers.Grass.SurfaceTextures[surfaceVariant].NormalMap;
					splatTextures[1, surfaceVariant].tileSize = new Vector2(surfaceLayers.Grass.SurfaceTextures[surfaceVariant].textureTileSize.x, surfaceLayers.Grass.SurfaceTextures[surfaceVariant].textureTileSize.y);
				}
				if (surfaceVariant < surfaceLayers.Sand.SurfaceTextures.Count && surfaceLayers.Sand.SurfaceTextures[surfaceVariant] != null)
				{
					splatTextures[2, surfaceVariant] = new SplatPrototype();
					splatTextures[2, surfaceVariant].texture = surfaceLayers.Sand.SurfaceTextures[surfaceVariant].Texture;
					splatTextures[2, surfaceVariant].normalMap = surfaceLayers.Sand.SurfaceTextures[surfaceVariant].NormalMap;
					splatTextures[2, surfaceVariant].tileSize = new Vector2(surfaceLayers.Sand.SurfaceTextures[surfaceVariant].textureTileSize.x, surfaceLayers.Sand.SurfaceTextures[surfaceVariant].textureTileSize.y);
				}
			}

			List<SplatPrototype> splatTexturesList = new List<SplatPrototype>();
			int index = 0;
			for (int terrainSurface = 0; terrainSurface < splatTextures.GetLength(0); terrainSurface++)
			{
				for (int splatTexture = 0; splatTexture < splatTextures.GetLength(1); splatTexture++)
				{
					if (splatTextures[terrainSurface, splatTexture] != null)
					{
						splatTexturesList.Add(splatTextures[terrainSurface, splatTexture]);
						splatTextureIndex[terrainSurface, splatTexture] = index.ToString();
						index++;
					}
				}
			}
			SplatPrototype[] splatTextures1D = new SplatPrototype[splatTexturesList.Count];
			splatTextures1D = splatTexturesList.ToArray();
			terrainData = terrain.terrainData;
			terrainData.splatPrototypes = splatTextures1D;

			terrainData.RefreshPrototypes();

			GenerateTexture();
		}

		void GenerateTexture()
		{
			if (water.waterTransform != null)
			{
				water.waterTransform.position = new Vector3(terrain.transform.position.x + terrainData.bounds.size.x / 2, water.waterLevel, terrain.transform.position.z + terrainData.bounds.size.x / 2);
				water.waterTransform.position += new Vector3(water.waterPositionOffset.x, 0, water.waterPositionOffset.y);
				water.waterTransform.localScale = new Vector3(terrainData.bounds.size.x / 10, 1, terrainData.bounds.size.z / 10);
				water.waterTransform.localScale = new Vector3(water.waterTransform.localScale.x * water.waterObjectScale.x, 1, water.waterTransform.localScale.z * water.waterObjectScale.y);
			}

			float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];


			for (int y = 0; y < terrainData.alphamapHeight; y++)
			{
				for (int x = 0; x < terrainData.alphamapWidth; x++)
				{
					float[] splatWeights = new float[terrainData.alphamapLayers];

					// Normalise x/y coordinates to range 0-1 
					float y_01 = (float)y / (float)terrainData.alphamapHeight;
					float x_01 = (float)x / (float)terrainData.alphamapWidth;
					//Get height and steepness
					float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapResolution), Mathf.RoundToInt(x_01 * terrainData.heightmapResolution));
					float steepness = terrainData.GetSteepness(y_01, x_01);

					float weightSum = 0;
					float rockSum = 0;
					float grassSum = 0;
					float sandSum = 0;

					//Generate rock base layer
					if (steepness >= surfaceLayers.Rock.rockTerrainAngleLower || height > surfaceLayers.Grass.grassMaxAltitude - surfaceLayers.Grass.grassBoundaryHeight)
					{
						rockSum = 0;
						int rockIndex = 0;
						for (int rockTex = 0; rockTex < surfaceLayers.Rock.SurfaceTextures.Count; rockTex++)
						{
							
							splatWeights[rockTex] = surfaceLayers.Rock.SurfaceTextures[rockIndex].TextureOpacity;


							rockSum += splatWeights[rockTex];
							rockIndex++;
						}

						for (int rockTex = 0; rockTex < surfaceLayers.Rock.SurfaceTextures.Count; rockTex++)
						{
							splatWeights[rockTex] /= rockSum;
							//weightSum += splatWeights[rockTex];
						}
					}

					//Generate grass/sand layer
					if (steepness < surfaceLayers.Rock.rockTerrainAngleUpper && height <= surfaceLayers.Grass.grassMaxAltitude)
					{
						//Above water level
						if (height > water.waterLevel + water.waterBankHeight)
						{
							grassSum = 0;
							int index = 0;
							for (int grassTex = surfaceLayers.Rock.SurfaceTextures.Count; grassTex < surfaceLayers.Rock.SurfaceTextures.Count + surfaceLayers.Grass.SurfaceTextures.Count; grassTex++)
							{
								splatWeights[grassTex] = surfaceLayers.Grass.SurfaceTextures[index].TextureOpacity;
								
								grassSum += splatWeights[grassTex];
								index++;
							}
							for (int grassTex = surfaceLayers.Rock.SurfaceTextures.Count; grassTex < surfaceLayers.Rock.SurfaceTextures.Count + surfaceLayers.Grass.SurfaceTextures.Count; grassTex++)
							{
								splatWeights[grassTex] /= grassSum;
								if (steepness > surfaceLayers.Rock.rockTerrainAngleLower)
									splatWeights[grassTex] *= 1 - ((steepness - surfaceLayers.Rock.rockTerrainAngleLower) / (surfaceLayers.Rock.rockTerrainAngleUpper - surfaceLayers.Rock.rockTerrainAngleLower));
								weightSum += splatWeights[grassTex];
							}
							if (steepness > surfaceLayers.Rock.rockTerrainAngleLower)
							{
								for (int rockTex = 0; rockTex < surfaceLayers.Rock.SurfaceTextures.Count; rockTex++)
								{
									splatWeights[rockTex] *= ((steepness - surfaceLayers.Rock.rockTerrainAngleLower) / (surfaceLayers.Rock.rockTerrainAngleUpper - surfaceLayers.Rock.rockTerrainAngleLower));
									weightSum += splatWeights[rockTex];
								}
							}
							/*for (int sandTex = surfaceLayers.Rock.SurfaceTextures.Count; sandTex < surfaceLayers.Rock.SurfaceTextures.Count + surfaceLayers.Grass.SurfaceTextures.Count + sandTextures.Length; sandTex++)
							{
								splatWeights[sandTex] = 0;
							}*/
						}
						//Below water level
						else if (height <= water.waterLevel)
						{
							sandSum = 0;
							int index = 0;
							for (int sandTex = surfaceLayers.Rock.SurfaceTextures.Count + surfaceLayers.Grass.SurfaceTextures.Count; sandTex < surfaceLayers.Rock.SurfaceTextures.Count + surfaceLayers.Grass.SurfaceTextures.Count + surfaceLayers.Sand.SurfaceTextures.Count; sandTex++)
							{
								splatWeights[sandTex] = surfaceLayers.Sand.SurfaceTextures[index].TextureOpacity;
								
								sandSum += splatWeights[sandTex];
								index++;
							}
							for (int sandTex = surfaceLayers.Rock.SurfaceTextures.Count + surfaceLayers.Grass.SurfaceTextures.Count; sandTex < surfaceLayers.Rock.SurfaceTextures.Count + surfaceLayers.Grass.SurfaceTextures.Count + surfaceLayers.Sand.SurfaceTextures.Count; sandTex++)
							{
								splatWeights[sandTex] /= sandSum;
								if (steepness > surfaceLayers.Rock.rockTerrainAngleLower)
									splatWeights[sandTex] *= 1 - ((steepness - surfaceLayers.Rock.rockTerrainAngleLower) / (surfaceLayers.Rock.rockTerrainAngleUpper - surfaceLayers.Rock.rockTerrainAngleLower));
								weightSum += splatWeights[sandTex];
							}
							if (steepness > surfaceLayers.Rock.rockTerrainAngleLower)
							{
								for (int rockTex = 0; rockTex < surfaceLayers.Rock.SurfaceTextures.Count; rockTex++)
								{
									splatWeights[rockTex] *= ((steepness - surfaceLayers.Rock.rockTerrainAngleLower) / (surfaceLayers.Rock.rockTerrainAngleUpper - surfaceLayers.Rock.rockTerrainAngleLower));
									weightSum += splatWeights[rockTex];
								}
							}

						}
						//On water bank
						else
						{
							sandSum = 0;
							int index = 0;
							for (int sandTex = surfaceLayers.Rock.SurfaceTextures.Count + surfaceLayers.Grass.SurfaceTextures.Count; sandTex < surfaceLayers.Rock.SurfaceTextures.Count + surfaceLayers.Grass.SurfaceTextures.Count + surfaceLayers.Sand.SurfaceTextures.Count; sandTex++)
							{
								splatWeights[sandTex] = surfaceLayers.Sand.SurfaceTextures[index].TextureOpacity;
								
								sandSum += splatWeights[sandTex];
								index++;
							}
							for (int sandTex = surfaceLayers.Rock.SurfaceTextures.Count + surfaceLayers.Grass.SurfaceTextures.Count; sandTex < surfaceLayers.Rock.SurfaceTextures.Count + surfaceLayers.Grass.SurfaceTextures.Count + surfaceLayers.Sand.SurfaceTextures.Count; sandTex++)
							{
								splatWeights[sandTex] /= sandSum;
								splatWeights[sandTex] *= 1 - ((height - water.waterLevel) / (water.waterBankHeight));

								if (steepness > surfaceLayers.Rock.rockTerrainAngleLower)
									splatWeights[sandTex] *= 1 - ((steepness - surfaceLayers.Rock.rockTerrainAngleLower) / (surfaceLayers.Rock.rockTerrainAngleUpper - surfaceLayers.Rock.rockTerrainAngleLower));
								weightSum += splatWeights[sandTex];
							}




							grassSum = 0;
							index = 0;
							for (int grassTex = surfaceLayers.Rock.SurfaceTextures.Count; grassTex < surfaceLayers.Rock.SurfaceTextures.Count + surfaceLayers.Grass.SurfaceTextures.Count; grassTex++)
							{
								splatWeights[grassTex] = surfaceLayers.Grass.SurfaceTextures[index].TextureOpacity;
								
								grassSum += splatWeights[grassTex];
								index++;
							}
							for (int grassTex = surfaceLayers.Rock.SurfaceTextures.Count; grassTex < surfaceLayers.Rock.SurfaceTextures.Count + surfaceLayers.Grass.SurfaceTextures.Count; grassTex++)
							{
								splatWeights[grassTex] /= grassSum;
								splatWeights[grassTex] *= ((height - water.waterLevel) / (water.waterBankHeight));

								if (steepness > surfaceLayers.Rock.rockTerrainAngleLower)
									splatWeights[grassTex] *= 1 - ((steepness - surfaceLayers.Rock.rockTerrainAngleLower) / (surfaceLayers.Rock.rockTerrainAngleUpper - surfaceLayers.Rock.rockTerrainAngleLower));
								weightSum += splatWeights[grassTex];
							}


							if (steepness > surfaceLayers.Rock.rockTerrainAngleLower)
							{
								for (int rockTex = 0; rockTex < surfaceLayers.Rock.SurfaceTextures.Count; rockTex++)
								{
									splatWeights[rockTex] *= ((steepness - surfaceLayers.Rock.rockTerrainAngleLower) / (surfaceLayers.Rock.rockTerrainAngleUpper - surfaceLayers.Rock.rockTerrainAngleLower));
									weightSum += splatWeights[rockTex];
								}
							}

						}


					}

					
					//Apply splatweights
					for (int i = 0; i < terrainData.alphamapLayers; i++)
					{
						splatmapData[x, y, i] = splatWeights[i];
					}
				}
			}
			terrainData.SetAlphamaps(0, 0, splatmapData);
		}

	}
}