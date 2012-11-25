﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;

namespace Tanks3DFPP.Terrain
{
    /// <summary>
    ///     Fractal height map, using the midpoint displacement algorithm, see http://www.lighthouse3d.com/opengl/terrain/index.php?mpd2.
    /// </summary>
    public class FractalMap : IHeightMap
    {
        private Random rand;
        private int roughness;
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

        private readonly string numericFormat;

        private float randomizedDisplacement
        {
            get
            {
                return (float)(this.displacement * (rand.NextDouble() - 0.5));
            }
        }

        public FractalMap(int size, int roughness, float maxHeight, bool scribeMode)
        {

            if (this.scribeMode = scribeMode)
            {
                using (StreamWriter fw = new StreamWriter("heightMap_test.txt"))
                {
                    fw.Write("");
                }
            }

            rand = new Random();
            this.mapDimension = (1 << size);
            this.maxHeight = maxHeight;
            this.steps = size;
            numericFormat = string.Format("D{0}", maxHeight.ToString().Length);
            this.displacement = roughness;
            this.GenerateHeightData();
        }

        private void GenerateHeightData()
        {
            this.InitializeHeightMap();

            // iterate on the whole map
            this.IterateMPD(new Rectangle(0, 0, this.mapDimension, this.mapDimension));
            if (this.scribeMode)
            {
                using (StreamWriter fw = new StreamWriter("heightMap_test.txt", true))
                {
                    fw.WriteLine(this.ToString());
                }
            }
        }

        private void IterateMPD(Rectangle area)
        {
            if (area.Width < 2 || area.Height < 2)
            {
                return;
            }

            PerformSquareStep(area);
            PerformDiamondStep(area);
            ReduceDisplacement();

            Rectangle topLeftArea = new Rectangle(area.Left, area.Top, area.Width / 2, area.Height / 2), // Vit: /4 changed to /2
                        topRightArea = new Rectangle(area.Center.X, area.Top, area.Width / 2, area.Height / 2),
                        bottomLeftArea = new Rectangle(area.Left, area.Center.Y, area.Width / 2, area.Height / 2),
                        bottomRightArea = new Rectangle(area.Center.X, area.Center.Y, area.Width / 2, area.Height / 2);
            IterateMPD(topLeftArea);
            IterateMPD(topRightArea);
            IterateMPD(bottomLeftArea);
            IterateMPD(bottomRightArea);
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

        private void ReduceDisplacement()
        {
            this.displacement /= 2;
        }

        private void PerformSquareStep(Rectangle area)
        {
            this.heightMap[area.Center.X, area.Center.Y] = MathHelper.Clamp((this.heightMap[area.Left, area.Top] + this.heightMap[area.Right, area.Top] + this.heightMap[area.Left, area.Bottom] + this.heightMap[area.Right, area.Bottom]) / 4 + this.randomizedDisplacement, 0, this.maxHeight);
        }

        private void PerformDiamondStep(Rectangle area)
        {
            Point top = new Point(area.Left + area.Width / 2, area.Top),
                    bottom = new Point(area.Left + area.Width / 2, area.Bottom),
                    left = new Point(area.Left, area.Top + area.Height / 2),
                    right = new Point(area.Right, area.Top + area.Height / 2),
                center = area.Center;

            CreateHorizontalDiamondPart(top, area.Left, area.Right, area.Top, center);
            CreateHorizontalDiamondPart(bottom, area.Left, area.Right, area.Bottom, center);
            CreateVerticalDiamondPart(left, area.Top, area.Bottom, area.Left, center);
            CreateVerticalDiamondPart(right, area.Top, area.Bottom, area.Right, center);
        }

        private void CreateHorizontalDiamondPart(Point target, int left, int right, int y, Point center)
        {
            if (this.heightMap[target.X, target.Y] == -1)
            {
                this.heightMap[target.X, target.Y] = MathHelper.Clamp((this.heightMap[left, y] + this.heightMap[right, y] + this.heightMap[center.X, center.Y]) / 3 + this.randomizedDisplacement, 0, this.maxHeight);
            }
        }

        private void CreateVerticalDiamondPart(Point target, int top, int bottom, int x, Point center)
        {
            if (this.heightMap[target.X, target.Y] == -1)
            {
                this.heightMap[target.X, target.Y] = MathHelper.Clamp((this.heightMap[x, top] + this.heightMap[x, bottom] + this.heightMap[center.X, center.Y]) / 3 + this.randomizedDisplacement, 0, this.maxHeight);
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
    }
}
