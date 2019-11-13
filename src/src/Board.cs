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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Photoshop;

public class Board
{
	public const int numSquaresX = 10;
	public const int numSquaresY = 20;

	private const int squareSize = 24;
	private const int squareSpacing = 3;

	private const int canvasW = 600;
	private const int canvasH = 800;
	private const string canvasTitle = "EvanKale4Ruler";

	private const int numResetFrames = 7;
	private int currResetFrameNum = -1;

	private int boardX, boardY, boardW, boardH;
	private Square[][] squares;
	private int[] disappearingRowsAnimationCtr;
	private Background backgroundImage;

	private StringBuilder javascriptBuffer = new StringBuilder("", 10000);

	private ApplicationClass psApp;
	private Document psDoc;

	private ArtLayer blockLayer;
	private ArtLayer squareLayer;

	public Board(string character)
	{
		InitializePhotoshop();

		backgroundImage = new Background(psApp, psDoc, canvasW, canvasH, character, 4, 1);

		squareLayer = psDoc.ArtLayers.Add();
		squareLayer.Name = "squares";
		squareLayer.Opacity = 32;

		blockLayer = psDoc.ArtLayers.Add();
		blockLayer.Name = "blocks";

		boardW = numSquaresX * (squareSize + squareSpacing) - squareSpacing;
		boardH = numSquaresY * (squareSize + squareSpacing) - squareSpacing;
		boardX = (canvasW - boardW) / 2;
		boardY = (canvasH - boardH) / 2;

		squares = new Square[numSquaresX][];
		for (int i = 0; i < numSquaresX; ++i)
		{
			squares[i] = new Square[numSquaresY];
			for (int j = 0; j < numSquaresY; ++j)
			{
				squares[i][j] = new Square();
			}
		}

		disappearingRowsAnimationCtr = new int[squares[0].Length];
		for (int i = 0; i < disappearingRowsAnimationCtr.Length; ++i)
			disappearingRowsAnimationCtr[i] = -1;
	}

	public bool IsValid(int x, int y)
	{
		if (x < 0 || x >= numSquaresX
			|| y < 0 || y >= numSquaresY)
			return false;
		return true;
	}

	public bool IsEmpty(int x, int y)
	{
		if (IsValid(x, y)
			&& (squares[x][y].color == null || squares[x][y].isActive))
			return true;
		return false;
	}

	public void Clear(int x, int y)
	{
		if (IsValid(x, y))
		{
			if (squares[x][y].color != null)
			{
				squares[x][y].color = null;
				squares[x][y].isDirty = true;
				squares[x][y].isActive = false;
			}
		}
	}

	public void Deactivate(int x, int y)
	{
		if (IsValid(x, y))
		{
			squares[x][y].isActive = false;
		}
	}

	public void SetColor(RGBi color, int x, int y, bool isActive)
	{
		if (IsValid(x, y))
		{
			if (squares[x][y].color != color)
			{
				squares[x][y].color = color;
				squares[x][y].isDirty = true;
				squares[x][y].isActive = isActive;
			}
		}
	}

	public void PlaceBlock(Block block, int locX, int locY, int rotation, bool isActive)
	{
		for (int blockX = 0; blockX < block.Width(); ++blockX)
		{
			for (int blockY = 0; blockY < block.Width(); ++blockY)
			{
				int boardX = locX + blockX;
				int boardY = locY + blockY;
				bool isBlockSquare = block.GetSquare(blockX, blockY, rotation);

				if (isBlockSquare)
				{
					SetColor(block.color, boardX, boardY, isActive);
				}
			}
		}
	}

	public void DeactivateBlock(Block block, int locX, int locY, int rotation)
	{
		for (int blockX = 0; blockX < block.Width(); ++blockX)
		{
			for (int blockY = 0; blockY < block.Width(); ++blockY)
			{
				int boardX = locX + blockX;
				int boardY = locY + blockY;
				bool isBlockSquare = block.GetSquare(blockX, blockY, rotation);

				if (isBlockSquare)
				{
					Deactivate(boardX, boardY);
				}
			}
		}
	}

	public void ClearBlock(Block block, int locX, int locY, int rotation)
	{
		for (int blockX = 0; blockX < block.Width(); ++blockX)
		{
			for (int blockY = 0; blockY < block.Width(); ++blockY)
			{
				int boardX = locX + blockX;
				int boardY = locY + blockY;
				bool isBlockSquare = block.GetSquare(blockX, blockY, rotation);

				if (isBlockSquare)
				{
					Clear(boardX, boardY);
				}
			}
		}
	}

