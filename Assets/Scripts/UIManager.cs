using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject shopPanel;

    [SerializeField]
    private Shop shop;

    // Start is called before the first frame update
    private void Start()
    {
        shopPanel.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void UpgradeSkill(Button skillButton)
    {
        string skillName = skillButton.name;
        Skill upgradedSkill;
        if (shop.UpgradeSkill(skillName, out upgradedSkill))
        {
            var description = skillButton.gameObject.transform.Find("Description").gameObject.GetComponent<Text>();
            description.text = "Price: " + upgradedSkill.upgradeCost + "\nCurrent Value: " + upgradedSkill.currentValue;
        }
    }

    public void ToggleShopPanel()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
    }
}