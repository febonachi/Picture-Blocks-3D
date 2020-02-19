using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class UIProgressBar : MonoBehaviour, IUIElement {
    #region editor
    [SerializeField] private Image starImage = default;
    [SerializeField] private TextMeshProUGUI percentText = default;
    #endregion

    private Animator animator;

    #region private
    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private IEnumerator _setProgress(float progress, bool showStar) {
        int percent = 0;
        while (percent < progress) {
            percent++;
            percentText.text = $"{percent}%";
            yield return null;
        }
        if(showStar) starImage.gameObject.SetActive(true);
    }
    #endregion

    #region public
    public void setProgress(float progress, bool showStar) {
        StartCoroutine(_setProgress(progress, showStar));
    }
    #endregion

    #region IUIElement
    public void show() {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        starImage.gameObject.SetActive(false);
        animator.SetTrigger("show");
    }

    public void hide() {
        animator.SetTrigger("hide");
    }

    public void reset() {
        percentText.text = "0%";
        starImage.gameObject.SetActive(false);
    }
    #endregion
}