	public bool IsBlockValid(Block block, int locX, int locY, int rotation)
	{
		for (int blockX = 0; blockX < block.Width(); ++blockX)
		{
			for (int blockY = 0; blockY < block.Width(); ++blockY)
			{
				bool isBlockSquare = block.GetSquare(blockX, blockY, rotation);

				if (isBlockSquare)
				{
					int x = locX + blockX;
					int y = locY + blockY;

					if (!IsEmpty(x, y))
						return false;
				}
			}
		}

		return true;
	}

	public void Draw(bool isDrawingSquareLayer)
	{
		if (isDrawingSquareLayer)
			psDoc.ActiveLayer = squareLayer;
		else
			psDoc.ActiveLayer = blockLayer;

		// This unwrapped javascript code runs faster due to
		// suspendHistory capability in the javascript library.

		//have one subpath for each color
		Dictionary<RGBi, List<Pt2i>> colorSubpaths = new Dictionary<RGBi, List<Pt2i>>();

		//Take all dirtied squares and add to respective color subpath
		for (int y = 0; y < numSquaresY; ++y)
		{
			for (int x = 0; x < numSquaresX; ++x)
			{
				if (squares[x][y].isDirty)
				{
					int xBrushLoc = boardX + x * (squareSize + squareSpacing) + squareSize / 2;
					int yBrushLoc = boardY + y * (squareSize + squareSpacing) + squareSize / 2;
					RGBi colorKey = squares[x][y].color;

					if (colorKey == null)
						colorKey = Square.emptyColor;

					if (!colorSubpaths.ContainsKey(colorKey))
						colorSubpaths[colorKey] = new List<Pt2i>();

					colorSubpaths[colorKey].Add(new Pt2i(xBrushLoc, yBrushLoc));
					squares[x][y].isDirty = false;
				}
			}
		}

		javascriptBuffer.Append("activeDocument.suspendHistory(\"DrawBoard\",\"");

		//Loop through all colors and stroke the subpaths
		int i = 0;
		foreach (KeyValuePair<RGBi, List<Pt2i>> colorSubpathEntry in colorSubpaths)
		{
			//if drawing square layer, then only draw when RGBi is empty color
			if (isDrawingSquareLayer && colorSubpathEntry.Key != Square.emptyColor)
				continue;

			RGBi strokeColor = colorSubpathEntry.Key;
			Pt2i[] colorSubpath = colorSubpathEntry.Value.ToArray();

			if (colorSubpath.Length > 0)
			{
				for (int j = 0; j < colorSubpath.Length; ++j)
				{
					javascriptBuffer.Append("var lineArray" + i + "_" + j + " = new Array(); ");
					javascriptBuffer.Append("lineArray" + i + "_" + j + "[0] = new PathPointInfo; ");
					javascriptBuffer.Append("lineArray" + i + "_" + j + "[0].kind = PointKind.CORNERPOINT; ");
					javascriptBuffer.Append("lineArray" + i + "_" + j + "[0].anchor = Array(" + colorSubpath[j].x + ", " + colorSubpath[j].y + "); ");
					javascriptBuffer.Append("lineArray" + i + "_" + j + "[0].leftDirection = lineArray" + i + "_" + j + "[0].anchor; ");
					javascriptBuffer.Append("lineArray" + i + "_" + j + "[0].rightDirection = lineArray" + i + "_" + j + "[0].anchor; ");
				}

				javascriptBuffer.Append("var lineSubPathArray" + i + " = new Array(); ");

				for (int j = 0; j < colorSubpath.Length; ++j)
				{
					javascriptBuffer.Append("lineSubPathArray" + i + "[" + j + "] = new SubPathInfo(); ");
					javascriptBuffer.Append("lineSubPathArray" + i + "[" + j + "].operation = ShapeOperation.SHAPEXOR; ");
					javascriptBuffer.Append("lineSubPathArray" + i + "[" + j + "].closed = false; ");
					javascriptBuffer.Append("lineSubPathArray" + i + "[" + j + "].entireSubPath = lineArray" + i + "_" + j + "; ");
				}

				//create the path item
				javascriptBuffer.Append("var myPathItem" + i + " = activeDocument.pathItems.add(\\\"Color" + i + "\\\", lineSubPathArray" + i + "); ");

				//Stroke it so we can see something.
				// if drawing block layer and RGBi is empty color, then use eraser
				if (!isDrawingSquareLayer && colorSubpathEntry.Key == Square.emptyColor)
				{
					javascriptBuffer.Append("myPathItem" + i + ".strokePath(ToolType.ERASER); ");
				}
				// otherwise, use pencil
				else
				{
					//set foreground color
					javascriptBuffer.Append("app.foregroundColor.rgb.hexValue = \'" + strokeColor.ToHexString() + "\'; ");
					javascriptBuffer.Append("myPathItem" + i + ".strokePath(ToolType.PENCIL); ");
				}
			}
			++i;
		}

		javascriptBuffer.Append("activeDocument.pathItems.removeAll(); ");
		javascriptBuffer.Append("\")");

		try { psApp.DoJavaScript(javascriptBuffer.ToString()); }
		catch (Exception) { }

		javascriptBuffer.Clear();
	}

