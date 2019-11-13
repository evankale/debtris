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

using Photoshop;

public class Background
{
	private const string bgFilePathSuffix = ".png";

	private ApplicationClass psApp;
	private Document psDoc;

	private int currentBgNum;
	private ArtLayer[] bgLayers;
	private string bgFileNamePrefix;
	private int numFrames;
	private int numUpdatesBeforeNextImage;
	private int updateCtr;

	public Background(ApplicationClass psApp, Document psDoc, int resizeToWidth, int resizeToHeight, string bgFileNamePrefix, int numFrames, int numUpdatesBeforeNextImage)
	{
		this.psApp = psApp;
		this.psDoc = psDoc;
		this.numFrames = numFrames;
		this.bgFileNamePrefix = bgFileNamePrefix;
		this.numUpdatesBeforeNextImage = numUpdatesBeforeNextImage;

		updateCtr = 0;
		bgLayers = new ArtLayer[numFrames];

		Document tempDoc;
		ArtLayer tempLayer;

		for (int i = numFrames - 1; i >= 0; --i)
		{
			tempDoc = psApp.Open(ResourceManager.GetResourceFolderPath() + bgFileNamePrefix + i + bgFilePathSuffix);
			tempDoc.ResizeImage(resizeToWidth, resizeToHeight, null, PsResampleMethod.psNearestNeighbor);
			tempLayer = (ArtLayer)tempDoc.ActiveLayer;
			bgLayers[i] = (ArtLayer)tempLayer.Duplicate(psDoc, PsElementPlacement.psPlaceAtBeginning);
			tempDoc.Close(PsSaveOptions.psDoNotSaveChanges);
			psApp.ActiveDocument = psDoc;
			bgLayers[i].Name = bgFileNamePrefix + i;
		}

		currentBgNum = 0;
	}

	public void Update()
	{
		++updateCtr;

		if (updateCtr == numUpdatesBeforeNextImage)
		{
			NextImage();
			updateCtr = 0;
		}
	}

	private void NextImage()
	{
		if (currentBgNum == numFrames - 1)
		{
			bgLayers[0].Visible = true;

			if (numFrames > 1)
				bgLayers[1].Visible = true;

			currentBgNum = 0;
		}
		else
		{
			bgLayers[currentBgNum].Visible = false;

			if (currentBgNum + 2 < numFrames)
				bgLayers[currentBgNum + 2].Visible = true;

			++currentBgNum;
		}
	}

}
