using Raylib_cs;
using ImGuiNET;
using System.Numerics;

namespace Core;

/// <summary>
/// A round astronomical body.
/// </summary>
public class Planet
{
    public class Region
    {
        public readonly Vector3 pos;
        public readonly int height;
        public Region(Vector3 pos, int height)
        {
            this.pos = pos;
            this.height = height;
        }
    }

    public readonly int size;
    private readonly uint initialSeed;
    private uint seed;
    public readonly float maxHeight;
    private byte[] height;

    private Texture2D testTex;

    public Model planetModel;
    private Texture2D planetTex;
    
    private Model skyboxModel;
    private Texture2D skyboxTex;
    
    private Planet.Region[] regions;
    private Planet.Region[] subregions;
    
    private Vector3 moonPos;
    private float moonOrbitPos;
    private float moonOrbitDistance = 1f;
    private float moonSize = 0.05f;
    //private float moonOrbitSpeed = 1f / 29f;
    
    public Vector3 sunPos;
    private float sunOrbitPos;
    private float sunOrbitDistance = 10f;
    private float sunSize = 0.5f;
    //private float sunOrbitSpeed = 0.005f;

    //private Mesh triangleMesh;

    // Constructor
    public Planet(uint seed, int size, float maxHeight = 0.1f)
    {
        this.initialSeed = this.seed = seed;
        this.size = size;
        this.maxHeight = maxHeight;
        Generate();
    }

    // Generate the planet
    private unsafe void Generate()
    {
        Logger.Log("Generating planet (" + seed.ToString() + ")");

        testTex = Raylib.LoadTexture("./assets/textures/uv_checker_cubemap_1024.png");
        
        // Generate planet
        GenerateRegions();
        MakeHeight();
        planetModel = Raylib.LoadModelFromMesh(MakeMesh(renderSize: 100));
        
        // Generate planet texture
        MakePlanetTex(out planetTex);
        Raylib.SetMaterialTexture(ref planetModel, 0, MaterialMapIndex.Albedo, ref planetTex);
        
        // Generate skybox
        MakeSkyboxTex(out skyboxTex);
        skyboxModel = Raylib.LoadModelFromMesh(MakeSkyboxMesh());
        Raylib.SetMaterialTexture(ref skyboxModel, 0, MaterialMapIndex.Albedo, ref skyboxTex);

        // Generate triangle
        //triangleMesh = MakeTriangleMesh();
    }
    
    // Called every frame
    public void Update(float deltaTime)
    {
        float sunPhase = (float)Game.time % (60f * 60f * 24f) / (60f * 60f * 24f);
        float moonPhase = (float)Game.time % (60f * 60f * 24f * 29.5f) / (60f * 60f * 24f * 29.5f);
        //moonOrbitPos += moonOrbitSpeed;
        //moonOrbitPos %= MathF.PI * 2;
        //moonOrbitPos = (float)time % (60f * 60f * 24f) * MathF.PI * 2;
        //sunOrbitPos += sunOrbitSpeed;
        //sunOrbitPos %= MathF.PI * 2;
        sunOrbitPos = sunPhase * MathF.PI * 2f;
        moonOrbitPos = sunOrbitPos + moonPhase * MathF.PI * 2f;
        sunPos.X = MathF.Sin(sunOrbitPos);
        sunPos.Y = MathF.Sin(moonPhase * MathF.PI * 2f) * 0.02f;
        sunPos.Z = MathF.Cos(sunOrbitPos);
        
        moonPos.X = MathF.Sin(moonOrbitPos);
        //moonPos.Y = MathF.Sin(moonPhase * MathF.PI * 2f) * 0.02f;
        moonPos.Z = MathF.Cos(moonOrbitPos);
    }

    // Render 3D graphics
    public void Render3D()
    {
        Raylib.DrawModel(planetModel, Vector3.Zero, 1f, Color.White);
        //Raylib.DrawSphere(moonPos * (1f + moonOrbitDistance), moonSize, Color.Red);
        Raylib.DrawModelEx(planetModel, moonPos * (1f + moonOrbitDistance), Vector3.UnitY, 90f - moonOrbitPos * 180 / MathF.PI * 0.7f, new Vector3(moonSize), Color.Red);
        // Matrix4x4 matrix = Matrix4x4.Identity * Raymath.MatrixTranslate(0f, 1.6f, 0f) * Raymath.MatrixRotateX(MathF.PI) * Raymath.MatrixScale(0.1f, 0.1f, 0.1f);
        //unsafe { Raylib.DrawMesh(triangleMesh, model.Materials[0], matrix); }
        Raylib.DrawModel(skyboxModel, Vector3.Zero, 100f, Color.White);
        Raylib.DrawSphere(sunPos * (1f + sunOrbitDistance), sunSize, Color.Yellow);
        if (Game.debug)
        {
            Raylib.DrawLine3D(sunPos * (1f + sunOrbitDistance), Vector3.Zero, Color.Yellow);
            Raylib.DrawCircle3D(sunPos * (1f + sunOrbitDistance), 1f + sunOrbitDistance, Vector3.UnitX, 90f, Color.Yellow); 
            Raylib.DrawCircle3D(Vector3.Zero, 1f + moonOrbitDistance, Vector3.UnitX, 90f, Color.Red); 
            

            Raylib.DrawLine3D(new Vector3(-1.5f, 0f, 0f), new Vector3(1.5f, 0f, 0f), Color.Red);
            Raylib.DrawLine3D(new Vector3(0f, -1.5f, 0f), new Vector3(0f, 1.5f, 0f), Color.Blue);
            Raylib.DrawLine3D(new Vector3(0f, 0f, -1.5f), new Vector3(0f, 0f, 1.5f), Color.Green);
            // foreach (Planet.Region region in regions)    { Raylib.DrawSphereEx(region.pos, 0.02f, 10, 10, Color.Maroon); }
            // foreach (Planet.Region region in subregions) { Raylib.DrawSphereEx(region.pos, 0.01f, 10, 10, Color.Violet); }
        }
    }
    
