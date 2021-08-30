using UnityEngine;
using UnityEditor;

namespace TerraTexLite
{

	[CustomEditor(typeof(TerraTexLite))]
	public class TerraTexLiteGUI : Editor
	{
		TerraTexLite tt;
		GUIStyle titleStyle;
		GUIStyle headingStyle;
		GUIStyle smallHeadingStyle;
		GUIStyle wrappedLabelStyle;


		int surfaceTexturesIndex;

		Texture2D titleImage;
		bool newTextureUpdate;

		Texture2D[] buttons = new Texture2D[4];
		Texture2D[] titles = new Texture2D[4];
		Texture2D upgrade;
		string[] surfaces = { "Base", "Cliffs", "Waterbed" };
		string[] tooltips = { "Terrain", "Surface", "Water", "Generate" };



		bool UndoMapsInitiated;

		void OnEnable()
		{
			initStyles();
			tt = (TerraTexLite)target;

			string path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
			path = path.Replace("TerraTexLiteGUI.cs", "");
			titleImage = (Texture2D)AssetDatabase.LoadAssetAtPath(path + "GUI Textures/Title.png", typeof(Texture2D));
			buttons[0] = (Texture2D)AssetDatabase.LoadAssetAtPath(path + "GUI Textures/TerrainIcon.png", typeof(Texture2D));
			buttons[2] = (Texture2D)AssetDatabase.LoadAssetAtPath(path + "GUI Textures/WaterIcon.png", typeof(Texture2D));
			buttons[1] = (Texture2D)AssetDatabase.LoadAssetAtPath(path + "GUI Textures/SurfaceIcon.png", typeof(Texture2D));
			buttons[3] = (Texture2D)AssetDatabase.LoadAssetAtPath(path + "GUI Textures/GenerateIcon.png", typeof(Texture2D));
			titles[0] = (Texture2D)AssetDatabase.LoadAssetAtPath(path + "GUI Textures/Terrain.png", typeof(Texture2D));
			titles[2] = (Texture2D)AssetDatabase.LoadAssetAtPath(path + "GUI Textures/Water.png", typeof(Texture2D));
			titles[1] = (Texture2D)AssetDatabase.LoadAssetAtPath(path + "GUI Textures/Surface.png", typeof(Texture2D));
			titles[3] = (Texture2D)AssetDatabase.LoadAssetAtPath(path + "GUI Textures/Generate.png", typeof(Texture2D));
			upgrade = (Texture2D)AssetDatabase.LoadAssetAtPath(path + "GUI Textures/upgrade.png", typeof(Texture2D));
		}




		public override void OnInspectorGUI()
		{

			float temp = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 150;

			GUILayout.Space(5);

			//Title Image
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(titleImage);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();


			//Tab Buttons
			EditorGUI.BeginChangeCheck();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUIContent[] tabs = { new GUIContent(buttons[0], tooltips[0]), new GUIContent(buttons[1], tooltips[1]), new GUIContent(buttons[2], tooltips[2]), new GUIContent(buttons[3], tooltips[3]) };
			int tab1 = GUILayout.Toolbar(tt.ui.tab, tabs, GUILayout.Height(40), GUILayout.MaxWidth(550));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			bool update = false;
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(tt, "TerraTex Settings Tab Changed");
				tt.ui.tab = tab1;
				update = true;
			}

			//Content
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical("box", GUILayout.MaxWidth(550));

			//Sub-Title image
			GUILayout.Label(titles[tt.ui.tab]);

