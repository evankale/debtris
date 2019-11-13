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

using System.IO;
using System.Reflection;

public class ResourceManager
{
	public static Stream GetStream(string embeddedFileName)
	{
		string[] resFileNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
		foreach (string resFileName in resFileNames)
		{
			if (resFileName.EndsWith(embeddedFileName))
			{
				return Assembly.GetExecutingAssembly().GetManifestResourceStream(resFileName);
			}
		}
		return null;
	}

	public static string GetResourceFolderPath()
	{
		string assemblyLocation = Assembly.GetExecutingAssembly().Location;
		string resFolderPath = Path.GetDirectoryName(assemblyLocation) + "\\res\\";
		return resFolderPath;
	}
}
