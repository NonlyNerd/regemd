﻿/*
 * Copyright (c) 2017 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour {
	private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
	private static Tile previousSelected = null;

	private SpriteRenderer spriteRenderer;
	private bool isSelected = false;

	private Vector2[] adjacentDirections = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

	private bool matchFound = false;

	void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
    }

	void OnMouseDown() {
		if (spriteRenderer.sprite == null || BoardManager.instance.IsShifting) {
			return;
		}

		if (isSelected) {
			Deselect();
		} else {
			if (previousSelected == null) {
				Select();
			} else {
				if (GetAllAdjacentTiles().Contains(previousSelected.gameObject)) {
					GUIManager.instance.MoveCounter--;
					SwapSprite(previousSelected.spriteRenderer);
					previousSelected.ClearAllMatches();
					previousSelected.Deselect();
					ClearAllMatches();
				} else {
					//previousSelected.GetComponent<Tile>().Deselect();
					
					previousSelected.Deselect();
					
					Select();
				}
			}
		}
	}

	private void SwapSprite(SpriteRenderer secondSpriteRenderer) {
		if (spriteRenderer.sprite == secondSpriteRenderer.sprite) {
			return;
		}

		Sprite swap = secondSpriteRenderer.sprite;
		secondSpriteRenderer.sprite = spriteRenderer.sprite;
		spriteRenderer.sprite = swap;
		SFXManager.instance.PlaySFX(Clip.Swap);
	}

	private GameObject GetAdjacent(Vector2 castDir) {
		RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);

		if (hit.collider != null) {
			return hit.collider.gameObject;
		}

		return null;
	}

	private List<GameObject> GetAllAdjacentTiles() {
		List<GameObject> adjacentTiles = new List<GameObject>();

		for (int i = 0; i < adjacentDirections.Length; i++) {
			adjacentTiles.Add(GetAdjacent(adjacentDirections[i]));
		}

		return adjacentTiles;
	}

	private List<GameObject> FindMatch(Vector2 castDir) {
		List<GameObject> matchingTiles = new List<GameObject>();
		RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);

		while(hit.collider != null && hit.collider.GetComponent<SpriteRenderer>().sprite == spriteRenderer.sprite) {
			matchingTiles.Add(hit.collider.gameObject);
			hit = Physics2D.Raycast(hit.collider.transform.position, castDir);
		}

		return matchingTiles;
	}

	private void ClearMatch(Vector2[] paths) {
		List<GameObject> matchingTiles = new List<GameObject>();

		for (int i = 0; i < paths.Length; i++) {
			matchingTiles.AddRange(FindMatch(paths[i]));
		}

		if (matchingTiles.Count >= 2) {
			for (int i = 0; i < matchingTiles.Count; i++) {
				matchingTiles[i].GetComponent<SpriteRenderer>().sprite = null;
			}

			matchFound = true;
		}
	}

	public void ClearAllMatches() {
    if (spriteRenderer.sprite == null) {
		return;
	}

    ClearMatch(new Vector2[2] { Vector2.left, Vector2.right });
    ClearMatch(new Vector2[2] { Vector2.up, Vector2.down });
    if (matchFound) {
        spriteRenderer.sprite = null;
        matchFound = false;
		StopCoroutine(BoardManager.instance.FindNullTiles());
		StartCoroutine(BoardManager.instance.FindNullTiles());
        SFXManager.instance.PlaySFX(Clip.Clear);
    }
}


	private void Select() {
		isSelected = true;
		spriteRenderer.color = selectedColor;
		previousSelected = gameObject.GetComponent<Tile>();
		SFXManager.instance.PlaySFX(Clip.Select);
	}

	private void Deselect() {
		isSelected = false;
		spriteRenderer.color = Color.white;
		previousSelected = null;
	}

}