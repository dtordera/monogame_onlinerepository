#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

#endregion

namespace spherewaves
{
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		private const int _WIDTH = 500;
		private const int _HEIGHT = 500;

		private const int _MAXCOLORS = 255; // Maximum palette colors
		private static System.Drawing.Color[] Palette = new System.Drawing.Color[_MAXCOLORS];

		Random rnd = new Random();

		private class Focus :IDisposable
		{
			public int x,y;
			public int t = 0;

			public void Dispose()
			{
			}
		}

		List<Focus> F;

		Texture2D texture = null;

		public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";	            
			graphics.PreferredBackBufferWidth = _WIDTH;
			graphics.PreferredBackBufferHeight = _HEIGHT;
			graphics.IsFullScreen = false;
			graphics.ApplyChanges ();
		}

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
			// search distance between pivot colors in palette 
			float m = Palette.Length / (C.Length);

			// let's tie palette, make it rounded
			Palette [0] = C [0];
			Palette [_MAXCOLORS-1] = C [0];

			// Lets point each choosen color at position and do gradient between them
			for (int i = 1; i < C.Length; i++) 
			{
				Palette [(int)(i * m)] = C [i];
				Gradient ((int)((i-1)* m),(int)(i * m));
			}

			// lets degrade last color index. This line is due float can be not exactly _MAXCOLORS-1
			Gradient ((int)((C.Length-1)*m), _MAXCOLORS - 1);
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

		protected void DoPalette()
		{
			System.Drawing.Color[] C = new System.Drawing.Color[5];

			C[0] = System.Drawing.Color.DarkSlateGray;
			C[1] = System.Drawing.Color.SlateGray;
			C[2] = System.Drawing.Color.LightSlateGray;
			C[3] = System.Drawing.Color.Silver;
			C[4] = System.Drawing.Color.White;

			FillGradients(C);
			SwapRB();
		}

		protected override void Initialize ()
		{
			base.Initialize ();
			spriteBatch = new SpriteBatch (GraphicsDevice);

			texture = new Texture2D (graphics.GraphicsDevice, _WIDTH, _HEIGHT);
				
			DoPalette();
		}

		protected void DumpToBitmap()
		{
			int x, y;
			System.Drawing.Color c;

			int[] Work = new int[_WIDTH*_HEIGHT];

			for (x=0;x<_WIDTH;x++)
			for (y=0;y<_HEIGHT;y++)	
				Work[x + y*_WIDTH] = 128;

			for (x = 0; x < _WIDTH; x++)
				for (y = 0; y < _HEIGHT; y++) 
				{
					c = Palette [Work [x + y * _WIDTH]];
					Work [x + y * _WIDTH] = c.ToArgb();
				}

			texture.SetData<Int32> (Work);
		}

		protected override void Update (GameTime gameTime)
		{
			if (Keyboard.GetState ().IsKeyDown (Keys.Escape)) 
				Exit ();

			if (Keyboard.GetState().IsKeyDown(Keys.Space))
			{
				graphics.PreferredBackBufferHeight = _WIDTH;
				graphics.PreferredBackBufferWidth = _HEIGHT;
				graphics.ApplyChanges();
			}

			if (Keyboard.GetState().IsKeyDown(Keys.M))
				F.Add(new Focus() { x = rnd.Next()%_WIDTH, y = rnd.Next()%_HEIGHT });
	
			base.Update (gameTime);

			DumpToBitmap();
		}

		protected override void Draw (GameTime gameTime)
		{		   
			base.Draw (gameTime);

			spriteBatch.Begin ();
			spriteBatch.Draw(texture, Vector2.Zero, Microsoft.Xna.Framework.Color.White);
			spriteBatch.End ();
		}
	}
}