    // Render 2D graphics
    public void Render2D()
    {
        //Raylib.DrawTextureEx(texture, new Vector2(0f, 0f), 0f, 0.175f, Color.White);
    }

    public void RenderImGui()
    {
        if (ImGui.Begin("Planet"))
        {
            ImGui.Text("Seed:       " + initialSeed.ToString()) ;
            ImGui.Text("Size:       " + size.ToString()) ;
            ImGui.Text("Regions:    " + regions.Length.ToString()) ;
            ImGui.Text("Subregions: " + subregions.Length.ToString()) ;
        }
    }

    // Free allocated memory
    public void Exit()
    {
        //Raylib.UnloadMesh(triangleMesh);
        Raylib.UnloadTexture(testTex);
        Raylib.UnloadTexture(planetTex);
        Raylib.UnloadTexture(skyboxTex);
        Raylib.UnloadModel(planetModel);
        Raylib.UnloadModel(skyboxModel);
        //Raylib.UnloadMaterial(material);
    }

    // Returns the planet surface coordinate for a given cube face index & grid position
    public (int XDeg, float Xmin, int Ydeg, float Ymin) GetGcs((int Face, int X, int Y) pos)
    {
        Vector2 result = new Vector2((float)pos.X, pos.Face > 3 ? (float)pos.Y : (float)pos.Y + (((float)size + 1f) * 0.5f));
        int equator = pos.Face;
        if (pos.Face > 3)
        {
            Vector2 tFace = new Vector2((float)size * 0.5f,                 0f);
            Vector2 bFace = new Vector2((float)size * 0.5f,        (float)size);
            Vector2 lFace = new Vector2(                0f, (float)size * 0.5f);
            Vector2 rFace = new Vector2(       (float)size, (float)size * 0.5f);
            float tDist = Vector2.Distance(result, tFace);
            float bDist = Vector2.Distance(result, bFace);
            float rDist = Vector2.Distance(result, rFace);
            float lDist = Vector2.Distance(result, lFace);
            equator = 3;
            float dist = tDist;
            if (bDist < dist) { dist = bDist; equator = 1; }
            if (rDist < dist) { dist = rDist; equator = (pos.Face == 4 ? 0 : 2); }
            if (lDist < dist) { dist = lDist; equator = (pos.Face == 4 ? 2 : 0); }
            if (pos.Face == 4)
            {
                switch (equator)
                {
                    case 1:
                        result.Y = 1f + (float)pos.Y - ((float)size + 1f) * 0.5f;
                        break;
                    case 0:
                        result.X = (float)size - (float)pos.Y;
                        result.Y = 1f + (float)pos.X - (((float)size + 1f) * 0.5f);
                        break;
                    case 3:
                        result.X = (float)size - (float)pos.X;
                        result.Y = (((float)size + 1f) * 0.5f) - (float)pos.Y;
                        break;
                    case 2:
                        result.X = (float)pos.Y;
                        result.Y = (((float)size + 1f) * 0.5f) - (float)pos.X;
                        break;
                }
            }
            else if (pos.Face == 5)
            {
                result.Y = (((float)size + 1f) * 1.5f) - 1f;
                switch (equator)
                {
                    case 3:
                        result.X = (float)size - (float)pos.X;
                        result.Y += (float)pos.Y;
                        break;
                    case 2:
                        result.X = (float)pos.Y;
                        result.Y += ((float)size - (float)pos.X);
                        break;
                    case 1:
                        result.X = (float)pos.X;
                        result.Y += (float)size - (float)pos.Y;
                        break;
                    case 0: 
                        result.X = (float)size - (float)pos.Y;
                        result.Y += (float)pos.X;
                        break;
                }
            }
        }
        // Handle equator sides
        switch (equator)
        {
            case 3: result.X += result.X < (float)size / 2f ? (float)size * 3.5f : (float)size * -0.5f; break;
            case 2: result.X += (float)size * 0.5f; break;
            case 1: result.X += (float)size * 1.5f; break;
            case 0: result.X += (float)size * 2.5f; break;
        }

        result =  result / (float)size * 90f - new Vector2(180f, 90f);
        // if (result.X <  -90f) { result.X =  -90f;}
        // if (result.X >   90f) { result.X =   90f;}
        // if (result.Y < -180f) { result.X = -180f;}
        // if (result.X >  180f) { result.X =  180f;}
        int xDeg = (int)MathF.Truncate(result.X);
        float xMin = MathF.Round((result.X - (float)xDeg) * 60f, 2);
        int yDeg = (int)MathF.Truncate(result.Y);
        float yMin = MathF.Round((result.Y - (float)yDeg) * 60f, 2);
        return (Xdeg: xDeg, Xmin: xMin, Ydeg: yDeg, Ymin: yMin);
    }

    // Place regions and sub-regions on the planet surface
    private void GenerateRegions()
    {
        uint minNumRegions           = 4;
        uint maxNumRegions           = 32;
        uint maxNumSubregions        = 4;
        int numRegions    = Lfsr.MakeInt(ref seed, min: minNumRegions, max: maxNumRegions);
        int numSubregions = Lfsr.MakeInt(ref seed, min: (uint)numRegions, max: (uint)(numRegions * maxNumSubregions));
        PlaceRegions(numRegions, ref regions);
        PlaceRegions(numSubregions, ref subregions);
    }
    