			EditorGUI.BeginChangeCheck();
			switch (tt.ui.tab)
			{
				#region Terrain
				case 0:
					GUILayout.BeginHorizontal();
					//GUILayout.Label("Terrain Object", GUILayout.Width(150));
					Terrain terrain = (Terrain)EditorGUILayout.ObjectField("Terrain Object", tt.terrain, typeof(Terrain), true);
					GUILayout.EndHorizontal();
					if (EditorGUI.EndChangeCheck())
					{
						Undo.RecordObject(tt, "TerraTex Terrain Settings Changed");
						tt.terrain = terrain;
					}
					break;
				#endregion
				#region Surface
				case 1:
					//Sub-Tab Buttons
					EditorGUI.BeginChangeCheck();
					int subTab1 = GUILayout.Toolbar(tt.ui.subTab, surfaces, GUILayout.Height(25));
					if (EditorGUI.EndChangeCheck())
					{
						Undo.RecordObject(tt, "TerraTex Settings SubTab Changed");
						tt.ui.subTab = subTab1;
						update = true;
					}
					surfaceoptions(update);

					//Undo system included in surfaceOptions()
					break;
				#endregion
				#region Water
				case 2:
					EditorGUI.BeginChangeCheck();
					GUILayout.BeginHorizontal();
					//GUILayout.Label("Water Object", GUILayout.Width(150));
					Transform waterTransform = (Transform)EditorGUILayout.ObjectField("Water Object", tt.water.waterTransform, typeof(Transform), true);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					//GUILayout.Label("Water Level", GUILayout.Width(150));
					//float OLDwl = tt.water.waterLevel;
					float waterLevel = EditorGUILayout.FloatField("Water Level", tt.water.waterLevel);
					//if (!update)
					//	update = waterLevel == OLDwl? false : true;
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					//GUILayout.Label("Water Bank Height", GUILayout.Width(150));
					//float OLDwbh = tt.water.waterBankHeight;
					float waterBankHeight = Mathf.Max(0, EditorGUILayout.FloatField("Water Bank Height", tt.water.waterBankHeight));
					//if (!update)
					//	update = waterBankHeight == OLDwbh ? false : true;

					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					//GUILayout.Label("Water Object Scale", GUILayout.Width(150));
					Vector2 waterObjectScale = EditorGUILayout.Vector2Field("Water Object Scale", tt.water.waterObjectScale);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					//GUILayout.Label("Water Position Offset", GUILayout.Width(150));
					Vector2 waterPositionOffset = EditorGUILayout.Vector2Field("Water Position Offest", tt.water.waterPositionOffset);
					GUILayout.EndHorizontal();

					if (EditorGUI.EndChangeCheck())
					{
						Undo.RecordObject(tt, "TerraTex Water Settings Changed");
						tt.water.waterTransform = waterTransform;
						tt.water.waterLevel = waterLevel;
						tt.water.waterBankHeight = waterBankHeight;
						tt.water.waterObjectScale = waterObjectScale;
						tt.water.waterPositionOffset = waterPositionOffset;
					}

					
					break;
				#endregion
				#region Generate
				case 3:
					GUILayout.Label("<i>Generating the texture can take several seconds, during which the editor will become unresponsive\nPlease be patient and wait for the process to finish</i>", wrappedLabelStyle);
					//GUILayout.EndHorizontal();
					//GUILayout.Label("<b>Generate</b>", headingStyle);
					if (GUILayout.Button("Generate Terrain Texture"))
						tt.runScript();
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Clear Terrain Texture") && EditorUtility.DisplayDialog("Confirm", "Are you sure you want to completely clear the current texture from the terrain?\n\nThis cannot be undone", "Yes", "No"))
					{
						TerrainData terrainData = tt.terrain.terrainData;
						float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
						terrainData.SetAlphamaps(0, 0, splatmapData);
					}
					GUILayout.EndHorizontal();
					break;
					#endregion
			}

			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = temp;

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button(upgrade, GUI.skin.label))
			{
				Application.OpenURL("https://assetstore.unity.com/packages/slug/87137");
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			//DrawDefaultInspector();
		}

		void initStyles()
		{
			titleStyle = new GUIStyle();
			titleStyle.fontSize = 18;
			titleStyle.normal.textColor = new Color32(90, 140, 90, 255);
			titleStyle.richText = true;

			headingStyle = new GUIStyle();
			headingStyle.fontSize = 16;
			headingStyle.richText = true;

			smallHeadingStyle = new GUIStyle();
			smallHeadingStyle.fontSize = 14;
			smallHeadingStyle.richText = true;

			wrappedLabelStyle = new GUIStyle();
			wrappedLabelStyle.fontSize = 10;
			wrappedLabelStyle.wordWrap = true;
			wrappedLabelStyle.richText = true;


		}

