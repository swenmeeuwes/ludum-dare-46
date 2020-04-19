using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HatchEggButton : MonoBehaviour
{
    [SerializeField] private Ant.AntType _antType;
    [SerializeField] private TMP_Text _errorText;

    public Button Button { get; private set; }

    private void Start() {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(() => OnClick());

        _errorText.alpha = 0;

        if (_antType != Ant.AntType.Builder) {
            Button.interactable = false;
        }
    }

    private void OnClick() {
        var hatchableEggs = EggManager.Instance.EggsThatCanBeHatched.ToList();
        if (hatchableEggs.Count == 0) {
            _errorText.DOFade(1, .35f);
            _errorText.DOFade(0, .35f).SetDelay(3f);
        } else {
            var egg = hatchableEggs[0];
            egg.Hatch(_antType);
        }

        ActionManager.Instance.UpdateLayEggs();

        // 
        var hatchEggButtons = transform.parent.GetComponentsInChildren<HatchEggButton>();
        foreach (var item in hatchEggButtons) {
            item.Button.interactable = true;
        }
    }
}
