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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

class KeyListener
{
	public const ConsoleKey keyExit = ConsoleKey.X;
	public const ConsoleKey keyMoveLeft = ConsoleKey.A;
	public const ConsoleKey keyMoveRight = ConsoleKey.D;
	public const ConsoleKey keyMoveDown = ConsoleKey.S;
	public const ConsoleKey keyRotate = ConsoleKey.E;
	public const ConsoleKey keyChangeCharater = ConsoleKey.Y;
	private const string consoleTitle = "DEBTRIS Controller Window";

	public static ConcurrentStack<ConsoleKey> keyQueue = new ConcurrentStack<ConsoleKey>();
	private static List<ConsoleKey> validKeys = new List<ConsoleKey>();

	public static bool ReadConsoleKey()
	{
		ConsoleKeyInfo keyPressed = Console.ReadKey();
		Console.Write("\b");

		//Exit key
		if (keyPressed.Key == keyExit)
			return false;

		if (validKeys.Contains(keyPressed.Key))
		{
			keyQueue.Push(keyPressed.Key);

			if (keyPressed.Key == keyMoveLeft
				|| keyPressed.Key == keyMoveRight
				|| keyPressed.Key == keyMoveDown)
				SoundManager.PlayClickSound();
			else if (keyPressed.Key == keyRotate)
				SoundManager.PlayWooshSound();
		}

		return true;
	}

	public static void Initialize()
	{
		validKeys.Add(keyMoveLeft);
		validKeys.Add(keyMoveRight);
		validKeys.Add(keyMoveDown);
		validKeys.Add(keyRotate);
		validKeys.Add(keyChangeCharater);

		const int SWP_NOZORDER = 0x4;
		const int SWP_NOACTIVATE = 0x10;
		const int SW_RESTORE = 9;
		var hWnd = GetConsoleWindow();

		//Set size and position of console
		Console.WindowWidth = 40;
		Console.WindowHeight = 15;
		Console.BufferWidth = 40;
		Console.BufferHeight = 15;
		Console.Title = consoleTitle;
		SetWindowPos(hWnd, IntPtr.Zero, 25, 25, 400, 500, SWP_NOZORDER | SWP_NOACTIVATE);

		//Focus the console
		ShowWindowAsync(hWnd, SW_RESTORE);
		SetForegroundWindow(hWnd);

		ShowConsoleInstructions();
	}

	private static void ShowConsoleInstructions()
	{
		Console.WriteLine("Bring focus here to play.");
		Console.WriteLine(keyMoveLeft + " to move left.");
		Console.WriteLine(keyMoveRight + " to move right.");
		Console.WriteLine(keyMoveDown + " to move down.");
		Console.WriteLine(keyRotate + " to rotate.");
		Console.WriteLine(keyExit + " to exit.");
		Console.WriteLine(keyChangeCharater + " to change character.");
		Console.WriteLine();
	}

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetConsoleWindow();
	[DllImport("user32.dll")]
	private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
	[DllImport("user32.dll")]
	private static extern bool SetForegroundWindow(IntPtr hWnd);
	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);
}