		static int[] count = new int[4];
		//static float[] heightScroll = new float[4];
		//static int tt.ui.last;
		void surfaceoptions(bool updatePerlinMap)
		{
			switch (tt.ui.subTab)
			{
				#region grass
				case 0:
					{
						bool update = false;
						if (newTextureUpdate)
						{
							updatePerlinMap = true;
							newTextureUpdate = false;
						}

						GUILayout.BeginVertical("box");
						GUILayout.BeginHorizontal();
						//GUILayout.Label("Grass Max Altitude", GUILayout.Width(150));
						float OLDgma = tt.surfaceLayers.Grass.grassMaxAltitude;
						float grassMaxAltitude = Mathf.Max(EditorGUILayout.FloatField("Base Max Altitude", tt.surfaceLayers.Grass.grassMaxAltitude), 0);
						if (!update)
							update = grassMaxAltitude == OLDgma ? false : true;
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						//GUILayout.Label("Grass Boundary Height", GUILayout.Width(150));
						float OLDgbh = tt.surfaceLayers.Grass.grassBoundaryHeight;
						float grassBoundaryHeight = Mathf.Max(EditorGUILayout.FloatField("Base Boundary Height", tt.surfaceLayers.Grass.grassBoundaryHeight), 0);
						if (!update)
							update = grassBoundaryHeight == OLDgbh ? false : true;
						//if (grassBoundaryHeight < 0)
						//	grassBoundaryHeight = 0;
						GUILayout.EndHorizontal();

						GUILayout.Space(5);


						GUILayout.Label("<b>Texture</b>", smallHeadingStyle);


						int textureIndexInt = 0;
						bool addNewTexture = false;
						if (tt.surfaceLayers.Grass.SurfaceTextures.Count == 0)
						{
							addNewTexture = true;
						}

						Vector2 textureTileSize = new Vector2(0, 0);
						Texture2D mainTexture = new Texture2D(0, 0);
						Texture2D normalMap = new Texture2D(0, 0);

						float textureOpacity = 1;
						TextureModes textureMode = TextureModes.Fixed;
						int newIndex = -1;
						if (tt.surfaceLayers.Grass.SurfaceTextures.Count > 0)
						{
							GUILayout.BeginVertical("box");

							GUILayout.BeginHorizontal();
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							//GUILayout.Label("Texture Tile Size", GUILayout.Width(150));
							EditorGUILayout.PrefixLabel("Texture Tile Size");
							textureTileSize = EditorGUILayout.Vector2Field("", tt.surfaceLayers.Grass.SurfaceTextures[textureIndexInt].textureTileSize);
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							//GUILayout.Label("Texture", GUILayout.Width(150));
							EditorGUILayout.PrefixLabel("Texture");
							//mainTexture
							mainTexture = (Texture2D)EditorGUILayout.ObjectField(tt.surfaceLayers.Grass.SurfaceTextures[textureIndexInt].Texture, typeof(Texture2D), true);
							//tt.surfaceLayers.Grass.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].Texture = (Texture2D)EditorGUILayout.ObjectField(tt.surfaceLayers.Grass.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].Texture, typeof(Texture2D), true);
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							//GUILayout.Label("Normal Map", GUILayout.Width(150));
							EditorGUILayout.PrefixLabel("Normal Map");
							normalMap = (Texture2D)EditorGUILayout.ObjectField(tt.surfaceLayers.Grass.SurfaceTextures[textureIndexInt].NormalMap, typeof(Texture2D), true);
							GUILayout.EndHorizontal();

							GUILayout.EndVertical();


						}



						GUILayout.EndVertical();
						if (EditorGUI.EndChangeCheck() || updatePerlinMap)
						{
							Undo.RecordObject(tt, "TerraTex Grass Surface Settings Changed");


							tt.ui.textureIndex[tt.ui.subTab] = textureIndexInt;

							tt.surfaceLayers.Grass.grassMaxAltitude = grassMaxAltitude;
							tt.surfaceLayers.Grass.grassBoundaryHeight = grassBoundaryHeight;

							if (tt.surfaceLayers.Grass.SurfaceTextures.Count > 0)
							{
								tt.surfaceLayers.Grass.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].TextureMode = textureMode;

								tt.surfaceLayers.Grass.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].textureTileSize = textureTileSize;
								tt.surfaceLayers.Grass.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].Texture = mainTexture;
								tt.surfaceLayers.Grass.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].NormalMap = normalMap;


								tt.surfaceLayers.Grass.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].TextureOpacity = textureOpacity;


							}

							if (addNewTexture)
							{
								tt.surfaceLayers.Grass.SurfaceTextures.Add(new SurfaceTexture());
								tt.ui.textureIndex[tt.ui.subTab] = tt.surfaceLayers.Grass.SurfaceTextures.Count - 1;
								//update = true;
								newTextureUpdate = true;

							}

							if (newIndex != -1)
								tt.ui.textureIndex[tt.ui.subTab] = newIndex;


						}

						count[0] = tt.surfaceLayers.Grass.SurfaceTextures.Count;

						break;
					}
				#endregion
				#region rock
				case 1:
					{
						bool update = false;
						if (newTextureUpdate)
						{
							updatePerlinMap = true;
							newTextureUpdate = false;
						}

						GUILayout.BeginVertical("box");
						GUILayout.BeginHorizontal();
						//GUILayout.Label("0% Rock Angle", GUILayout.Width(150));
						float OLDrtal = tt.surfaceLayers.Rock.rockTerrainAngleLower;
						float rockTerrainAngleLower = Mathf.Clamp(EditorGUILayout.FloatField("0% Cliff Angle", tt.surfaceLayers.Rock.rockTerrainAngleLower), 0, 90);
						if (!update)
							update = rockTerrainAngleLower == OLDrtal ? false : true;
						GUILayout.EndHorizontal();

						//tt.surfaceLayers.Rock.rockTerrainAngleLower
						GUILayout.BeginHorizontal();
						//GUILayout.Label("100% Rock Angle", GUILayout.Width(150));
						float OLDrtau = tt.surfaceLayers.Rock.rockTerrainAngleUpper;
						float rockTerrainAngleUpper = Mathf.Clamp(EditorGUILayout.FloatField("100% Cliff Angle", tt.surfaceLayers.Rock.rockTerrainAngleUpper), 0, 90);
						if (!update)
							update = rockTerrainAngleUpper == OLDrtau ? false : true;
						GUILayout.EndHorizontal();

						GUILayout.Space(5);


						GUILayout.Label("<b>Texture</b>", smallHeadingStyle);

						int textureIndexInt = 0;
						bool addNewTexture = false;
						if (tt.surfaceLayers.Rock.SurfaceTextures.Count == 0)
						{
							addNewTexture = true;
						}

						Vector2 textureTileSize = new Vector2(0, 0);
						Texture2D mainTexture = new Texture2D(0, 0);
						Texture2D normalMap = new Texture2D(0, 0);
						float textureOpacity = 1;
						TextureModes textureMode = TextureModes.Fixed;
						int newIndex = -1;
						if (tt.surfaceLayers.Rock.SurfaceTextures.Count > 0)
						{
							GUILayout.BeginVertical("box");

							GUILayout.BeginHorizontal();
							//GUILayout.Label("Texture Tile Size", GUILayout.Width(150));
							EditorGUILayout.PrefixLabel("Texture Tile Size");
							textureTileSize = EditorGUILayout.Vector2Field("", tt.surfaceLayers.Rock.SurfaceTextures[textureIndexInt].textureTileSize);
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							//GUILayout.Label("Texture", GUILayout.Width(150));
							EditorGUILayout.PrefixLabel("Texture");
							//mainTexture
							mainTexture = (Texture2D)EditorGUILayout.ObjectField(tt.surfaceLayers.Rock.SurfaceTextures[textureIndexInt].Texture, typeof(Texture2D), true);
							//tt.surfaceLayers.Rock.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].Texture = (Texture2D)EditorGUILayout.ObjectField(tt.surfaceLayers.Rock.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].Texture, typeof(Texture2D), true);
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							//GUILayout.Label("Normal Map", GUILayout.Width(150));
							EditorGUILayout.PrefixLabel("Normal Map");
							normalMap = (Texture2D)EditorGUILayout.ObjectField(tt.surfaceLayers.Rock.SurfaceTextures[textureIndexInt].NormalMap, typeof(Texture2D), true);
							GUILayout.EndHorizontal();

							GUILayout.EndVertical();

						}



						GUILayout.EndVertical();
						if (EditorGUI.EndChangeCheck() || updatePerlinMap)
						{
							Undo.RecordObject(tt, "TerraTex Rock Surface Settings Changed");
							
							tt.ui.textureIndex[tt.ui.subTab] = textureIndexInt;

							tt.surfaceLayers.Rock.rockTerrainAngleLower = rockTerrainAngleLower;
							tt.surfaceLayers.Rock.rockTerrainAngleUpper = rockTerrainAngleUpper;

							if (tt.surfaceLayers.Rock.SurfaceTextures.Count > 0)
							{
								tt.surfaceLayers.Rock.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].TextureMode = textureMode;

								tt.surfaceLayers.Rock.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].textureTileSize = textureTileSize;
								tt.surfaceLayers.Rock.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].Texture = mainTexture;
								tt.surfaceLayers.Rock.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].NormalMap = normalMap;


								if (tt.surfaceLayers.Rock.SurfaceTextures[textureIndexInt].TextureMode == TextureModes.Perlin)
								{
								}
								tt.surfaceLayers.Rock.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].TextureOpacity = textureOpacity;

							}


							if (addNewTexture)
							{
								tt.surfaceLayers.Rock.SurfaceTextures.Add(new SurfaceTexture());
								tt.ui.textureIndex[tt.ui.subTab] = tt.surfaceLayers.Rock.SurfaceTextures.Count - 1;
								//update = true;
								newTextureUpdate = true;

							}

							if (newIndex != -1)
								tt.ui.textureIndex[tt.ui.subTab] = newIndex;


						}

						count[0] = tt.surfaceLayers.Rock.SurfaceTextures.Count;
						break;
					}
				#endregion
				#region sand
				case 2:
					{
						if (newTextureUpdate)
						{
							updatePerlinMap = true;
							newTextureUpdate = false;
						}

						GUILayout.BeginVertical("box");


						GUILayout.Label("<b>Texture</b>", smallHeadingStyle);

						int textureIndexInt = 0;
						bool addNewTexture = false;
						if (tt.surfaceLayers.Sand.SurfaceTextures.Count == 0)
						{
							addNewTexture = true;
						}

						
						Vector2 textureTileSize = new Vector2(0, 0);
						Texture2D mainTexture = new Texture2D(0, 0);
						Texture2D normalMap = new Texture2D(0, 0);
						float textureOpacity = 1;
						TextureModes textureMode = TextureModes.Fixed;
						int newIndex = -1;
						if (tt.surfaceLayers.Sand.SurfaceTextures.Count > 0)
						{
							GUILayout.BeginVertical("box");

							GUILayout.BeginHorizontal();
							//GUILayout.Label("Texture Tile Size", GUILayout.Width(150));
							EditorGUILayout.PrefixLabel("Texture Tile Size");
							textureTileSize = EditorGUILayout.Vector2Field("", tt.surfaceLayers.Sand.SurfaceTextures[textureIndexInt].textureTileSize);
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							//GUILayout.Label("Texture", GUILayout.Width(150));
							EditorGUILayout.PrefixLabel("Texture");
							//mainTexture
							mainTexture = (Texture2D)EditorGUILayout.ObjectField(tt.surfaceLayers.Sand.SurfaceTextures[textureIndexInt].Texture, typeof(Texture2D), true);
							//tt.surfaceLayers.Sand.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].Texture = (Texture2D)EditorGUILayout.ObjectField(tt.surfaceLayers.Sand.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].Texture, typeof(Texture2D), true);
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							//GUILayout.Label("Normal Map", GUILayout.Width(150));
							EditorGUILayout.PrefixLabel("Normal Map");
							normalMap = (Texture2D)EditorGUILayout.ObjectField(tt.surfaceLayers.Sand.SurfaceTextures[textureIndexInt].NormalMap, typeof(Texture2D), true);
							GUILayout.EndHorizontal();

							
							GUILayout.EndVertical();
						}



						GUILayout.EndVertical();
						if (EditorGUI.EndChangeCheck() || updatePerlinMap)
						{
							Undo.RecordObject(tt, "TerraTex Sand Surface Settings Changed");

							tt.ui.textureIndex[tt.ui.subTab] = textureIndexInt;


							if (tt.surfaceLayers.Sand.SurfaceTextures.Count > 0)
							{
								tt.surfaceLayers.Sand.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].TextureMode = textureMode;

								tt.surfaceLayers.Sand.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].textureTileSize = textureTileSize;
								tt.surfaceLayers.Sand.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].Texture = mainTexture;
								tt.surfaceLayers.Sand.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].NormalMap = normalMap;
								
								tt.surfaceLayers.Sand.SurfaceTextures[tt.ui.textureIndex[tt.ui.subTab]].TextureOpacity = textureOpacity;

							}

							if (addNewTexture)
							{
								tt.surfaceLayers.Sand.SurfaceTextures.Add(new SurfaceTexture());
								tt.ui.textureIndex[tt.ui.subTab] = tt.surfaceLayers.Sand.SurfaceTextures.Count - 1;
								//update = true;
								newTextureUpdate = true;

							}

							if (newIndex != -1)
								tt.ui.textureIndex[tt.ui.subTab] = newIndex;


						}

						count[0] = tt.surfaceLayers.Sand.SurfaceTextures.Count;
						break;
					}
				#endregion

			}

		}


	}
}