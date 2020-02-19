using Utils;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class PictureFrame : MonoBehaviour {
    #region editor
    [SerializeField] private SpriteRenderer background = default;
    [SerializeField] private SpriteRenderer spriteRenderer = default;

    [Space]
    [Header("Frame settings")]
    [SerializeField] private Transform cornersHolder = default;
    [SerializeField] private GameObject oneUnitFramePrefab = default;
    #endregion

    #region private
    private void Awake() {
        background.gameObject.SetActive(false);
    }

    private void renderCorners(){
        // clear frames
        cornersHolder.Cast<Transform>().ToList().ForEach(t => Destroy(t.gameObject));

        // render frame corners
        Vector3 srExtents = spriteRenderer.bounds.extents;
        Vector3 srPosition = spriteRenderer.transform.position;

        int[] xOffset = {-1, 1, 1, -1};
        int[] yOffset = {1, 1, -1, -1};

        for(int i = 0; i < xOffset.Length; i++) {
            float angle = -90f * i;
            float sizeDelta = spriteRenderer.bounds.size.y;
            if (i % 2 == 0) sizeDelta = spriteRenderer.bounds.size.x;

            Vector3 corner = new Vector3(srPosition.x + (srExtents.x * xOffset[i]), srPosition.y + (srExtents.y * yOffset[i]), srPosition.z);
            GameObject cornerFrame = Instantiate(oneUnitFramePrefab, corner, Quaternion.Euler(0f, 0f, angle), cornersHolder);
            cornerFrame.transform.localScale = new Vector3(cornerFrame.transform.localScale.x + sizeDelta,
                                                           cornerFrame.transform.localScale.y,
                                                           cornerFrame.transform.localScale.z);
        }
    }
    #endregion

    #region public
    public void setSprite(Sprite sprite){
        spriteRenderer.sprite = sprite;

        renderCorners();
        background.gameObject.SetActive(true);

        float sign = Utility.maybe ? -1f : 1f;
        Vector3 cachedPosition = transform.position;
        transform.Translate(Vector3.right * 50f * sign);
        transform.DORotate(Vector3.right * 65f, .5f);
        transform.localScale = Vector3.zero;

        transform.DOMove(cachedPosition, 1.5f).SetEase(Ease.OutExpo);

        spriteRenderer.color = Utility.transparentColor;
        spriteRenderer.DOColor(Color.white, 2f).SetEase(Ease.InQuad);

        transform.DOScale(2f, 1f).OnComplete(() => {
            transform.DORotate(new Vector3(0f, 360f * sign, 0f), 2f, RotateMode.LocalAxisAdd).OnStepComplete(() => {
                transform.DOScale(transform.localScale * 1.25f, 1f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InFlash);
            }).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutCubic);
        });
    }
    #endregion
}
