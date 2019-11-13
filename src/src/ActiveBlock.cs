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

public class ActiveBlock : Block
{
	public bool cannotBeSpawned { get; private set; }

	private int x;
	private int y;
	private int rotation;
	private bool isNew;
	private Board board;

	public ActiveBlock(Board board)
	{
		this.board = board;
		int blockType = RandomNumber.Generate(0, Block.numTypes);
		SetType(blockType);
		rotation = RandomNumber.Generate(0, Block.numRotations);
		x = (Board.numSquaresX - Width()) / 2;
		y = 0;
		isNew = true;
		cannotBeSpawned = !SetLocation(x, y, rotation);
	}

	public bool Move(int x, int y)
	{
		return SetLocation(this.x + x, this.y + y, this.rotation);
	}

	public bool Rotate()
	{
		return SetLocation(this.x, this.y, this.rotation + 1);
	}

	public void Freeze()
	{
		board.DeactivateBlock(this, x, y, rotation);
	}

	public void ForcePlace()
	{
		board.PlaceBlock(this, x, y, rotation, false);
	}

	private bool SetLocation(int x, int y, int rotation)
	{
		if (board.IsBlockValid(this, x, y, rotation))
		{
			if (!isNew)
				board.ClearBlock(this, this.x, this.y, this.rotation);
			board.PlaceBlock(this, x, y, rotation, true);
			this.x = x;
			this.y = y;
			this.rotation = rotation;
			isNew = false;
			return true;
		}
		return false;
	}
}