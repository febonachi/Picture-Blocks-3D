using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class UIEndLevelMenu : MonoBehaviour, IUIElement {
    #region editor
    [SerializeField] private Button nextLevelButton = default;
    #endregion

    private Animator animator;

    #region private
    private void Awake() {
        animator = GetComponent<Animator>();

        nextLevelButton.onClick.AddListener(() => GameController.instance.nextLevel());
    }
    #endregion

    #region IUIElement
    public void show() {
        if(!gameObject.activeSelf) gameObject.SetActive(true);
        animator.SetTrigger("show");
    }

    public void hide() {
        gameObject.SetActive(false);
    }

    public void reset() { 
        
    }
    #endregion
}
