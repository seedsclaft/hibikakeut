using UnityEngine;

public static class RectTransformExtension
{
    public enum AnchorPresets
    {
        TopLeft,
        TopCenter,
        TopRight,

        MiddleLeft,
        MiddleCenter,
        MiddleRight,

        BottomLeft,
        BottomCenter,
        BottomRight,
        BottomStretch,

        VertStretchLeft,
        VertStretchRight,
        VertStretchCenter,

        HorStretchTop,
        HorStretchMiddle,
        HorStretchBottom,

        StretchAll
    }

    public enum PivotPresets
    {
        TopLeft,
        TopCenter,
        TopRight,

        MiddleLeft,
        MiddleCenter,
        MiddleRight,

        BottomLeft,
        BottomCenter,
        BottomRight,
    }

    public static void SetAnchoredPositionY(this RectTransform r, float y)
    {
        var position = r.anchoredPosition;
        position.y = y;
        r.anchoredPosition = position;
    }

    // 参考：https://forum.unity.com/threads/test-if-ui-element-is-visible-on-screen.276549/
    /// <summary>
    /// 与えられたRectTransformのバウンディングボックスのコーナーのうち、スクリーンスペースに表示されているものを数える。
    /// 毎Update呼ぶと重いので注意。
    /// </summary>
    /// <returns>The amount of bounding box corners that are visible.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="camera">Camera. Leave it null for Overlay Canvasses.</param>
    private static int CountCornersVisibleFrom(this RectTransform rectTransform, Rect screenBounds, Camera camera = null)
    {
        if (screenBounds == null) {
            // 20230626:もしnullならスクリーンサイズを入れる
            screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
        }
        var objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(objectCorners);

        int visibleCorners = 0;
        Vector3 tempScreenSpaceCorner; // Cached
        for (int i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
        {
            if (camera != null)
                tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
            else {
                //Debug.Log(rectTransform.gameObject.name + " :: " + objectCorners[i].ToString("F2"));
                tempScreenSpaceCorner = objectCorners[i]; // If no camera is provided we assume the canvas is Overlay and world space == screen space
            }

            if (screenBounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
            {
                visibleCorners++;
            }
        }
        return visibleCorners;
    }

    /// <summary>
    /// このRectTransformが完全に表示されているかどうかを判定する。
    /// Works by checking if each bounding box corner of this RectTransform is inside the screen space view frustrum.
    /// </summary>
    /// <returns><c>true</c> if is fully visible; otherwise, <c>false</c>.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="screenBounds">可視化範囲.</param>
    /// <param name="isCheckActiveInHierarchy">ヒエラルキー上の親のActiveをチェックするか？true：チェックする.</param>
    /// <param name="camera">Camera. Leave it null for Overlay Canvasses.</param>
    public static bool IsFullyVisibleFrom(this RectTransform rectTransform, Rect screenBounds, bool isCheckActiveInHierarchy = true, Camera camera = null)
    {
        if (isCheckActiveInHierarchy) {
            if (!rectTransform.gameObject.activeInHierarchy) {
                // ヒエラルキー上の親がActiveOFFの場合
                return false;
            }
        }

        return CountCornersVisibleFrom(rectTransform, screenBounds, camera) == 4; // 4つの角がすべて表示されている場合は True
    }

    /// <summary>
    /// このRectTransformが少なくとも部分的に表示されているかどうかを判定する。
    /// Works by checking if any bounding box corner of this RectTransform is inside the screen space view frustrum.
    /// </summary>
    /// <returns><c>true</c> if is at least partially visible; otherwise, <c>false</c>.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="screenBounds">可視化範囲.</param>
    /// <param name="isCheckActiveInHierarchy">ヒエラルキー上の親のActiveをチェックするか？true：チェックする.</param>
    /// <param name="camera">Camera. Leave it null for Overlay Canvasses.</param>
    public static bool IsVisibleFrom(this RectTransform rectTransform, Rect screenBounds, bool isCheckActiveInHierarchy = true, Camera camera = null)
    {
        if (isCheckActiveInHierarchy) {
            if (!rectTransform.gameObject.activeInHierarchy) {
                // ヒエラルキー上の親がActiveOFFの場合
                return false;
            }
        }

        return CountCornersVisibleFrom(rectTransform, screenBounds, camera) > 0; // 角が表示される場合はtrue
    }


    /// <summary>
    /// RectTransformのAnchorのプリセットをスクリプトから「インスペクタ上のenumと同じように」
    /// 指定する為の拡張クラス
    /// </summary>
    public static void SetAnchor(this RectTransform source, AnchorPresets anchorPreset, float offsetX = 0, float offsetY = 0)
    {
        source.anchoredPosition = new Vector3(offsetX, offsetY, 0);

        switch (anchorPreset) {
            case AnchorPresets.TopLeft: {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
            case AnchorPresets.TopCenter: {
                    source.anchorMin = new Vector2(0.5f, 1);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
            case AnchorPresets.TopRight: {
                    source.anchorMin = new Vector2(1, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

            case AnchorPresets.MiddleLeft: {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(0, 0.5f);
                    break;
                }
            case AnchorPresets.MiddleCenter: {
                    source.anchorMin = new Vector2(0.5f, 0.5f);
                    source.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                }
            case AnchorPresets.MiddleRight: {
                    source.anchorMin = new Vector2(1, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }

            case AnchorPresets.BottomLeft: {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 0);
                    break;
                }
            case AnchorPresets.BottomCenter: {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 0);
                    break;
                }
            case AnchorPresets.BottomRight: {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

            case AnchorPresets.HorStretchTop: {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }
            case AnchorPresets.HorStretchMiddle: {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }
            case AnchorPresets.HorStretchBottom: {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

            case AnchorPresets.VertStretchLeft: {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
            case AnchorPresets.VertStretchCenter: {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
            case AnchorPresets.VertStretchRight: {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

            case AnchorPresets.StretchAll: {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }
            case AnchorPresets.BottomStretch:
                break;
            default:
                break;
        }
    }
    public static void SetPivot(this RectTransform source, PivotPresets preset)
    {
        switch (preset) {
            case PivotPresets.TopLeft: {
                    source.pivot = new Vector2(0, 1);
                    break;
                }
            case PivotPresets.TopCenter: {
                    source.pivot = new Vector2(0.5f, 1);
                    break;
                }
            case PivotPresets.TopRight: {
                    source.pivot = new Vector2(1, 1);
                    break;
                }

            case PivotPresets.MiddleLeft: {
                    source.pivot = new Vector2(0, 0.5f);
                    break;
                }
            case PivotPresets.MiddleCenter: {
                    source.pivot = new Vector2(0.5f, 0.5f);
                    break;
                }
            case PivotPresets.MiddleRight: {
                    source.pivot = new Vector2(1, 0.5f);
                    break;
                }

            case PivotPresets.BottomLeft: {
                    source.pivot = new Vector2(0, 0);
                    break;
                }
            case PivotPresets.BottomCenter: {
                    source.pivot = new Vector2(0.5f, 0);
                    break;
                }
            case PivotPresets.BottomRight: {
                    source.pivot = new Vector2(1, 0);
                    break;
                }

            default:
                break;
        }
    }
}