    // Place regions on the planet surface
    private void PlaceRegions(int num, ref Planet.Region[] result)
    {
        result = new Planet.Region[num];
        float minDistance = 1f / num;
        for (int i = 0; i < num; i++)
        {
            bool placed = false;
            while (!placed)
            {
                // Select a random point
                Vector3 pos = Vector3.Normalize(new Vector3((float)Lfsr.MakeInt(ref seed, true), (float)Lfsr.MakeInt(ref seed, true), (float)Lfsr.MakeInt(ref seed, true)));
                
                // Discard the region if position already is occupied
                bool discard = false;
                foreach (Planet.Region? region in result) { if (region != null && Vector3.Distance(pos, region.pos) < minDistance) { discard = true; Logger.Err("Planet region discarded (too close)"); break; } }
                
                // Add the region if the position is free
                if (!discard)
                {
                    placed = true;
                    result[i] = new Planet.Region(pos, Lfsr.MakeInt(ref seed, max: (uint)255));
                }
            }
        }
    }

    // Find the three closest regions from a given point on the planet surface
    private void FindRegions(Vector3 pos, ref Planet.Region[] source, ref Planet.Region[] result, ref float[] resultDist, ref float[] resultRatio)
    {
        bool[] found = new bool[3];
        for (int i = 0; i < source.Length; i++)
        {
            float dist = Vector3.Distance(pos, source[i].pos);
            if (!found[0] || dist < resultDist[0])
            {
                if (found[0])
                {
                    if (found[1])
                    {
                        found[2] = true;
                        result[2] = result[1];
                        resultDist[2] = resultDist[1];
                    }
                    found[1] = true;
                    result[1] = result[0];
                    resultDist[1] = resultDist[0];
                }
                found[0] = true;
                result[0] = source[i];
                resultDist[0] = dist;
            }
            if (source[i] != result[0] && (!found[1] || dist < resultDist[1]))
            {
                if (found[1])
                {
                    found[2] = true;
                    result[2] = result[1];
                    resultDist[2] = resultDist[1];
                }
                found[1] = true;
                result[1] = source[i];
                resultDist[1] = dist;
            }
            if (source[i] != result[0] && source[i] != result[1] && (!found[2] || dist < resultDist[2]))
            {
                found[2] = true;
                result[2] = source[i];
                resultDist[2] = dist;
            }
        }
        float totalDist = resultDist[0] + resultDist[1] + resultDist[2];
        resultRatio[0] = resultDist[0] / totalDist;
        resultRatio[1] = resultDist[1] / totalDist;
        resultRatio[2] = resultDist[2] / totalDist;
    }
    
    // Returns the heightmap colorbuffer index for a given cube face index & grid position
    private int GetCubemapIndex(int size, int face, int x, int y)
    {
        int indexOffset = size * (face % 3) + (face > 2 ? (size * 3) * size : 0);
        return indexOffset + ((size * 3) * y) + x;
    }

    private unsafe void MakeSkyboxTex(out Texture2D result)
    {
        int imgSize = 512;
        int imgWidth = imgSize * 3;
        int imgHeight = imgSize * 2;
        byte* pixels = (byte*)Raylib.MemAlloc(imgWidth * imgHeight * sizeof(byte));
        for (int face = 0; face < 6; face++)
        {
            for (int y = 0; y < imgSize; y++)
            {
                for (int x = 0; x < imgSize; x++)
                {
                    float intensity = (float)x / (float)imgSize;
                    pixels[GetCubemapIndex(imgSize, face, x, y)] = (byte)(intensity * 255f);
                }
            }
        }
        Image skyboxImg = new Image
        {
            Data = pixels,
            Width = imgWidth,
            Height = imgHeight,
            Format = PixelFormat.UncompressedGrayscale,
            Mipmaps = 1,
        };
        result = Raylib.LoadTextureFromImage(skyboxImg);
        Raylib.UnloadImage(skyboxImg);
    }

    private unsafe void MakePlanetTex(out Texture2D result)
    {
        byte* pixels = (byte*)Raylib.MemAlloc(height.Length * sizeof(byte));
        for (int i = 0; i < height.Length; i++) { pixels[i] = height[i]; }
        Image planetImg = new Image
        {
            Data = pixels,
            Width = (size + 1) * 3,
            Height = (size + 1) * 2,
            Format = PixelFormat.UncompressedGrayscale,
            Mipmaps = 1,
        };
        result = Raylib.LoadTextureFromImage(planetImg);
        Raylib.UnloadImage(planetImg);
    }

