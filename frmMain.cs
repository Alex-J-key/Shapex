using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Shapex
{
    public partial class frmMain : Form
    {
        public static readonly Random Rng = new Random();

        public PerlinNoiseGenerator png = new PerlinNoiseGenerator();

        public int _cameraX = 0;
        public int _cameraY = 0;
        public int ShapeSize = 25; // size of one corner; a full shape is ShapeSize * 2

        public List<Shape> Shapes = new List<Shape>();

        public frmMain()
        {
            InitializeComponent();
            DoubleBuffered = true;
            KeyPreview = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GenerateShapes();

            // Start the camera in the middle of the generated world
            int cell = ShapeSize * 2;
            _cameraX = -(ClientSize.Width / cell) / 2;
            _cameraY = -(ClientSize.Height / cell) / 2;
            Invalidate();
        }

        private Point WorldToScreen(int worldX, int worldY, int cellSize)
            => new Point((worldX - _cameraX) * cellSize, (worldY - _cameraY) * cellSize);

        private void RenderShape(Graphics g, Shape s)
        {
            int cell = ShapeSize * 2; // a shape is 2x2 corners
            Point pos = WorldToScreen(s.x, s.y, cell);

            // Skip anything off-screen
            if (pos.X > ClientSize.Width || pos.Y > ClientSize.Height ||
                pos.X + cell < 0 || pos.Y + cell < 0)
                return;

            var circleRect = new Rectangle(pos, new Size(cell, cell));

            foreach (Corner c in s.Corners)
            {
                using var brush = new SolidBrush(c.CornerColor);

                if (c.TypeOfCorner == Corner.Type.Circle)
                {
                    int start = c.CornerLocation switch
                    {
                        Corner.Location.Top_Left => 180,
                        Corner.Location.Top_Right => 270,
                        Corner.Location.Bottom_Right => 0,
                        _ => 90 // Bottom_Left
                    };
                    g.FillPie(brush, circleRect, start, 90);
                    g.DrawPie(Pens.Black, circleRect, start, 90);
                }
                else if (c.TypeOfCorner == Corner.Type.Sqaure)
                {
                    int ox = c.CornerLocation is Corner.Location.Top_Right or Corner.Location.Bottom_Right ? ShapeSize : 0;
                    int oy = c.CornerLocation is Corner.Location.Bottom_Left or Corner.Location.Bottom_Right ? ShapeSize : 0;
                    var r = new Rectangle(pos.X + ox, pos.Y + oy, ShapeSize, ShapeSize);
                    g.FillRectangle(brush, r);
                    g.DrawRectangle(Pens.Black, r);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            foreach (Shape s in Shapes)
                RenderShape(e.Graphics, s);
        }

        public void GenerateShapes() // Generates the terrain
        {
            png.GenerateRandomSeed(50);
            png.PerlinScale = 25; // higher = smoother, lower = denser
            png.GeneratePerlinNoise();

            for (int x = -100; x < 100; x++)
            {
                for (int y = -100; y < 100; y++)
                {
                    double n = png.GetPerlinNoise(x, y);
                    var type = n <= -0.2 ? Corner.Type.Circle : Corner.Type.Sqaure; // tweak bands to taste
                    Shapes.Add(new Shape(type, type, type, type) { x = x, y = y });
                }
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: _cameraY--; break;
                case Keys.S: _cameraY++; break;
                case Keys.A: _cameraX--; break;
                case Keys.D: _cameraX++; break;
                case Keys.OemMinus: if (ShapeSize > 1) ShapeSize--; break;
                case Keys.Oemplus: ShapeSize++; break;
                case Keys.Space:
                    Shapes.Clear();
                    GenerateShapes();
                    break;
            }
            Invalidate();
        }
    }

    public class Cell
    {
        public Structure BuiltStructure;
    }

    public class Structure
    {
        public Shape ContainingShape;
    }

    public class Shape
    {
        public List<Corner> Corners = new List<Corner>();
        public int x;
        public int y;

        public Shape() { }

        public Shape(Corner.Type top_left, Corner.Type top_right, Corner.Type bottom_left, Corner.Type bottom_right)
        {
            Corners.Add(new Corner(top_left, Corner.Location.Top_Left));
            Corners.Add(new Corner(top_right, Corner.Location.Top_Right));
            Corners.Add(new Corner(bottom_left, Corner.Location.Bottom_Left));
            Corners.Add(new Corner(bottom_right, Corner.Location.Bottom_Right));

            Color[] palette = { Color.Blue, Color.Green, Color.Yellow, Color.Red };
            foreach (Corner c in Corners)
                c.CornerColor = palette[frmMain.Rng.Next(palette.Length)];
        }
    }

    public class Corner
    {
        public Color CornerColor;
        public Type TypeOfCorner;
        public Location CornerLocation;

        public Corner(Type typeOfCorner, Location locationofcorner)
        {
            TypeOfCorner = typeOfCorner;
            CornerLocation = locationofcorner;
        }

        public Corner(Type typeOfCorner)
        {
            TypeOfCorner = typeOfCorner;
        }

        public enum Type
        {
            Empty,
            Circle,
            Sqaure
        }

        public enum Location
        {
            Top_Left,
            Top_Right,
            Bottom_Left,
            Bottom_Right
        }
    }

    public class PerlinNoiseGenerator
    {
        public string WorldSeedUnHashed;
        public int WorldSeedHash;
        public double PerlinScale = 20; // higher = smoother, lower = denser
        public int[] PermutationTable = new int[512];

        public PerlinNoiseGenerator() { }

        public void GenerateRandomSeed(int length)
        {
            Random rng = new Random();
            string seed = "";
            for (int i = 1; i <= length; i++)
                seed += (char)rng.Next(32, 126); // random ASCII characters
            WorldSeedUnHashed = seed;
        }

        public void GeneratePerlinNoise()
        {
            WorldSeedHash = GetDerministicHashCode(WorldSeedUnHashed);
            MakePermutationTable();
        }

        public int GetDerministicHashCode(string tobehashed)
        {
            int hash = 23;
            foreach (char c in tobehashed)
                hash = unchecked((hash * 31) + c);
            return hash;
        }

        public void MakePermutationTable()
        {
            List<int> tempTable = new List<int>();
            for (int i = 0; i < 256; i++)
                tempTable.Add(i);

            Random rng = new Random(WorldSeedHash);

            int n = tempTable.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (tempTable[k], tempTable[n]) = (tempTable[n], tempTable[k]);
            }

            for (int i = 0; i < 256; i++) // duplicate the first 256 up to index 511
            {
                PermutationTable[i] = tempTable[i];
                PermutationTable[i + 256] = tempTable[i];
            }
        }

        public double GetPerlinNoise(int x, int y)
        {
            double dx = x / PerlinScale;
            double dy = y / PerlinScale;

            double xf = dx - Math.Floor(dx);
            double yf = dy - Math.Floor(dy);

            int X = (int)Math.Floor(dx);
            int Y = (int)Math.Floor(dy);

            int X0 = X & 255;
            int X1 = (X + 1) & 255;
            int Y0 = Y & 255;
            int Y1 = (Y + 1) & 255;

            int hashBL = PermutationTable[PermutationTable[X0] + Y0];
            int hashBR = PermutationTable[PermutationTable[X1] + Y0];
            int hashTL = PermutationTable[PermutationTable[X0] + Y1];
            int hashTR = PermutationTable[PermutationTable[X1] + Y1];

            double gradientBL = Gradient(hashBL, xf, yf);
            double gradientBR = Gradient(hashBR, xf - 1, yf);
            double gradientTL = Gradient(hashTL, xf, yf - 1);
            double gradientTR = Gradient(hashTR, xf - 1, yf - 1);

            double fadeX = Fade(xf);
            double fadeY = Fade(yf);

            double top = Lerp(gradientTL, gradientTR, fadeX);
            double bottom = Lerp(gradientBL, gradientBR, fadeX);
            return Lerp(bottom, top, fadeY);
        }

        public double Lerp(double a, double b, double t) => a + t * (b - a);

        public double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10); // 6t^5 - 15t^4 + 10t^3

        public double Gradient(int hash, double dx, double dy)
        {
            switch (hash & 3)
            {
                case 0: return dx + dy;
                case 1: return -dx + dy;
                case 2: return dx - dy;
                case 3: return -dx - dy;
                default: return 0;
            }
        }
    }
}