	public void SetAllBlocksColor(RGBi color)
	{
		for (int x = 0; x < squares.Length; ++x)
		{
			for (int y = 0; y < squares[0].Length; ++y)
			{
				if (squares[x][y].color != null)
				{
					squares[x][y].color = color;
					squares[x][y].isDirty = true;
					squares[x][y].isActive = false;
				}
			}
		}
	}

	public void ClearFilledRows()
	{
		for (int y = 0; y < squares[0].Length; ++y)
		{
			//if row is not already disappearing
			if (disappearingRowsAnimationCtr[y] == -1)
			{
				bool rowFilled = false;
				for (int x = 0; x < squares.Length; ++x)
				{
					if (squares[x][y].color == null)
						break;
					if (x == squares.Length - 1)
						rowFilled = true;
				}

				if (rowFilled)
				{
					disappearingRowsAnimationCtr[y] = Square.numDisappearingColors;
					SoundManager.PlayGoodSound();
				}
			}
		}
	}

	public void UpdateAnimations()
	{
		//if is resetting, then do reset animations
		if (currResetFrameNum != -1)
		{
			currResetFrameNum--;

			int numMovingResetFrames = numResetFrames - 3;

			if (currResetFrameNum == numMovingResetFrames - 1)
			{
				SoundManager.PlayBadSound();
			}

			if (currResetFrameNum < numMovingResetFrames)
			{
				DeleteRowsFromBottom((int)Math.Ceiling((double)numSquaresY / (numMovingResetFrames)));
			}
		}
		//else check for row disappearance animatinos
		else
		{
			//update all disappearing rows
			for (int y = disappearingRowsAnimationCtr.Length - 1; y >= 0;)
			{
				if (disappearingRowsAnimationCtr[y] != -1)
				{
					disappearingRowsAnimationCtr[y]--;

					if (disappearingRowsAnimationCtr[y] == -1)
					{
						DeleteRow(y);
						// continue without decrementing y
						// because rows above y is now shifted
						// down 1 row after deletion.
						continue;
					}
					else
					{
						SetRowColor(Square.disappearingColor[disappearingRowsAnimationCtr[y]], y);
					}
				}
				--y;
			}
		}

		backgroundImage.Update();
	}

	public bool IsResetting()
	{
		return currResetFrameNum != -1;
	}

	public void Reset()
	{
		//begin reset animation
		currResetFrameNum = numResetFrames;

		//reset disappearingRowsAnimations
		for (int i = 0; i < disappearingRowsAnimationCtr.Length; ++i)
			disappearingRowsAnimationCtr[i] = -1;
	}

	private void SetRowColor(RGBi color, int rowNum)
	{
		for (int x = 0; x < squares.Length; ++x)
		{
			squares[x][rowNum].color = color;
			squares[x][rowNum].isDirty = true;
		}
	}

	private void DeleteRowsFromBottom(int numRows)
	{
		//Shift all rows numRows down
		for (int y = numSquaresY - 1; y >= 0; --y)
		{
			for (int x = 0; x < squares.Length; ++x)
			{
				if (y - numRows < 0)
				{
					squares[x][y].Clear();
				}
				else
				{
					squares[x][y].CopyFrom(squares[x][y - numRows]);
				}
			}
		}
	}