    // Procedural generation of an 8-bit/1 channel grayscale heightmap image for the planet
    private void MakeHeight()
    {
        // Border sizes 
        float borderSizeRegion       = 0.15f;
        float borderSizeSubregion    = 0.45f;
        float edgeSizeRegion         = 0.3f;
        float edgeSizeSubregion      = 0.3f;
        // Noise set up
        float noiseAmountRegion      = 1f;
        float noiseSizeRegion        = 3f;
        float noiseAmountSubregion   = 1f;
        float noiseSizeSubregion     = 5f;
        float noiseSizeFractal       = 100f;
        // Height ratios
        float heightPercentNoise     = 0.025f;
        float heightPercentSubregion = 0.30f;
        float heightPercentRegion    = 1.0f - heightPercentNoise - heightPercentSubregion;

        // Create pixel array for the heightmap image
        int imgWidth = (size + 1) * 3;
        int imgHeight = (size + 1) * 2;
        height = new byte[imgWidth * imgHeight];
        
        // Generate noise seeds
        float noiseSeedRegion     = (float)Lfsr.MakeInt(ref seed);
        float noiseSeedSubregion  = (float)Lfsr.MakeInt(ref seed);
        float noiseSeedFractal    = (float)Lfsr.MakeInt(ref seed);
        
        // Iterate through every position on the cube
        for (int face = 0; face < 6; face++)
        {
            for (int y = 0; y < size + 1; y++)
            {
                for (int x = 0; x < size + 1; x++)
                {
                    // Set position
                    Vector2 normalizedPos = new Vector2((float)x, (float)y) / (float)size;
                    Vector3 pos = Vector3.Normalize(TransformCubeToSphere(Transform2DToCube(face, normalizedPos)) * (float) size);
                    
                    // Generate noise
                    float noiseRegion     = Perlin.Octave4(pos * noiseSizeRegion, noiseSeedRegion, octaves: 4);
                    float noiseSubregion  = Perlin.Octave4(pos * noiseSizeSubregion, noiseSeedSubregion, octaves: 6);
                    float noiseFractal    = Perlin.Octave4(pos * noiseSizeFractal, noiseSeedFractal, octaves: 3);
                   
                    // Set region positions
                    Vector3 posRegion    = pos + new Vector3(noiseAmountRegion * noiseRegion);
                    Vector3 posSubregion = pos + new Vector3(noiseAmountSubregion * noiseSubregion);

                    // Find the three closest regions from the current position on the planet surface
                    Planet.Region[] nearbyRegions     = new Region[3];
                    float[] nearbyRegionsDist  = new float[3];
                    float[] nearbyRegionsRatio = new float[3];
                    FindRegions(posRegion, ref regions, ref nearbyRegions, ref nearbyRegionsDist, ref nearbyRegionsRatio);
                    
                    // Find the three closest subregions from the current position on the planet surface
                    Planet.Region[] nearbySubregions     = new Region[3];
                    float[] nearbySubregionsDist  = new float[3];
                    float[] nearbySubregionsRatio = new float[3];
                    FindRegions(posSubregion, ref subregions, ref nearbySubregions, ref nearbySubregionsDist, ref nearbySubregionsRatio);
                    
                    // Set region height
                    float borderRatioRegion = ((nearbyRegionsDist[0] / nearbyRegionsDist[1]) > 1.0f - borderSizeRegion) ? Smoothstep.QuadraticRational(((nearbyRegionsDist[0] / nearbyRegionsDist[1]) - (1.0f - borderSizeRegion)) / borderSizeRegion) : 0.0f;
                    float heightRegion = ((float)nearbyRegions[0].height * (1f - borderRatioRegion)) + ((((float)nearbyRegions[0].height + (float)nearbyRegions[1].height) / 2f) * borderRatioRegion);
                    if (nearbyRegionsRatio[0] > edgeSizeRegion && nearbyRegionsRatio[1] > edgeSizeRegion && nearbyRegionsRatio[2] > edgeSizeRegion) 
                    { 
                        float edgeRatioRegion = Smoothstep.QuadraticRational((nearbyRegionsRatio[0] - edgeSizeRegion) / (0.3333334f - edgeSizeRegion));
                        heightRegion = (heightRegion * (1f - edgeRatioRegion)) + ((((float)nearbyRegions[0].height + (float)nearbyRegions[1].height + (float)nearbyRegions[2].height) / 3f) * edgeRatioRegion); 
                    }

                    // Set subregion height
                    float borderRatioSubregion = ((nearbySubregionsDist[0] / nearbySubregionsDist[1]) > 1.0f - borderSizeSubregion) ? Smoothstep.QuadraticRational(((nearbySubregionsDist[0] / nearbySubregionsDist[1]) - (1.0f - borderSizeSubregion)) / borderSizeSubregion) : 0.0f;
                    float heightSubregion = ((float)nearbySubregions[0].height * (1f - borderRatioSubregion)) + ((((float)nearbySubregions[0].height + (float)nearbySubregions[1].height) / 2f) * borderRatioSubregion);
                    if (nearbySubregionsRatio[0] > edgeSizeSubregion && nearbySubregionsRatio[1] > edgeSizeSubregion && nearbySubregionsRatio[2] > edgeSizeSubregion) 
                    { 
                        float edgeRatioSubregion = Smoothstep.QuadraticRational((nearbySubregionsRatio[0] - edgeSizeSubregion) / (0.3333334f - edgeSizeSubregion));
                        heightSubregion = (heightSubregion * (1f - edgeRatioSubregion)) + ((((float)nearbySubregions[0].height + (float)nearbySubregions[1].height + (float)nearbySubregions[2].height) / 3f) * edgeRatioSubregion); 
                    }

                    // Store final height in the height array
                    height[GetCubemapIndex(size + 1, face, x, y)] = (byte)((heightSubregion * (heightPercentSubregion * (0.1f + Smoothstep.QuadraticRational((heightRegion / 255f)) * 0.9f))) + (heightRegion * heightPercentRegion) + ((noiseFractal * 255f) * heightPercentNoise));
                }
            }
        }
    }

    // Transform a given cube face index & grid position to a 3D point in local space
    private unsafe Vector3 Transform2dTo3d(int face, Vector2 pos, ref byte[] height)
    {
        float foundHeight = ((float)height[GetCubemapIndex(size +  1, face, (int)pos.X, (int)pos.Y)] / 255f) * maxHeight;
        return TransformCubeToSphere(Transform2DToCube(face, pos / (float)size)) * (1f - maxHeight + foundHeight);
    }
    
