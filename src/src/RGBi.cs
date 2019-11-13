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

public class RGBi
{
	public int r, g, b;

	public RGBi(int r, int g, int b)
	{
		this.r = r;
		this.g = g;
		this.b = b;
	}

	public bool Equals(RGBi other)
	{
		return (r == other.r)
			&& (g == other.g)
			&& (b == other.b);
	}

	public string ToHexString()
	{
		return String.Format("{0:x2}{1:x2}{2:x2}", r, g, b);
	}

	public static RGBi Lerp(RGBi color1, RGBi color2, double blend)
	{
		RGBi retColor = new RGBi(
			(int)(color1.r * (1.0 - blend) + color2.r * blend),
			(int)(color1.g * (1.0 - blend) + color2.g * blend),
			(int)(color1.b * (1.0 - blend) + color2.b * blend)
			);
		return retColor;
	}
}