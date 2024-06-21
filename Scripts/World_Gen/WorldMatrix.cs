using Godot;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

public class PerlinNoise{

	private static Random random = new Random();

	// Fade function as defined by Ken Perlin
	private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);

	// Linear interpolation function
	private static double Lerp(double t, double a, double b) => a + t * (b - a);

	// Gradient function
	private static double Grad(int hash, double x, double y)
	{
		int h = hash & 7;      // Convert low 3 bits of hash code
		double u = h < 4 ? x : y; // into 8 simple gradient directions
		double v = h < 4 ? y : x;
		return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
	}

	


	public static void generatePerm(){
		perm = new int[256];
		for(int i = 0; i< 256; i++){
			perm[i] = random.Next(256);
		}
	}

	// Permutation array to hash the coordinates
	private static int[] perm;

	private static int Permute(int i) => perm[i & 255];
	
	// Generate Perlin Noise value for a 2D point
	public static double Noise(double x, double y)
	{
		int X = (int)Math.Floor(x) & 255; // Find unit square that contains the point
		int Y = (int)Math.Floor(y) & 255;

		x -= Math.Floor(x); // Relative x, y
		y -= Math.Floor(y);

		double u = Fade(x); // Compute fade curves
		double v = Fade(y);

		int A = Permute(X) + Y;
		int B = Permute(X + 1) + Y;


		return Lerp(v, Lerp(u, Grad(Permute(A), x, y), Grad(Permute(B), x - 1, y)),
					   Lerp(u, Grad(Permute(A + 1), x, y - 1), Grad(Permute(B + 1), x - 1, y - 1)));
	}

	private static double CalculateDistanceFromBorder(int x, int y, int width, int height)
	{
		int distX = Math.Min(x, width - x - 1);
		int distY = Math.Min(y, height - y - 1);
		return Math.Min(distX, distY);
	}

	// Falloff function
	private static double Falloff(double distanceFromBorder, double falloffStartDistance)
	{
		if (distanceFromBorder > falloffStartDistance)
		{
			return 1.0;
		}
		else
		{
			// Smoothly decrease from 1.0 to 0.0 using a smoothstep-like function
			double t = 1- ((falloffStartDistance - distanceFromBorder) / falloffStartDistance);
			return Smoothstep(0, 1, t);
		}
	}
	// Smoothstep function for falloff
	private static double Smoothstep(double edge0, double edge1, double x)
	{
		x = Math.Clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
		return x * x * (3 - 2 * x);
	
	}

	private static void NormalizeAndScale(double[,] noise, int width, int height, double falloffStartDistance)
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{

				double distanceFromBorder = CalculateDistanceFromBorder(x, y, width, height);
				
				// Apply falloff based on distance
				double falloff = Falloff(distanceFromBorder, falloffStartDistance);
				noise[x, y] = 255 * ((noise[x, y]+1)/2) * falloff; // Converts into value between 0 and 255
			}
		}
	}

	public static double[,] GeneratePerlinNoise(int width, int height, double falloffStartDistance, double scale, int octaves, double persistence, double lacunarity)
	{
		generatePerm(); // generate random noise 

		double[,] noise = new double[width, height];
		double amplitude = 1.0;
		double frequency = 1;

		for (int octave = 0; octave < octaves; octave++)
		{
			amplitude *= persistence; // Decrease amplitude with each octave
			
			

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					double sampleX = x / scale * frequency;
					double sampleY = y / scale * frequency;
					noise[x, y] += Noise(sampleX, sampleY) * amplitude;
				}
			}

			frequency *= lacunarity; // frequency = lacunarity ^ octave
		}

		NormalizeAndScale(noise, width, height, falloffStartDistance);

		return noise;
	}

}



public partial class WorldMatrix : Node2D
{

	[Export]
	public Godot.TileMap Map;

	[Export]
	public int height, width, octaves;

	[Export]
	public double falloffPercentage, scale2, persistence, lacunarity;




	// Fills a matrix with random inputs between -1 and 1
	// PRE: Matrix real height and width match the input arguments height and width
	static void FillRandom(float[,] matrix, int height, int width)
	{
		Random rand = new Random();
		GD.Print(rand);
		for (int i = 0; i < height; i++){
			for (int j = 0; j < width; j++){
				matrix[i,j] = (float) ((float)(rand.Next(0, 200)) / 100.0 - 1); // Fills matrix with random number between -1 and 1
			}
		}
	}

	// Prints values of the matrix. Values of each row separated by a tab ('\t') and each line separated by a line break ('\n')
	// PRE: Matrix real height and width match the input arguments height and width
	static void PrintMatrix(double[,] matrix)
	{

		String line = "";

		for (int i = 0; i < matrix.GetLength(0); i++)
		{
			for (int j = 0; j < matrix.GetLength(1); j++)
			{
				line += (matrix[i, j] + "\t"); // Adjust formatting as needed
			}
			GD.Print(line);
			line = "";
		}
	}

	public void FillMap(double[,] matrix){
		int h = matrix.GetLength(0);
		int w = matrix.GetLength(1);

		for(int i = 0; i < h; i++){
			for(int j = 0; j<w; j++){
				// Add Tile
				Map.SetCell(0,new Vector2I(i - height/2,j - width/2), 0, new Vector2I(getColor(matrix[i,j]),0));
			}
		}
	}

	public int getColor(double x){
		int res;
		if ( x < 110) res = 254;
		else if ( x < 115) res = 245;
		else if(x < 120) res = 235;
		else if( x < 135) res = 130;
		else if ( x < 150) res = 100;
		else if ( x < 190) res = 50;
		else res = 0;
		return res;

	}

	public override void _Ready()
	{

		if (Map == null || Map.TileSet == null)
		{
			GD.PrintErr("TileMap or TileSet is not set.");
			return;
		}

		GD.Print("Start");

		//float[,] matrix = new float[height,width];
		//FillRandom(matrix, height, width);
		//FillMap(matrix);

		double falloffStartDistance = falloffPercentage / 100 * Math.Min(width, height);

		double[,] matrix = PerlinNoise.GeneratePerlinNoise(width,height, falloffStartDistance,scale2,octaves,persistence,lacunarity);

											//
		//PrintMatrix(matrix);
		FillMap(matrix);

		GD.Print("End of Worldgen");
		
		
	}
}