    // Project the planet as a 3D unfolded cube
    // private Vector3 Transform2DToFlat(int face, Vector2 pos)
    // {
    //     Vector3 result = new Vector3(pos.X - (float)size * 2, 0f, pos.Y - (float)size);
    //     switch (face)
    //     {
    //         case 1:
    //             result += new Vector3((float)size, 0.0f, 0.0f);
    //             break;
    //         case 2:
    //             result += new Vector3((float)size * 2, 0.0f, 0.0f);
    //             break;
    //         case 3:
    //             result += new Vector3((float)size * 3, 0.0f, 0.0f);
    //             break;
    //         case 4:
    //             result += new Vector3((float)size, 0.0f, -(float)size);
    //             break;
    //         case 5:
    //             result += new Vector3((float)size, 0.0f, (float)size);
    //             break;
    //     }
    //     return result;
    // }

    // Project the planet as a 3D cube
    private Vector3 Transform2DToCube(int face, Vector2 pos)
    {
        switch (face)
        {
            case 0: return new Vector3(       -0.5f, 0.5f - pos.Y, pos.X - 0.5f);
            case 1: return new Vector3(0.5f - pos.X, 0.5f - pos.Y,        -0.5f);
            case 2: return new Vector3(        0.5f, 0.5f - pos.Y, 0.5f - pos.X);
            case 3: return new Vector3(pos.X - 0.5f, 0.5f - pos.Y,         0.5f);
            case 4: return new Vector3(0.5f - pos.X,         0.5f, 0.5f - pos.Y);
            case 5: return new Vector3(pos.X - 0.5f,        -0.5f, 0.5f - pos.Y);
        }
        return new Vector3();
    }

    // Cube to sphere projection
    private Vector3 TransformCubeToSphere(Vector3 pos)
    {
        pos *= 2f;
        return new Vector3
        (
            pos.X * MathF.Sqrt(1f - ((pos.Y * pos.Y) + (pos.Z * pos.Z)) / 2f + ((pos.Y * pos.Y) * (pos.Z * pos.Z)) / 3f),
            pos.Y * MathF.Sqrt(1f - ((pos.X * pos.X) + (pos.Z * pos.Z)) / 2f + ((pos.X * pos.X) * (pos.Z * pos.Z)) / 3f),
            pos.Z * MathF.Sqrt(1f - ((pos.X * pos.X) + (pos.Y * pos.Y)) / 2f + ((pos.X * pos.X) * (pos.Y * pos.Y)) / 3f)
        );
    }

    public (int, int, int) TransformCubeTo2D(Vector3 pos)
    {
        Vector2 result = new Vector2(0.5f, 0.5f);
        int face = 0;
        if      (pos.Z == -0.5f) { face = 1; }
        else if (pos.X ==  0.5f) { face = 2; }
        else if (pos.Z ==  0.5f) { face = 3; }
        else if (pos.Y ==  0.5f) { face = 4; }
        else if (pos.Y == -0.5f) { face = 5; }
        switch (face)
        {
            case 0: result += new Vector2( pos.Z, -pos.Y); break;
            case 1: result += new Vector2(-pos.X, -pos.Y); break;
            case 2: result += new Vector2(-pos.Z, -pos.Y); break;
            case 3: result += new Vector2( pos.X, -pos.Y); break;
            case 4: result += new Vector2(-pos.X, -pos.Z); break;
            case 5: result += new Vector2( pos.X, -pos.Z); break;
        }
        result *= (float)size;
        return (face, (int)result.X, (int)result.Y);
    }
    
