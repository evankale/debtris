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
using System.Threading;

class GameThread
{
	private const float targetFPS = 3;
	private const int numMovesBeforeBlockFreeze = 3;

	private Board board;
	private ActiveBlock activeBlock;
	private int msPerFrame;
	private int blockFreezeMovesCtr;
	private volatile bool isStopped;

	private List<string> characterList;
	private int currentCharacterIndex;

	public GameThread()
	{
		msPerFrame = (int)(1000 / targetFPS);
		Square.InitializeClass();
		Block.InitializeClass();
		SoundManager.Initialize();
		characterList = new List<string>();
		characterList.Add("evan");
		characterList.Add("trumpus");
		characterList.Add("truedough");
		currentCharacterIndex = 0;
	}

	public void NextCharacter()
	{
		currentCharacterIndex++;
		if (currentCharacterIndex >= characterList.Count)
			currentCharacterIndex = 0;
		Reset();
	}

	public void Reset()
	{
		board = new Board(characterList[currentCharacterIndex]);
		blockFreezeMovesCtr = 0;
		isStopped = false;
		board.Draw(true);
		CreateNewBlock();
		KeyListener.keyQueue.Clear();
	}

	public void Start()
	{
		board = new Board(characterList[currentCharacterIndex]);
		blockFreezeMovesCtr = 0;
		isStopped = false;
		board.Draw(true);
		CreateNewBlock();
		KeyListener.keyQueue.Clear();

		while (!isStopped)
		{
			long startMs = DateTime.Now.Ticks / 10000;
			ProcessInput();
			Update();
			board.Draw(false);
			int frameMs = (int)(DateTime.Now.Ticks / 10000 - startMs);
			int sleepTime = (int)(msPerFrame - frameMs);
			if (sleepTime > 0)
				Thread.Sleep(sleepTime);
		}
	}

	public void Stop()
	{
		isStopped = true;
	}

	private void ProcessInput()
	{
		int numKeysToDequeue = KeyListener.keyQueue.Count;

		for (int i = 0; i < numKeysToDequeue; ++i)
		{
			ConsoleKey keyPressed = 0;

			if (KeyListener.keyQueue.TryPop(out keyPressed))
			{
				if (activeBlock != null)
					switch (keyPressed)
					{
						case KeyListener.keyMoveRight:
							activeBlock.Move(1, 0);
							break;
						case KeyListener.keyMoveLeft:
							activeBlock.Move(-1, 0);
							break;
						case KeyListener.keyMoveDown:
							MoveActiveBlockDown();
							break;
						case KeyListener.keyRotate:
							activeBlock.Rotate();
							break;
						case KeyListener.keyChangeCharater:
							NextCharacter();
							break;
					}
			}
		}
	}

	private void Update()
	{
		if (activeBlock != null)
		{
			MoveActiveBlockDown();
		}
		else if (activeBlock == null && !board.IsResetting())
		{
			CreateNewBlock();
		}

		board.UpdateAnimations();
	}

	private void MoveActiveBlockDown()
	{
		bool hasMovedDown = activeBlock.Move(0, 1);
		if (hasMovedDown)
		{
			blockFreezeMovesCtr = 0;
		}
		else
		{
			blockFreezeMovesCtr++;

			if (blockFreezeMovesCtr >= numMovesBeforeBlockFreeze)
			{
				activeBlock.Freeze();
				board.ClearFilledRows();
				KeyListener.keyQueue.Clear();
				blockFreezeMovesCtr = 0;
				activeBlock = null;
				SoundManager.PlayBoofSound();
			}
		}
	}

	private void CreateNewBlock()
	{
		activeBlock = new ActiveBlock(board);
		if (activeBlock.cannotBeSpawned)
		{
			board.SetAllBlocksColor(Square.deadColor);
			activeBlock.ForcePlace();
			SoundManager.PlayBoofSound();
			board.Reset();
			activeBlock = null;
		}
	}
}
