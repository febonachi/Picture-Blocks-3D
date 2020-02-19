using UnityEngine;

public class UIGameMenu : MonoBehaviour, IUIElement {
    #region editor
    [SerializeField] private UIProgressBar uiProgressBar = default;
    #endregion

    #region public properties
    public UIProgressBar progressBar => uiProgressBar;
    #endregion

    #region IUIElement
    public void show() {
        if(!gameObject.activeSelf) gameObject.SetActive(true);
        uiProgressBar.show();
    }

    public void hide() {
        uiProgressBar.hide();
    }

    public void reset() {
        uiProgressBar.reset();
    }
    #endregion
}
