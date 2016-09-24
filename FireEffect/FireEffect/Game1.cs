using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace FireEffect
{
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		private const int _WIDTH = 300;   // 'heat' array size 
		private const int _HEIGHT = 350; // 

		private const int _FIREBASELENGTH = 5; // base line length 
		private const int _MAXCOLORS = 255; // Maximum palette colors

		private byte[] Back = new byte[_WIDTH*_HEIGHT];
		private byte[] Fore = new byte[_WIDTH*_HEIGHT];
		private Int32 [] w = new Int32[_WIDTH*_HEIGHT];

		private static Random r = new Random (); // Working random object
		private static System.Drawing.Color[] Palette = new System.Drawing.Color[_MAXCOLORS];

		Texture2D texture = null;

		protected void Gradient (int ao, int af)
		{
			int d = af - ao; // total distance between colors

			float 
			ro = Palette [ao].R, // Initial r,g,b values
			go = Palette [ao].G,
			bo = Palette [ao].B,
			dr = (Palette[af].R - ro)/d, // diferential of r,g,b
			dg = (Palette[af].G - go)/d, 
			db = (Palette[af].B - bo)/d;

			// lets fill each color in palette between range
			for (int i=0;i<d+1;i++)
				Palette [i + ao] = 
					System.Drawing.Color.FromArgb (
						(byte)(ro + i*dr), 
						(byte)(go + i*dg), 
						(byte)(bo + i*db)
					);
		}

		protected void FillGradients(System.Drawing.Color[] C)
		{
			// counting the back color, 0 indexed, there will be C.Length + 1 color turns. 
			int m = _MAXCOLORS / (C.Length+1);

			// first color (last one, 0, 'coldest' color) will be back page color
			Palette[0] = System.Drawing.Color.Black;

			// Lets point each choosen color at position and do gradient between them
			for (int i = 1; i < C.Length+1; i++) 
			{
				Palette [i * m] = C [i-1];
				Gradient ((i - 1) * m, i * m);
			}
		}

		public void btRandomPalette_Click (object sender, EventArgs args)
		{
			System.Drawing.Color[] C = new System.Drawing.Color[6];

			for (int i = 0; i < C.Length; i++) 
				C[i] = System.Drawing.Color.FromArgb (r.Next () % 0xff, r.Next () % 0xff, r.Next () % 0xff);

			FillGradients (C);
		}

		private void SwapRB()
		{
			System.Drawing.Color c;

			for (int i = 0; i < Palette.Length; i++) 
			{
				c = Palette [i];
				Palette [i] = System.Drawing.Color.FromArgb (c.B, c.G, c.R);
			}
		}

		public void btDefaultPalette_Click (object sender, EventArgs args)
		{
			System.Drawing.Color[] C = new System.Drawing.Color[6];

			C [0] = System.Drawing.Color.Navy;
			C [1] = System.Drawing.Color.OrangeRed;
			C [2] = System.Drawing.Color.Orange;
			C [3] = System.Drawing.Color.Yellow;
			C [4] = System.Drawing.Color.White;
			C [5] = System.Drawing.Color.White;

			FillGradients (C);
			SwapRB ();
		}

		protected void Burn()
		{
			Random r = new Random ();

			int x, y;
			int c;

			for (y = 1; y < _HEIGHT - 1; y++) 
				for (x = 1; x < _WIDTH - 1; x++) 
				{
					// Get all surrounding color indexs and do mean...
					c = (Back [x - 1 + (y - 1)*_WIDTH] + Back [x + (y - 1)*_WIDTH] + Back [x + 1 + (y - 1)*_WIDTH] +
						Back [x - 1 + y*_WIDTH]  +  /*   (x,y)    */     Back [x + 1 + y*_WIDTH]     + 
						Back [x - 1 + (y + 1)*_WIDTH] + Back [x + (y + 1)*_WIDTH] + Back [x + 1 + (y + 1)*_WIDTH])
						>> 3;

					c += (r.Next() % 2) - 1;

					if (c > 255)
						c = 255;
					if (c < 0)
						c = 0;

					// ...and we put it in the upper pixel. And then, fire grows...
					Fore [x + (y - 1)*_WIDTH] = (byte)c;
				}
		}

		protected void FireBase()
		{
			byte c = (byte)(_MAXCOLORS-1);
			int x, y;

			// Lets do, at image base, some random color lines
			for (y = _HEIGHT - 3; y < _HEIGHT; y++)
				for (x = 0; x < _WIDTH; x ++) 
				{
					if (x % _FIREBASELENGTH == 0)
						c = (byte)(Back [x + y*_WIDTH] + r.Next () % _MAXCOLORS);

					Back [x + y*_WIDTH] = (byte)(c % _MAXCOLORS - 1);
				}
		}

		protected void DumpToBitmap()
		{
			int x, y;
			System.Drawing.Color c;

			for (x = 0; x < _WIDTH; x++)
				for (y = 0; y < _HEIGHT; y++) 
				{
					c = Palette [Fore [x + y * _WIDTH]];
					w [x + y * _WIDTH] = c.ToArgb();
				}

			texture.SetData<Int32> (w);

			Array.Copy(Fore,Back,_WIDTH * _HEIGHT);
		}

		public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);

			graphics.PreferredBackBufferWidth = _WIDTH;
			graphics.PreferredBackBufferHeight = _HEIGHT;
			graphics.IsFullScreen = false;
			graphics.ApplyChanges ();

			Content.RootDirectory = "Content";	            		
		}
			
		protected override void Initialize ()
		{
			base.Initialize ();
			texture = new Texture2D (graphics.GraphicsDevice, _WIDTH, _HEIGHT);

			btDefaultPalette_Click (null, null);
		}
			
		protected override void LoadContent ()
		{
			spriteBatch = new SpriteBatch (GraphicsDevice);
		}
			
		protected override void Update (GameTime gameTime)
		{
			if (Keyboard.GetState ().IsKeyDown (Keys.Escape)) {
				Exit ();
			}
				
			if (Keyboard.GetState().IsKeyDown(Keys.Space))
			{
				graphics.PreferredBackBufferHeight = _HEIGHT;
				graphics.PreferredBackBufferWidth = _WIDTH;
				graphics.ApplyChanges();
			}

			if (Keyboard.GetState ().IsKeyDown (Keys.R))
				btRandomPalette_Click (null, null);

			if (Keyboard.GetState ().IsKeyDown (Keys.D))
				btDefaultPalette_Click (null, null);

			base.Update (gameTime);

			Burn ();
			DumpToBitmap ();
			FireBase ();
		}
			
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			base.Draw (gameTime);

			spriteBatch.Begin ();
			spriteBatch.Draw(texture, Vector2.Zero, Microsoft.Xna.Framework.Color.White);
			spriteBatch.End ();
		}
	}
}

