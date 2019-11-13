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

class Square
{
	public static RGBi emptyColor = new RGBi(187, 187, 187);
	public static RGBi deadColor = new RGBi(100, 100, 100);
	public static RGBi rowFilledColor = new RGBi(255, 255, 0);

	public const int numDisappearingColors = 3;
	public static RGBi[] disappearingColor;

	public RGBi color;
	public bool isDirty;
	public bool isActive;

	public Square()
	{
		color = null;
		isDirty = true;
		isActive = false;
	}

	public void CopyFrom(Square other)
	{
		isDirty = isDirty || other.isDirty;

		if (color != other.color)
			isDirty = true;

		color = other.color;
		isActive = false;
	}

	public void Clear()
	{
		if (color != null)
			isDirty = true;

		color = null;
		isActive = false;
	}

	public static void InitializeClass()
	{
		disappearingColor = new RGBi[numDisappearingColors];
		for (int i = 0; i < disappearingColor.Length; ++i)
		{
			disappearingColor[i] = RGBi.Lerp(emptyColor, rowFilledColor, (double)i / numDisappearingColors);
		}
	}
}