    public Vector3 TransformSphereToCube(Vector3 pos)
    {
    
        float inverseSqrt2 = 0.70710676908493042f;
    
        float x = pos.X;
        float y = pos.Y;
        float z = pos.Z;

        float fx = MathF.Abs(x);
        float fy = MathF.Abs(y);
        float fz = MathF.Abs(z);


        if (fy >= fx && fy >= fz) {
            float a2 = x * x * 2f;
            float b2 = z * z * 2f;
            float inner = -a2 + b2 -3f;
            float innersqrt = -MathF.Sqrt((inner * inner) - 12f * a2);

            if(x == 0f || x == -0f) {
                pos.X = 0f;
            }
            else {
                pos.X = MathF.Sqrt(innersqrt + a2 - b2 + 3f) * inverseSqrt2;
            }

            if(z == 0f || z == -0f) {
                pos.Z = 0f;
            }
            else {
                pos.Z = MathF.Sqrt(innersqrt - a2 + b2 + 3f) * inverseSqrt2;
            }

            if(pos.X > 1f) pos.X = 1f;
            if(pos.Z > 1f) pos.Z = 1f;

            if(x < 0f) pos.X = -pos.X;
            if(z < 0f) pos.Z = -pos.Z;

            if (y > 0f) {
                // top face
                pos.Y = 1.0f;
            }
            else {
                // bottom face
                pos.Y = -1.0f;
            }
        }
        else if (fx >= fy && fx >= fz) {
            float a2 = y * y * 2f;
            float b2 = z * z * 2f;
            float inner = -a2 + b2 -3f;
            float innersqrt = -MathF.Sqrt((inner * inner) - 12f * a2);

            if(y == 0f || y == -0f) {
                pos.Y = 0f;
            }
            else {
                pos.Y = MathF.Sqrt(innersqrt + a2 - b2 + 3f) * inverseSqrt2;
            }

            if(z == 0f || z == -0f) {
                pos.Z = 0f;
            }
            else {
                pos.Z = MathF.Sqrt(innersqrt - a2 + b2 + 3f) * inverseSqrt2;
            }

            if(pos.Y > 1f) pos.Y = 1f;
            if(pos.Z > 1f) pos.Z = 1f;

            if(y < 0f) pos.Y = -pos.Y;
            if(z < 0f) pos.Z = -pos.Z;

            if (x > 0f) {
                // right face
                pos.X = 1f;
            }
            else {
                // left face
                pos.X = -1f;
            }
        }
        else {
            float a2 = x * x * 2f;
            float b2 = y * y * 2f;
            float inner = -a2 + b2 -3f;
            float innersqrt = -MathF.Sqrt((inner * inner) - 12f * a2);

            if(x == 0f || x == -0f) {
                pos.X = 0f;
            }
            else {
                pos.X = MathF.Sqrt(innersqrt + a2 - b2 + 3f) * inverseSqrt2;
            }

            if(y == 0f || y == -0f) {
                pos.Y = 0f;
            }
            else {
                pos.Y = MathF.Sqrt(innersqrt - a2 + b2 + 3f) * inverseSqrt2;
            }
        
            if(pos.X > 1f) pos.X = 1f;
            if(pos.Y > 1f) pos.Y = 1f;

            if(x < 0f) pos.X = -pos.X;
            if(y < 0f) pos.Y = -pos.Y;

            if (z > 0f) {
                // front face
                pos.Z = 1f;
            }
            else {
                // back face
                pos.Z = -1f;
            }
        }
        
        return pos * 0.5f;
    }
    private unsafe Mesh MakeTriangleMesh()
    {
        int numVerts = 5;
        int numTris = 18;

        Mesh mesh = new(numVerts, numTris);
        mesh.AllocVertices();
        mesh.AllocIndices();
        Span<Vector3> vertices = mesh.VerticesAs<Vector3>();
        Span<ushort> indices = mesh.IndicesAs<ushort>();
        
        vertices[0]   = new Vector3(    0f, 0f,     0f);
        vertices[1]   = new Vector3(-0.25f, 1f,  0.25f);
        vertices[2]   = new Vector3(-0.25f, 1f, -0.25f);
        vertices[3]   = new Vector3( 0.25f, 1f, -0.25f);
        vertices[4]   = new Vector3( 0.25f, 1f,  0.25f);
        
        indices[0]  = (ushort)2;
        indices[1]  = (ushort)1;
        indices[2]  = (ushort)4;

        indices[3]  = (ushort)2;
        indices[4]  = (ushort)4;
        indices[5]  = (ushort)3;

        indices[6]  = (ushort)1;
        indices[7]  = (ushort)0;
        indices[8]  = (ushort)4;

        indices[9]  = (ushort)4;
        indices[10] = (ushort)0;
        indices[11] = (ushort)3;

        indices[12] = (ushort)3;
        indices[13] = (ushort)0;
        indices[14] = (ushort)2;

        indices[15] = (ushort)2;
        indices[16] = (ushort)0;
        indices[17] = (ushort)1;

        Raylib.UploadMesh(ref mesh, false);
        return mesh;
    } 

