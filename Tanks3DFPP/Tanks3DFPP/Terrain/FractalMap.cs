using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Tanks3DFPP.Terrain.Interfaces;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP.Terrain
{
    /// <summary>
    /// Fractal height map, using the midpoint displacement algorithm, see http://www.lighthouse3d.com/opengl/terrain/index.php?mpd2.
    /// </summary>
    public class FractalMap : AsyncLoadingElement, IHeightMap
    {
        private Random rand;
        //private int roughness;
        private double displacement;
        private float maxHeight;
        private float[,] heightMap;
        private readonly int steps;
        private readonly int mapDimension;
        private int realMapDimension
        {
            get
            {
                return this.mapDimension + 1;
            }
        }
        private bool scribeMode;

        private int afterSmoothingLevel;

        public float[,] Data
        {
            get
            {
                return this.heightMap;
            }
        }

        public int Width
        {
            get
            {
                return this.realMapDimension;
            }
        }

        public int Height
        {
            get
            {
                return this.realMapDimension;
            }
        }

        public int HeightOffset { get; private set; }

        public int HighestPeak { get; private set; }

        public Vector3? Intersection(Ray ray, int scale = 1)
        {
            Vector3? result = null;
            while (result == null)
            {

            }

            return null;
        }

        private readonly string numericFormat;
        private float randomizedDisplacement
        {
            get
            {
                return (float)(this.displacement * (rand.NextDouble() - 0.5));
            }
        }

        private float getRoughness(float roughness)
        {
            return (float)(roughness * (rand.NextDouble() - 0.5));
        }

        public FractalMap(int size, int roughness, float maxHeight, int afterSmoothingLevel = 0, bool scribeMode = false)
        {
            if (this.scribeMode = scribeMode)
            {
                using (StreamWriter fw = new StreamWriter("heightMap_test.txt"))
                {
                    fw.Write("");
                }
            }

            this.afterSmoothingLevel = afterSmoothingLevel;
            this.HeightOffset = (int)maxHeight;
            rand = new Random();
            this.mapDimension = (1 << size);
            this.maxHeight = maxHeight;
            this.steps = size;
            numericFormat = string.Format("D{0}", maxHeight.ToString().Length);
            this.displacement = roughness;

        }

        public void Initialize()
        {
            Thread t = new Thread(() =>
                {
                    GenerateHeightData();
                    if (afterSmoothingLevel > 0)
                    {
                        this.SmoothTerrain(afterSmoothingLevel);
                    }

                    this.FireReady(this);
                });
            t.Start();
       
        }

        private void GenerateHeightData()
        {
            this.InitializeHeightMap();

            // iterate on the whole map
            this.IterateMPD(new Rectangle(0, 0, this.mapDimension, this.mapDimension), (float)this.displacement);
            if (this.scribeMode)
            {
                using (StreamWriter fw = new StreamWriter("heightMap_test.txt", true))
                {
                    fw.WriteLine(this.ToString());
                }
            }
        }

        // TODO: Investigate the impact of changing roughness.
        private void IterateMPD(Rectangle area, float roughness)
        {
            if (area.Width < 2 || area.Height < 2)
            {
                return;
            }

            PerformSquareStep(area, roughness);
            PerformDiamondStep(area, roughness);
            Rectangle topLeftArea = new Rectangle(area.Left, area.Top, area.Width / 2, area.Height / 2), // Vit: /4 changed to /2
                        topRightArea = new Rectangle(area.Center.X, area.Top, area.Width / 2, area.Height / 2),
                        bottomLeftArea = new Rectangle(area.Left, area.Center.Y, area.Width / 2, area.Height / 2),
                        bottomRightArea = new Rectangle(area.Center.X, area.Center.Y, area.Width / 2, area.Height / 2);

            ++TotalProgress;
            IterateMPD(topLeftArea, roughness / 2);
            IterateMPD(topRightArea, roughness / 2);
            IterateMPD(bottomLeftArea, roughness / 2);
            IterateMPD(bottomRightArea, roughness / 2);
        }

        private void InitializeHeightMap()
        {
            this.heightMap = new float[this.realMapDimension, this.realMapDimension];
            for (int x = 0; x < this.realMapDimension; ++x)
            {
                for (int y = 0; y < this.realMapDimension; ++y)
                {
                    this.heightMap[x, y] = -1f;
                }
            }

            #region Initial seeding
            this.heightMap[0, 0] = (float)(this.maxHeight * rand.NextDouble());
            this.heightMap[0, this.mapDimension] = (float)(this.maxHeight * rand.NextDouble());
            this.heightMap[this.mapDimension, 0] = (float)(this.maxHeight * rand.NextDouble());
            this.heightMap[this.mapDimension, this.mapDimension] = (float)(this.maxHeight * rand.NextDouble());
            #endregion
        }

        private void TryAcquireOffsetAndPeak(float candidate)
        {
            if (candidate < this.HeightOffset)
            {
                this.HeightOffset = (int)candidate;
            }

            if (candidate > this.HighestPeak)
            {
                this.HighestPeak = (int)candidate;
            }
        }

        // A canvas-like effect occurs due to too extreme decrementation of the displacement
        //private void ReduceDisplacement()
        //{
        //    this.displacement *= Math.Pow(2, -1);
        //}

        private void PerformSquareStep(Rectangle area, float roughness)
        {
            this.heightMap[area.Center.X, area.Center.Y] = MathHelper.Clamp((this.heightMap[area.Left, area.Top] + this.heightMap[area.Right, area.Top] + this.heightMap[area.Left, area.Bottom] + this.heightMap[area.Right, area.Bottom]) / 4 + this.getRoughness(roughness), 0, this.maxHeight);
            TryAcquireOffsetAndPeak(this.heightMap[area.Center.X, area.Center.Y]);
        }

        private void PerformDiamondStep(Rectangle area, float roughness)
        {
            Point top = new Point(area.Left + area.Width / 2, area.Top),
                    bottom = new Point(area.Left + area.Width / 2, area.Bottom),
                    left = new Point(area.Left, area.Top + area.Height / 2),
                    right = new Point(area.Right, area.Top + area.Height / 2),
                center = area.Center;

            CreateHorizontalDiamondPart(top, area.Left, area.Right, area.Top, center, roughness);
            CreateVerticalDiamondPart(left, area.Top, area.Bottom, area.Left, center, roughness);
            CreateHorizontalDiamondPart(bottom, area.Left, area.Right, area.Bottom, center, roughness);
            CreateVerticalDiamondPart(right, area.Top, area.Bottom, area.Right, center, roughness);
        }

        private void CreateHorizontalDiamondPart(Point target, int left, int right, int y, Point center, float roughness)
        {
            if (this.heightMap[target.X, target.Y] == -1)
            {
                float value = (this.heightMap[left, y] + this.heightMap[right, y] + this.heightMap[center.X, center.Y]) / 3 + this.getRoughness(roughness);
                this.heightMap[target.X, target.Y] = MathHelper.Clamp(value, 0, this.maxHeight);
                TryAcquireOffsetAndPeak(this.heightMap[target.X, target.Y]);
            }
        }

        private void CreateVerticalDiamondPart(Point target, int top, int bottom, int x, Point center, float roughness)
        {
            if (this.heightMap[target.X, target.Y] == -1)
            {
                float value = (this.heightMap[x, top] + this.heightMap[x, bottom] + this.heightMap[center.X, center.Y]) / 3 + this.getRoughness(roughness);
                this.heightMap[target.X, target.Y] = MathHelper.Clamp(value, 0, this.maxHeight);
                TryAcquireOffsetAndPeak(this.heightMap[target.X, target.Y]);
            }
        }

        public void SmoothTerrain(int passes)
        {
            float[,] newHeightData;

            while (passes > 0)
            {
                passes--;

                // Note: MapWidth and MapHeight should be equal and power-of-two values 
                newHeightData = new float[this.realMapDimension, this.realMapDimension];

                for (int x = 0; x < this.realMapDimension; x++)
                {
                    for (int y = 0; y < this.realMapDimension; y++)
                    {
                        int adjacentSections = 0;
                        float sectionsTotal = 0.0f;

                        if ((x - 1) > 0) // Check to left
                        {
                            sectionsTotal += this.heightMap[x - 1, y];
                            adjacentSections++;

                            if ((y - 1) > 0) // Check up and to the left
                            {
                                sectionsTotal += this.heightMap[x - 1, y - 1];
                                adjacentSections++;
                            }

                            if ((y + 1) < this.realMapDimension) // Check down and to the left
                            {
                                sectionsTotal += this.heightMap[x - 1, y + 1];
                                adjacentSections++;
                            }
                        }

                        if ((x + 1) < this.realMapDimension) // Check to right
                        {
                            sectionsTotal += this.heightMap[x + 1, y];
                            adjacentSections++;

                            if ((y - 1) > 0) // Check up and to the right
                            {
                                sectionsTotal += this.heightMap[x + 1, y - 1];
                                adjacentSections++;
                            }

                            if ((y + 1) < this.realMapDimension) // Check down and to the right
                            {
                                sectionsTotal += this.heightMap[x + 1, y + 1];
                                adjacentSections++;
                            }
                        }

                        if ((y - 1) > 0) // Check above
                        {
                            sectionsTotal += this.heightMap[x, y - 1];
                            adjacentSections++;
                        }

                        if ((y + 1) < this.realMapDimension) // Check below
                        {
                            sectionsTotal += this.heightMap[x, y + 1];
                            adjacentSections++;
                        }

                        newHeightData[x, y] = (this.heightMap[x, y] + (sectionsTotal / adjacentSections)) * 0.5f;
                    }
                }

                // Overwrite the HeightData info with our new smoothed info
                for (int x = 0; x < this.realMapDimension; x++)
                {
                    for (int y = 0; y < this.realMapDimension; y++)
                    {
                        this.heightMap[x, y] = newHeightData[x, y];
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("A {0}x{0} heightmap.", this.realMapDimension));
            for (int x = 0; x < this.realMapDimension; ++x)
            {
                for (int y = 0; y < this.realMapDimension; ++y)
                {
                    sb.Append(string.Format("{0:F0}\t", this.heightMap[y, x]));
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static event EventHandler<ProgressEventArgs> Progressing;

        public static int TotalProgress { get; private set; }
    }
}
