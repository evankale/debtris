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
using System.Media;
using System.Threading;

public class SoundManager
{
	private static SoundPlayer[] goodSound;
	private static SoundPlayer[] badSound;
	private static SoundPlayer[] clickSound;
	private static SoundPlayer[] wooshSound;
	private static SoundPlayer[] boofSound;

	private const int numGoodSounds = 4;
	private const int numBadSounds = 4;
	private const int numClickSounds = 4;
	private const int numWooshSounds = 4;
	private const int numBoofSounds = 4;

	private const int goodSoundConsecutiveDelay = 1000 * 10000;
	private const int clickSoundConsecutiveDelay = 200 * 10000;
	private const int wooshSoundConsecutiveDelay = 200 * 10000;

	private static long goodSoundLastPlayed;
	private static long clickSoundLastPlayed;
	private static long wooshSoundLastPlayed;

	public static void PlayGoodSound()
	{
		long time = DateTime.Now.Ticks;
		if (time - goodSoundLastPlayed > goodSoundConsecutiveDelay)
		{
			ThreadPool.QueueUserWorkItem(ignoredState =>
			{
				goodSound[RandomNumber.Generate(0, numGoodSounds)].PlaySync();
			});
			goodSoundLastPlayed = time;
		}
	}

	public static void PlayBadSound()
	{
		ThreadPool.QueueUserWorkItem(ignoredState =>
		{
			badSound[RandomNumber.Generate(0, 4)].PlaySync();
		});
	}

	public static void PlayClickSound()
	{
		long time = DateTime.Now.Ticks;
		if (time - clickSoundLastPlayed > clickSoundConsecutiveDelay)
		{
			clickSound[RandomNumber.Generate(0, numClickSounds)].Stop();
			clickSound[RandomNumber.Generate(0, numClickSounds)].Play();
			clickSoundLastPlayed = time;
		}
	}

	public static void PlayWooshSound()
	{
		long time = DateTime.Now.Ticks;
		if (time - wooshSoundLastPlayed > wooshSoundConsecutiveDelay)
		{
			wooshSound[RandomNumber.Generate(0, numWooshSounds)].Stop();
			wooshSound[RandomNumber.Generate(0, numWooshSounds)].Play();
			wooshSoundLastPlayed = time;
		}
	}

	public static void PlayBoofSound()
	{
		ThreadPool.QueueUserWorkItem(ignoredState =>
		{
			boofSound[RandomNumber.Generate(0, numBoofSounds)].PlaySync();
		});
	}

	public static void Initialize()
	{
		goodSound = new SoundPlayer[numGoodSounds];
		badSound = new SoundPlayer[numBadSounds];
		clickSound = new SoundPlayer[numClickSounds];
		wooshSound = new SoundPlayer[numWooshSounds];
		boofSound = new SoundPlayer[numBoofSounds];

		for (int i = 0; i < numGoodSounds; ++i)
			goodSound[i] = new SoundPlayer(ResourceManager.GetStream("good" + i + ".wav"));
		for (int i = 0; i < numBadSounds; ++i)
			badSound[i] = new SoundPlayer(ResourceManager.GetStream("bad" + i + ".wav"));
		for (int i = 0; i < numClickSounds; ++i)
			clickSound[i] = new SoundPlayer(ResourceManager.GetStream("click" + i + ".wav"));
		for (int i = 0; i < numWooshSounds; ++i)
			wooshSound[i] = new SoundPlayer(ResourceManager.GetStream("woosh" + i + ".wav"));
		for (int i = 0; i < numBoofSounds; ++i)
			boofSound[i] = new SoundPlayer(ResourceManager.GetStream("boof" + i + ".wav"));
	}
}
