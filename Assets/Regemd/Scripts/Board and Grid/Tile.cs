/*
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

	private int rowSize = 2;

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
				if (IsAdjacent(previousSelected.gameObject)) {
					SwapSprite(previousSelected.spriteRenderer);
				} else {					
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

		StartCoroutine(AnimateSwap(spriteRenderer, secondSpriteRenderer));
	}

	private IEnumerator AnimateSwap(SpriteRenderer firstSpriteRenderer, SpriteRenderer secondSpriteRenderer, float shiftDelay = .2f) {
		Sprite swap = secondSpriteRenderer.sprite;
		secondSpriteRenderer.sprite = firstSpriteRenderer.sprite;
		firstSpriteRenderer.sprite = swap;

		if (FindAnyMatches() || previousSelected.FindAnyMatches()) {
			SFXManager.instance.PlaySFX(Clip.Swap);
			MakeMove();
		} else {
			yield return new WaitForSeconds(shiftDelay);

			swap = firstSpriteRenderer.sprite;
			firstSpriteRenderer.sprite = secondSpriteRenderer.sprite;
			secondSpriteRenderer.sprite = swap;

			previousSelected.Deselect();
		}
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

	private bool IsAdjacent(GameObject objectToCheck) {
		return GetAllAdjacentTiles().Contains(objectToCheck);
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

	private List<GameObject> FindMatches(Vector2[] paths) {
		List<GameObject> matchingTiles = new List<GameObject>();

		for (int i = 0; i < paths.Length; i++) {
			matchingTiles.AddRange(FindMatch(paths[i]));
		}

		if (matchingTiles.Count >= rowSize) {
			matchFound = true;
		}

		return matchingTiles;
	}

	public bool FindAnyMatches() {
		if (spriteRenderer.sprite == null) {
			return false;
		}

		bool horizontal = FindMatches(new Vector2[2] { Vector2.left, Vector2.right }).Count >= rowSize;
		bool vertical = FindMatches(new Vector2[2] { Vector2.up, Vector2.down }).Count >= rowSize;

		return horizontal || vertical;
	}

	private void ClearMatch(Vector2[] paths) {
		List<GameObject> matchingTiles = FindMatches(paths);

		if (matchFound) {
			for (int i = 0; i < matchingTiles.Count; i++) {
				matchingTiles[i].GetComponent<SpriteRenderer>().sprite = null;
			}
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

	public void MakeMove() {
		GUIManager.instance.MoveCounter--;
		previousSelected.ClearAllMatches();
		previousSelected.Deselect();
		ClearAllMatches();
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