using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class ColorPicker : MonoBehaviour {
    #region editor
    [SerializeField] private SpriteRenderer spriteToPickColor = default;
    #endregion

    #region public properties
    public Sprite sprite => spriteToPickColor.sprite;
    #endregion

    private Plane plane;

    #region private
    private void Awake() {
        plane = new Plane(spriteToPickColor.transform.forward, spriteToPickColor.transform.position);
    }

    private void Start() {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Pictures");
        GameSettings gameSettings = GameController.instance.gameSettings;
        spriteToPickColor.sprite = sprites.First(s => s.name == gameSettings.currentSavedLevel.ToString());

        //Transform cube = transform.GetChild(0);
        //Color color = getPixelAverageColor(cube.transform.position, Vector3.down,
        //    Mathf.Min(cube.transform.localScale.x, cube.transform.localScale.z),
        //    24);
        //cube.GetComponent<MeshRenderer>().material.color = color;
    }
    #endregion

    #region public
    public bool getPixelColor(Vector3 rayOrigin, Vector3 rayAxis, out Color pixelColor){
        pixelColor = Color.white; // default color

        Ray ray = new Ray(rayOrigin, rayAxis);

        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.yellow, 60f);

        Sprite sprite = spriteToPickColor.sprite;
        Texture2D spriteTexture = sprite.texture;

        float rayIntersectDist;
        if(!plane.Raycast(ray, out rayIntersectDist)) return false;

        Vector3 spritePos = spriteToPickColor.worldToLocalMatrix.MultiplyPoint3x4(ray.origin + (ray.direction * rayIntersectDist));
        Rect textureRect = sprite.textureRect;

        float pixelPerUnit = sprite.pixelsPerUnit;
        float halfRealTextureWidth = spriteTexture.width * .5f;
        float halfRealTextureHeight = spriteTexture.height * .5f;

        int texturePosX = (int)(spritePos.x * pixelPerUnit + halfRealTextureWidth);
        int texturePosY = (int)(spritePos.y * pixelPerUnit + halfRealTextureHeight);
        if(texturePosX < 0 || texturePosX < textureRect.x || texturePosX >= Mathf.FloorToInt(textureRect.xMax)) return false;
        if(texturePosY < 0 || texturePosY < textureRect.y || texturePosY >= Mathf.FloorToInt(textureRect.yMax)) return false;

        pixelColor = spriteTexture.GetPixel(texturePosX, texturePosY);

        // print($"spritePos: {spritePos}" +
        //       $"textureRect: {textureRect}" +
        //       $"texturePos: {new Vector2(texturePosX, texturePosY)}" + 
        //       $"color: {pixelColor}");

        return true;
    }

    public Color getPixelAverageColor(Transform origin, Vector3 rayAxis) {
        List<Color> colors = new List<Color>();
        Color outColor;
        Vector3Int[] neighborIndexes = new Vector3Int[]{
            new Vector3Int(-1, 0, 1), new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 1),
            new Vector3Int(-1, 0, 0), new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, -1), new Vector3Int(0, 0, -1), new Vector3Int(1, 0, -1)
        };
        
        foreach(var index in neighborIndexes) {
            Vector3 rndDirection = new Vector3(index.x * origin.localScale.x, index.y, index.z * origin.localScale.z) * .75f;
            Vector3 newOrigin = origin.position + new Vector3(rndDirection.x, origin.position.y, rndDirection.z);
            if (getPixelColor(newOrigin, rayAxis, out outColor)) colors.Add(outColor);
        }

        Color result = Color.white;
        if (colors.Count != 0) {
            result = colors.First();
            foreach (Color c in colors.Skip(1)) result += c;
            result /= colors.Count;
        }
        return result;
    }
    #endregion
}