    private unsafe Mesh MakeSkyboxMesh()
    {
        int numVerts = 24;
        int numTris = 36;

        Mesh mesh = new(numVerts, numTris);
        mesh.AllocVertices();
        mesh.AllocIndices();
        mesh.AllocTexCoords();
        Span<Vector3> vertices = mesh.VerticesAs<Vector3>();
        Span<ushort> indices = mesh.IndicesAs<ushort>();
        Span<Vector2> texcoords = mesh.TexCoordsAs<Vector2>();
        
        float texCoordX = 1f / 3f;
        float texCoordY = 1f / 2f;

        // 0: LEFT
        vertices[0]   = new Vector3(-1f,  1f,  1f);
        vertices[1]   = new Vector3(-1f,  1f, -1f);
        vertices[2]   = new Vector3(-1f, -1f,  1f);
        vertices[3]   = new Vector3(-1f, -1f, -1f);
        texcoords[0]  = new Vector2(texCoordX * 0f, texCoordY * 0f);
        texcoords[1]  = new Vector2(texCoordX * 1f, texCoordY * 0f);
        texcoords[2]  = new Vector2(texCoordX * 0f, texCoordY * 1f);
        texcoords[3]  = new Vector2(texCoordX * 1f, texCoordY * 1f);
        indices[0]    = (ushort)0;
        indices[1]    = (ushort)2;
        indices[2]    = (ushort)3;
        indices[3]    = (ushort)0;
        indices[4]    = (ushort)3;
        indices[5]    = (ushort)1;

        // 1: REAR
        vertices[4]   = new Vector3(-1f,  1f, -1f);
        vertices[5]   = new Vector3( 1f,  1f, -1f);
        vertices[6]   = new Vector3(-1f, -1f, -1f);
        vertices[7]   = new Vector3( 1f, -1f, -1f);
        texcoords[4]  = new Vector2(0f, 0f);
        texcoords[5]  = new Vector2(0f, 0f);
        texcoords[6]  = new Vector2(0f, 0f);
        texcoords[7]  = new Vector2(0f, 0f);
        texcoords[4]  = new Vector2(texCoordX * 1f, texCoordY * 0f);
        texcoords[5]  = new Vector2(texCoordX * 2f, texCoordY * 0f);
        texcoords[6]  = new Vector2(texCoordX * 1f, texCoordY * 1f);
        texcoords[7]  = new Vector2(texCoordX * 2f, texCoordY * 1f);
        indices[6]    = (ushort)4;
        indices[7]    = (ushort)6;
        indices[8]    = (ushort)7;
        indices[9]    = (ushort)4;
        indices[10]   = (ushort)7;
        indices[11]   = (ushort)5;
        
        // 2: RIGHT
        vertices[8]   = new Vector3( 1f,  1f, -1f);
        vertices[9]   = new Vector3( 1f,  1f,  1f);
        vertices[10]  = new Vector3( 1f, -1f, -1f);
        vertices[11]  = new Vector3( 1f, -1f,  1f);
        texcoords[8]  = new Vector2(texCoordX * 2f, texCoordY * 0f);
        texcoords[9]  = new Vector2(texCoordX * 3f, texCoordY * 0f);
        texcoords[10] = new Vector2(texCoordX * 2f, texCoordY * 1f);
        texcoords[11] = new Vector2(texCoordX * 3f, texCoordY * 1f);
        indices[12]   = (ushort)8;
        indices[13]   = (ushort)10;
        indices[14]   = (ushort)11;
        indices[15]   = (ushort)8;
        indices[16]   = (ushort)11;
        indices[17]   = (ushort)9;
        
        // 3: FRONT
        vertices[12]  = new Vector3( 1f,  1f,  1f);
        vertices[13]  = new Vector3(-1f,  1f,  1f);
        vertices[14]  = new Vector3( 1f, -1f,  1f);
        vertices[15]  = new Vector3(-1f, -1f,  1f);
        texcoords[12] = new Vector2(texCoordX * 0f, texCoordY * 1f);
        texcoords[13] = new Vector2(texCoordX * 1f, texCoordY * 1f);
        texcoords[14] = new Vector2(texCoordX * 0f, texCoordY * 2f);
        texcoords[15] = new Vector2(texCoordX * 1f, texCoordY * 2f);
        indices[18]   = (ushort)12;
        indices[19]   = (ushort)14;
        indices[20]   = (ushort)15;
        indices[21]   = (ushort)12;
        indices[22]   = (ushort)15;
        indices[23]   = (ushort)13;
        
        // 4: TOP
        vertices[16]  = new Vector3(-1f,  1f,  1f);
        vertices[17]  = new Vector3( 1f,  1f,  1f);
        vertices[18]  = new Vector3(-1f,  1f, -1f);
        vertices[19]  = new Vector3( 1f,  1f, -1f);
        texcoords[16] = new Vector2(texCoordX * 1f, texCoordY * 1f);
        texcoords[17] = new Vector2(texCoordX * 2f, texCoordY * 1f);
        texcoords[18] = new Vector2(texCoordX * 1f, texCoordY * 2f);
        texcoords[19] = new Vector2(texCoordX * 2f, texCoordY * 2f);
        indices[24]   = (ushort)16;
        indices[25]   = (ushort)18;
        indices[26]   = (ushort)19;
        indices[27]   = (ushort)16;
        indices[28]   = (ushort)19;
        indices[29]   = (ushort)17;
        
        // 5: BOTTOM
        vertices[20]  = new Vector3(-1f, -1f, -1f);
        vertices[21]  = new Vector3( 1f, -1f, -1f);
        vertices[22]  = new Vector3(-1f, -1f,  1f);
        vertices[23]  = new Vector3( 1f, -1f,  1f);
        texcoords[20] = new Vector2(texCoordX * 2f, texCoordY * 1f);
        texcoords[21] = new Vector2(texCoordX * 3f, texCoordY * 1f);
        texcoords[22] = new Vector2(texCoordX * 2f, texCoordY * 2f);
        texcoords[23] = new Vector2(texCoordX * 3f, texCoordY * 2f);
        indices[30]   = (ushort)20;
        indices[31]   = (ushort)22;
        indices[32]   = (ushort)23;
        indices[33]   = (ushort)20;
        indices[34]   = (ushort)23;
        indices[35]   = (ushort)21;

        Raylib.UploadMesh(ref mesh, false);
        return mesh;
    } 

