/*
 * Copyright (c) 2019 Evan Kale
 * Email: EvanKale91@gmail.com
 * Web: www.youtube.com/EvanKale
 * Social: @EvanKale91
 *
 * This file is part of debtris.
 *
 * debtris is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;

public class Block
{
	public RGBi color { get; private set; }

	private bool[][] squares;

	public Block() { }

	public void SetType(int type)
	{
		squares = (bool[][])allBlockTypes[type].squares.Clone();
		color = (RGBi)allBlockTypes[type].color;
	}

	public bool GetSquare(int x, int y, int rotation)
	{
		switch (rotation % 4)
		{
			case 0:
				return squares[y][x];
			case 1:
				return squares[x][squares.Length - 1 - y];
			case 2:
				return squares[squares.Length - 1 - y][squares.Length - 1 - x];
			case 3:
				return squares[squares.Length - 1 - x][y];
		}
		return squares[y][x];
	}

	public int Width()
	{
		return squares.Length;
	}

	private Block(bool[][] squares, RGBi color)
	{
		this.squares = squares;
		this.color = color;
	}

	// ========
	//  Static
	// ========

	public static int numTypes { get; private set; }
	public const int numRotations = 4;

	private static bool classInitialized = false;
	private static string blockDefFile = "block.def";
	private static List<Block> allBlockTypes;

	public static void InitializeClass()
	{
		if (classInitialized)
			return;

		allBlockTypes = new List<Block>();

		using (StreamReader reader = new StreamReader(ResourceManager.GetStream(blockDefFile)))
		{
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				string[] color = line.Split(',');
				int red = Int32.Parse(color[0]);
				int green = Int32.Parse(color[1]);
				int blue = Int32.Parse(color[2]);

				List<List<bool>> squaresList = new List<List<bool>>();

				while (!reader.EndOfStream && !(line = reader.ReadLine()).Equals(""))
				{
					List<bool> curSquareList = new List<bool>();
					squaresList.Add(curSquareList);
					for (int i = 0; i < line.Length; ++i)
					{
						if (line[i] == 'o')
							curSquareList.Add(true);
						else
							curSquareList.Add(false);
					}
				}

				bool[][] squaresArray = new bool[squaresList.Count][];
				for (int i = 0; i < squaresArray.Length; ++i)
				{
					squaresArray[i] = squaresList[i].ToArray();
				}

				allBlockTypes.Add(new Block(squaresArray, new RGBi(red, green, blue)));
			}
		}
		numTypes = allBlockTypes.Count;
		classInitialized = true;
	}

}