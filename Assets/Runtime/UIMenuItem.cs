using UnityEngine;
using UnityEngine.UI;

public class UIMenuItem : MonoBehaviour
{
    public ITriggable context = null;

    [SerializeField] private Button button = null;

    public void Init(bool value)
    {
        button.interactable = value;
    }

    private void Awake()
    {
        button.onClick.AddListener(OnButton);
    }

    private void OnButton()
    {
        context?.Trigger(GetHashCode());
    }

    private void OnDestroy()
    {
        context = null;
    }
}