    // Generate the 3D mesh for the planet
    private unsafe Mesh MakeMesh(int renderSize = 100)
    {
    // TODO: Implement this in the mesh generation
    // Find normal for a vertex
    // vN = Vector3Normalize(Vector3CrossProduct(Vector3Subtract(vB, vA), Vector3Subtract(vC, vA)));
   
        float sizeRatio = (float)renderSize / (float)size;
        float tileSize = 1.0f / sizeRatio;

        //renderSize = size;

        // Set mesh specs
        int faceNumVerts = (renderSize + 1) * (renderSize + 1);
        int numVerts = 6 * faceNumVerts;
        int numTris = 2 * (6 * (renderSize * renderSize));
        
        // Allocate memory for the mesh
        Mesh mesh = new(numVerts, numTris);
        mesh.AllocVertices();
        mesh.AllocTexCoords();
        mesh.AllocNormals();
        //mesh.AllocColors();
        mesh.AllocIndices();
        
        // Contigous regions of memory set aside for mesh data
        Span<Vector3> vertices = mesh.VerticesAs<Vector3>();
        Span<Vector2> texcoords = mesh.TexCoordsAs<Vector2>();
        //Span<Color> colors = mesh.ColorsAs<Color>();
        Span<ushort> indices = mesh.IndicesAs<ushort>();
        Span<Vector3> normals = mesh.NormalsAs<Vector3>();
        
        // Make lookup table for cube face vert index start position
        int[] faceVertIndex = new int[6];
        for (int face = 0; face < 6; face++){
            faceVertIndex[face] = faceNumVerts * face;
        }

        // Loop through all cube faces
        ushort vertIndex = 0;
        int triIndex = 0;
        //Color color = Color.White;
        for (int face = 0; face < 6; face++)
        {
            
            // switch (face)
            // {
            //     case 0: color = Color.SkyBlue; break;
            //     case 1: color = Color.Red; break;
            //     case 2: color = Color.Yellow; break;
            //     case 3: color = Color.Green; break;
            //     case 4: color = Color.Purple; break;
            //     case 5: color = Color.Beige; break;
            // }

            for (int y = 0; y < renderSize; y++)
            {
                //int yPos = y / sizeRatio;
                for (int x = 0; x < renderSize; x++)
                {
                    //int xPos = x / sizeRatio;
                    // Set UV cordinates for vertices on the current grid position
                    float texCoordXStart = 0f;
                    float texCoordYStart = 0f;
                    float texCoordXSize = 1f / 3f;
                    float texCoordYSize = 1f / 2f;
                    switch (face)
                    {
                        case 0:
                            texCoordXStart = 0f;
                            texCoordYStart = 0f;
                            break;
                        case 1:
                            texCoordXStart = texCoordXSize;
                            texCoordYStart = 0f;
                            break;
                        case 2:
                            texCoordXStart = texCoordXSize * 2;
                            texCoordYStart = 0f;
                            break;
                        case 3:
                            texCoordXStart = 0f;
                            texCoordYStart = texCoordYSize;
                            break;
                        case 4:
                            texCoordXStart = texCoordXSize;
                            texCoordYStart = texCoordYSize;
                            break;
                        case 5:
                            texCoordXStart = texCoordXSize * 2;
                            texCoordYStart = texCoordYSize;
                            break;
                    }  
                    float texCoordLeft   = texCoordXStart + ((float)x * (texCoordXSize / (float)renderSize));
                    float texCoordRight  = texCoordXStart + (((float)x + 1f) * (texCoordXSize / (float)renderSize));
                    float texCoordTop    = texCoordYStart + ((float)y * (texCoordYSize / (float)renderSize));
                    float texCoordBottom = texCoordYStart + (((float)y + 1f) * (texCoordYSize / (float)renderSize));
                    
                    // Set vertex indexes
                    int vertIndexStart = vertIndex;
                    ushort vertTopLeft = (ushort)(vertIndex - (renderSize + (x == 0 ? 1 : 2)));
                    ushort vertTopRight = (ushort)(vertTopLeft + 1);
                    ushort vertBottomLeft = (ushort)(vertIndex - 1);
                    if (y == 0)
                    {
                        vertTopLeft = (ushort)(vertIndex - (x == 1 ? 3 : 2));
                    }
                    if (y == 1)
                    {
                        vertTopLeft += (ushort)((x == 0 ? x + 1 : x) - renderSize);
                        vertTopRight = (ushort)(vertTopLeft + (x == 0 ? 1 : 2));
                    }

                    // Make top-left vertex
                    if (y == 0 && x == 0)
                    {
                        vertices[vertIndex] = Transform2dTo3d(face, new Vector2(x * tileSize, y * tileSize), ref height);
                        normals[vertIndex] = Vector3.Normalize(vertices[vertIndex]);
                        texcoords[vertIndex] = new(texCoordLeft, texCoordTop);
                        //colors[vertIndex] = color;
                        vertTopLeft = (ushort)(vertIndex);
                        vertIndex++;
                    }

                    // Make top-right vertex
                    if (y == 0)
                    {
                        vertices[vertIndex] = Transform2dTo3d(face, new Vector2((x + 1) * tileSize, y * tileSize), ref height);
                        normals[vertIndex] = Vector3.Normalize(vertices[vertIndex]);
                        texcoords[vertIndex] = new(texCoordRight, texCoordTop);
                        //colors[vertIndex] = color;
                        vertTopRight = (ushort)(vertIndex);
                        vertIndex++;
                    }

                    // Make bottom-left vertex
                    if (x == 0)
                    {
                        vertices[vertIndex] = Transform2dTo3d(face, new Vector2(x * tileSize, (y + 1) * tileSize), ref height);
                        normals[vertIndex] = Vector3.Normalize(vertices[vertIndex]);
                        texcoords[vertIndex] = new(texCoordLeft, texCoordBottom);
                        //colors[vertIndex] = color;
                        vertBottomLeft = (ushort)(vertIndex);
                        vertIndex++;
                    }
                    
                    // Make bottom-right vertex
                    vertices[vertIndex] = Transform2dTo3d(face, new Vector2((x + 1) * tileSize, (y + 1) * tileSize), ref height);
                    normals[vertIndex] = Vector3.Normalize(vertices[vertIndex]);
                    texcoords[vertIndex] = new(texCoordRight, texCoordBottom);
                    //colors[vertIndex] = color;
                    ushort vertBottomRight = (ushort)(vertIndex);
                    vertIndex++;

                    // Triangle 1 (Counter-clockwise winding order)
                    indices[triIndex]     = vertTopLeft;
                    indices[triIndex + 1] = vertBottomRight;
                    indices[triIndex + 2] = vertTopRight;
                    triIndex += 3;

                    // Triangle 2 (Counter-clockwise winding order)
                    indices[triIndex]     = vertTopLeft;
                    indices[triIndex + 1] = vertBottomLeft;
                    indices[triIndex + 2] = vertBottomRight;
                    triIndex += 3;
                }
            }
        }

        // Upload mesh data from CPU (RAM) to GPU (VRAM) memory
        Raylib.UploadMesh(ref mesh, false);

        // Return the finished mesh
        return mesh;
    }
}