	private void DeleteRow(int rowNum)
	{
		//Shift everything above rowNum one row down
		for (int y = rowNum; y >= 0; --y)
		{
			for (int x = 0; x < squares.Length; ++x)
			{
				if (y == 0)
				{
					squares[x][y].Clear();
					disappearingRowsAnimationCtr[y] = -1;
				}
				else
				{
					if (squares[x][y].isActive)
					{
						//do nothing
					}
					else if (squares[x][y - 1].isActive)
					{
						squares[x][y].Clear();
					}
					else
					{
						squares[x][y].CopyFrom(squares[x][y - 1]);
					}
					disappearingRowsAnimationCtr[y] = disappearingRowsAnimationCtr[y - 1];
				}
			}
		}
	}

	private void InitializePhotoshop()
	{
		psApp = new ApplicationClass();
		psApp.Preferences.RulerUnits = PsUnits.psPixels;
		psApp.Preferences.TypeUnits = PsTypeUnits.psTypePixels;

		IEnumerator docEnum = psApp.Documents.GetEnumerator();
		List<Document> prevDebtrisDocs = new List<Document>();
		while (docEnum.MoveNext())
		{
			Document nextDoc = (Document)docEnum.Current;
			if (nextDoc.Name.Equals(canvasTitle))
				prevDebtrisDocs.Add(nextDoc);
		}
		foreach (Document doc in prevDebtrisDocs)
			doc.Close(PsSaveOptions.psDoNotSaveChanges);

		psDoc = psApp.Documents.Add(canvasW, canvasH, 76, canvasTitle,
			PsNewDocumentMode.psNewRGB, PsDocumentFill.psWhite, 1, PsBitsPerChannelType.psDocument8Bits);

		InitializeTools();
	}

	private void InitializeTools()
	{
		SelectTool("PcTl");
		try
		{
			SelectToolPreset("DebtrisPencil");
		}
		catch (Exception)
		{
			LoadToolPresets(ResourceManager.GetResourceFolderPath() + "tools.tpl");
			SelectToolPreset("DebtrisPencil");
		}
		SelectTool("ErTl");
		SelectToolPreset("DebtrisEraser");
	}

	private void SelectTool(string charID)
	{
		int idslct = psApp.CharIDToTypeID("slct");
		int idnull = psApp.CharIDToTypeID("null");
		int idtool = psApp.CharIDToTypeID(charID);
		ActionDescriptor actionDesc = new ActionDescriptor();
		ActionReference actionRef = new ActionReference();
		actionRef.PutClass(idtool);
		actionDesc.PutReference(idnull, actionRef);
		psApp.ExecuteAction(idslct, actionDesc, PsDialogModes.psDisplayNoDialogs);
	}

	private void LoadToolPresets(string toolsPath)
	{
		ActionDescriptor actionDesc = new ActionDescriptor();
		ActionReference actionRef = new ActionReference();
		int idsetd = psApp.CharIDToTypeID("setd");
		int idnull = psApp.CharIDToTypeID("null");
		int idPrpr = psApp.CharIDToTypeID("Prpr");
		int idtoolPreset = psApp.StringIDToTypeID("toolPreset");
		actionRef.PutProperty(idPrpr, idtoolPreset);
		int idcapp = psApp.CharIDToTypeID("capp");
		int idOrdn = psApp.CharIDToTypeID("Ordn");
		int idTrgt = psApp.CharIDToTypeID("Trgt");
		actionRef.PutEnumerated(idcapp, idOrdn, idTrgt);
		actionDesc.PutReference(idnull, actionRef);
		int idT = psApp.CharIDToTypeID("T   ");
		actionDesc.PutPath(idT, toolsPath);
		int idAppe = psApp.CharIDToTypeID("Appe");
		actionDesc.PutBoolean(idAppe, true);
		psApp.ExecuteAction(idsetd, actionDesc, PsDialogModes.psDisplayNoDialogs);
	}

	private void SelectToolPreset(string toolName)
	{
		ActionDescriptor actionDesc = new ActionDescriptor();
		ActionReference actionRef = new ActionReference();
		int idslct = psApp.CharIDToTypeID("slct");
		int idnull = psApp.CharIDToTypeID("null");
		int idtoolPreset = psApp.StringIDToTypeID("toolPreset");
		actionRef.PutName(idtoolPreset, toolName);
		actionDesc.PutReference(idnull, actionRef);
		psApp.ExecuteAction(idslct, actionDesc, PsDialogModes.psDisplayNoDialogs);
	}
}
