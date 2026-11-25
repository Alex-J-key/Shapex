using System.Drawing.Imaging.Effects;
using System.Windows.Forms;

namespace ShapeTemplate
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public PerlinNoiseGenerator png = new PerlinNoiseGenerator();

        public int _cameraX = 0;
        public int _cameraY = 0;

        public Shape Circle = new Shape(Corner.Type.Circle, Corner.Type.Circle, Corner.Type.Circle, Corner.Type.Circle);
        public Shape Square = new Shape(Corner.Type.Sqaure, Corner.Type.Sqaure, Corner.Type.Sqaure, Corner.Type.Sqaure);

        public List<Shape> Shapes = new List<Shape>();
        public bool rendering = false;
        public int ShapeSize = 50;

        private void button1_Click(object sender, EventArgs e)
        {
           
        }

        private Point WorldToScreen(int worldCellX, int worldCellY, int CellSize)
        {
            int screenX = (worldCellX - _cameraX) * CellSize;
            int screenY = (worldCellY - _cameraY) * CellSize;

            return new Point(screenX, screenY);
        }
        public void RenderShape(Shape s)
        {
            Graphics g = CreateGraphics();

            Point pos = WorldToScreen(s.x, s.y, ShapeSize);
            pos.X += ShapeSize * s.x;
            pos.Y += ShapeSize * s.y;

            //Point pos = new Point(x, y);

            Size CircleSize = new Size(ShapeSize * 2, ShapeSize * 2);
            Size SquareSize = new Size(ShapeSize, ShapeSize);

            Rectangle CircleRectangle = new Rectangle(pos, CircleSize);
            Rectangle SquareRectangle = new Rectangle(pos, SquareSize);

            foreach (Corner c in s.Corners)
            {
                if (c.TypeOfCorner == Corner.Type.Circle)
                {
                    int startAngle = 0;
                    int sweepAngle = 0;
                    switch (c.CornerLocation)
                    {
                        case Corner.Location.Top_Left: // TL
                            startAngle = 180;
                            sweepAngle = 90;
                            break;
                        case Corner.Location.Top_Right: // TR
                            startAngle = 270;
                            sweepAngle = 90;
                            break;
                        case Corner.Location.Bottom_Left: // BL
                            startAngle = 450;
                            sweepAngle = 90;
                            break;
                        case Corner.Location.Bottom_Right: // BR
                            startAngle = 360;
                            sweepAngle = 90;
                            break;
                    }
                    g.FillPie(new SolidBrush(c.CornerColor), CircleRectangle, startAngle, sweepAngle);
                    g.DrawPie(Pens.Black, CircleRectangle, startAngle, sweepAngle);
                }
                else if (c.TypeOfCorner == Corner.Type.Sqaure)
                {
                    switch (c.CornerLocation)
                    {
                        case Corner.Location.Top_Left: // TL 
                            // Default SquareRectangle
                            break;
                        case Corner.Location.Top_Right:// TR
                            SquareRectangle.X += ShapeSize;
                            break;
                        case Corner.Location.Bottom_Left: // BL
                            SquareRectangle.Y += ShapeSize;
                            break;
                        case Corner.Location.Bottom_Right: // BR
                            SquareRectangle.X += ShapeSize;
                            SquareRectangle.Y += ShapeSize;
                            break;
                    }
                    g.FillRectangle(new SolidBrush(c.CornerColor), SquareRectangle);
                    g.DrawRectangle(Pens.Black, SquareRectangle);
                }
            }
        }
        public void RenderShapes()
        {
            rendering = true;
            Graphics g = CreateGraphics();
            g.Clear(this.BackColor);

            foreach (Shape s in Shapes)
            {
                RenderShape(s);
            }
            rendering = false;
        }
        public void GenerateShapes()// Generates the Terrain, includes the colour of types of terrain too
        {
            Random rng = new Random();

            png.GenerateRandomSeed(50);
            //png.WorldSeedUnHashed = "fhsdjpfisuhi"; // Sets perlin noise generator seed to the world seed textboxt text
            png.PerlinScale = 25; // The higher the more smoothed out, the lower the more compact and dense
            png.GeneratePerlinNoise();

            for (int x = -100; x < 100; x++)
            {
                for (int y = -100; y < 100; y++)
                {
                    Shape shape = new Shape();
                    double NoiseValue = png.GetPerlinNoise(x, y);
                    if (NoiseValue <= -0.5)
                    {
                        shape = new Shape(Corner.Type.Circle, Corner.Type.Circle, Corner.Type.Circle, Corner.Type.Circle);
                        shape.x = x;
                        shape.y = y;
                        Shapes.Add(shape);
                        RenderShape(shape);
                    }
                    else if (NoiseValue >= -0.4 && NoiseValue <= -0.3)
                    {
                        shape = new Shape(Corner.Type.Circle, Corner.Type.Circle, Corner.Type.Circle, Corner.Type.Circle);
                        shape.x = x;
                        shape.y = y;
                        Shapes.Add(shape);
                    }
                    else if (NoiseValue >= -0.2 && NoiseValue <= -0.1)
                    {
                        shape = new Shape(Corner.Type.Sqaure, Corner.Type.Sqaure, Corner.Type.Sqaure, Corner.Type.Sqaure);
                        shape.x = x;
                        shape.y = y;
                        Shapes.Add(shape);
                    }
                    else if (NoiseValue >= 0 && NoiseValue <= 0.5)
                    {
                        shape = new Shape(Corner.Type.Sqaure, Corner.Type.Sqaure, Corner.Type.Sqaure, Corner.Type.Sqaure);
                        shape.x = x;
                        shape.y = y;
                        Shapes.Add(shape);
                    }
                    else if (NoiseValue >= 0.6 && NoiseValue <= 1.0)
                    {
                        shape = new Shape(Corner.Type.Sqaure, Corner.Type.Sqaure, Corner.Type.Sqaure, Corner.Type.Sqaure);
                        shape.x = x;
                        shape.y = y;
                        Shapes.Add(shape);
                    }
                }
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (rendering)
                return;

            switch (e.KeyCode)
            {
                case Keys.W:
                    _cameraY--;
                    break;
                case Keys.S:
                    _cameraY++;
                    break;
                case Keys.A:
                    _cameraX++;
                    break;
                case Keys.D:
                    _cameraX++;
                    break;
                case Keys.OemMinus:
                    ShapeSize--;
                    break;
                case Keys.Space:
                    Shapes.Clear();
                    GenerateShapes();
                    //MessageBox.Show("Done Generating..");
                    
                    //MessageBox.Show("Done Rendering..");
                    break;
            }
            RenderShapes();
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
        public Shape()
        {

        }
        public Shape(Corner.Type top_left, Corner.Type top_right, Corner.Type bottom_left, Corner.Type bottom_right)
        {

            Corner TopLeft = new Corner(top_left, Corner.Location.Top_Left);
            Corner TopRight = new Corner(top_right, Corner.Location.Top_Right); 
            Corner BottomLeft = new Corner(bottom_left, Corner.Location.Bottom_Left); 
            Corner BottomRight = new Corner(bottom_right, Corner.Location.Bottom_Right); 

            Corners.Add(TopLeft);
            Corners.Add(TopRight);
            Corners.Add(BottomLeft);
            Corners.Add(BottomRight);

            Random rng = new Random();

            foreach (Corner c in Corners)
            {
                switch (rng.Next(1, 6))
                { 
                    case 1:
                        c.CornerColor = Color.Blue;
                        break;
                    case 2:
                        c.CornerColor = Color.Green;
                        break;
                    case 3:
                        c.CornerColor = Color.Yellow;
                        break;
                    case 4:
                        c.CornerColor = Color.Red;
                        break;
                }
            }
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
        public double PerlinScale = 20; // The higher the more smoothed out, the lower the more compact and dense
        public int[] PermutationTable = new int[512];
        public PerlinNoiseGenerator()
        {

        }
        public void GenerateRandomSeed(int length)
        {
            Random rng = new Random();
            string RandomisedSeed = "";
            for (int i = 1; i <= length; i++)
            {
                RandomisedSeed += (char)rng.Next(32, 126); // Random ASCII characters
            }
            WorldSeedUnHashed = RandomisedSeed;
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
            {
                hash = (hash * 31) + c;
            }
            return hash;
        }
        public void MakePermutationTable()
        {
            List<int> tempTable = new List<int>();
            for (int i = 0; i < 256; i++)
            {
                tempTable.Add(i);
            }

            int seed = (int)WorldSeedHash;
            Random rng = new Random(seed);

            int n = tempTable.Count;
            while (n > 1)
            {
                n--;
                // Generates a deterministic index
                int k = rng.Next(n + 1);

                // Swap the elements
                var value = tempTable[k];
                tempTable[k] = tempTable[n];
                tempTable[n] = value;
            }

            for (int i = 0; i < 256; i++) // Duplicate the first 256 until index 511
            {
                PermutationTable[i] = tempTable[i];
                PermutationTable[i + 256] = tempTable[i];
            }
        }

        public double GetPerlinNoise(int x, int y)
        {
            // Converting Raw world coordinates to decimal coordinates by divding by scale
            double dx = x / PerlinScale;
            double dy = y / PerlinScale;

            // Remove integers to leave only decimals
            double xf = dx - Math.Floor(dx);
            double yf = dy - Math.Floor(dy);

            int X = (int)Math.Floor(dx);
            int Y = (int)Math.Floor(dy);

            // PreCalculate
            int X0 = X & 255;
            int X1 = (X + 1) & 255;
            int Y0 = Y & 255;
            int Y1 = (Y + 1) & 255;

            // Calculate the top, bottom, left, right corner values
            int hashBL = PermutationTable[PermutationTable[X0] + Y0];
            int hashBR = PermutationTable[PermutationTable[X1] + Y0];
            int hashTL = PermutationTable[PermutationTable[X0] + Y1];
            int hashTR = PermutationTable[PermutationTable[X1] + Y1];

            // Distance vector for each corner
            double gradientBL = Gradient(hashBL, xf, yf);
            double gradientBR = Gradient(hashBR, xf - 1, yf);
            double gradientTL = Gradient(hashTL, xf, yf - 1);
            double gradientTR = Gradient(hashTR, xf - 1, yf - 1);

            double fadeX = Fade(xf);
            double fadeY = Fade(yf);

            double TopAverage = Lerp(gradientTL, gradientTR, fadeX);
            double BottomAverage = Lerp(gradientBL, gradientBR, fadeX);
            double AverageLerp = Lerp(BottomAverage, TopAverage, fadeY);

            return AverageLerp;
        }
        public double Lerp(double a, double b, double t)
        {
            return a + t * (b - a);
        }
        public double Fade(double t)
        {
            // Equal to: 6t^5 - 15t^4 + 10t^3
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
        public double Gradient(int hash, double dx, double dy)
        {
            int pick = hash & 3; // & means bitwise AND

            switch (pick)
            {
                case 0: return (1 * dx) + (1 * dy);
                case 1: return (-1 * dx) + (1 * dy);
                case 2: return (1 * dx) + (-1 * dy);
                case 3: return (-1 * dx) + (-1 * dy);
                default: return 0;
            }
        }
    }
}