using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField]
    private GameObject stageBackground;

    private Animator anim;

    private SpriteRenderer spriteRenderer;

    private DataSavingManager dataSavingManager;

    private BlockSpawner blockSpawner;

    private Stage currentStage;

    // Start is called before the first frame update
    private void Start()
    {
        anim = stageBackground.GetComponent<Animator>();
        spriteRenderer = stageBackground.GetComponent<SpriteRenderer>();
        dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        blockSpawner = GameObject.FindGameObjectWithTag("BlockSpawner").GetComponent<BlockSpawner>();

        SwitchStage((int)dataSavingManager.GetOtherValue("CurrentStage"));
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void KilledBlock()
    {
        currentStage.KilledBlock();

        dataSavingManager.SetStage(currentStage.stageKey, currentStage);
        dataSavingManager.Save();

        if (currentStage.completed)
        {
            SwitchStage(currentStage.stageKey + 1);
        }
    }

    private void SwitchStage(int stageKey)
    {
        currentStage = dataSavingManager.GetStage(stageKey);
        dataSavingManager.SetOtherValue("CurrentStage", stageKey);
        dataSavingManager.Save();
        anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(currentStage.animatorName);
        //spriteRenderer.sprite = Resources.Load<Sprite>(currentStage.backgroundSpriteName);
        blockSpawner.currentBlockSpriteArray = Resources.LoadAll<Sprite>(currentStage.blockSpritesPath);
    }
}