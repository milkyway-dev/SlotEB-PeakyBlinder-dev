using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotController : MonoBehaviour
{


    [Header("Sprites")]
    [SerializeField] private Sprite[] iconImages;

    [Header("Slot Images")]
    [SerializeField] private List<SlotImage> slotMatrix;
    [SerializeField] internal GameObject disableIconsPanel;


    [Header("Slots Transforms")]
    [SerializeField] private RectTransform[] Slot_Transform;
    [SerializeField] private RectTransform mask_transform;
    [SerializeField] private RectTransform bg_transform;
    [SerializeField] private RectTransform[] sideBars;
    [SerializeField] private int level;


    [Header("tween properties")]
    [SerializeField] private int tweenHeight = 0;
    [SerializeField] private float initialPos;



    private List<Tweener> alltweens = new List<Tweener>();

    private Tweener WinTween = null;

    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField] private ImageAnimation[] VHObjectsBlue;
    [SerializeField] private ImageAnimation[] VHObjectsRed;

    [SerializeField] private ImageAnimation[] reel_border;
    [SerializeField] internal List<SlotIconView> animatedIcons = new List<SlotIconView>();

    [SerializeField] internal List<List<int>> freezeIndex = new List<List<int>>();

    internal IEnumerator StartSpin()
    {

        for (int i = 0; i < Slot_Transform.Length; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            // yield return new WaitForSeconds(0.1f);

        }

        yield return new WaitForSeconds(0.3f);
    }

    internal void PopulateSLotMatrix(List<List<int>> resultData)
    {

        for (int j = 0; j < slotMatrix[0].slotImages.Count; j++)
        {
            for (int i = 0; i < slotMatrix.Count; i++)
            {

                slotMatrix[i].slotImages[j].iconImage.sprite = iconImages[resultData[j][i]];
            }
        }

    }
    internal IEnumerator StopSpin()
    {

        for (int i = 0; i < Slot_Transform.Length; i++)
        {
            StopTweening(Slot_Transform[i], i);

            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.5f);

        KillAllTweens();

    }

    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < slotMatrix.Count; i++)
        {
            for (int j = 0; j < slotMatrix[i].slotImages.Count; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, iconImages.Length);
                slotMatrix[i].slotImages[j].iconImage.sprite = iconImages[randomIndex];
                slotMatrix[i].slotImages[j].pos = (i * 10 + j);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResizeSlotMatrix(true);
        }
        if (Input.GetMouseButtonDown(1))
        {
            ResizeSlotMatrix(false);
        }
    }

    internal void ResizeSlotMatrix(bool inc)
    {
        if (inc)
            level++;
        else
            level = 0;
        Vector2 sizeDelta = mask_transform.sizeDelta;

        float iconHeight = sizeDelta.y / (3 + level);
        float iconWidth = iconHeight*1.25f;

        sizeDelta.x = 5 * iconWidth;
        float reelHeight = 15 * iconHeight;
        initialPos = -(iconHeight * (3 + (level - 1) * 0.5f));

        mask_transform.DOSizeDelta(sizeDelta, 1f);


        if (level == 0)
        {
            sideBars[0].DOLocalMoveX(iconWidth * 2 + 50, 1f);
            sideBars[1].DOLocalMoveX(-iconWidth * 2 - 50, 1f);
        }
        else
        {
            sideBars[0].DOLocalMoveX(iconWidth * 2 -  (level - 1) * 15, 1f);
            sideBars[1].DOLocalMoveX(-iconWidth * 2 + (level - 1) * 15, 1f);
        }

        for (int i = 0; i < Slot_Transform.Length; i++)
        {
            int index = i;
            Slot_Transform[index].DOSizeDelta(new Vector2(iconWidth, reelHeight), 1f).OnUpdate(() =>
            {

                LayoutRebuilder.ForceRebuildLayoutImmediate(Slot_Transform[index]);

            });
        }
        for (int i = 0; i < Slot_Transform.Length; i++)
        {
            Vector2 finalPos = new Vector2((i - Slot_Transform.Length / 2) * iconWidth, initialPos);
            Slot_Transform[i].DOLocalMove(finalPos, 1);
        }

    }

    internal void StartIconBlastAnimation(List<string> iconPos, bool opposite = false)
    {
        // IconController tempIcon; 
        // for (int j = 0; j < iconPos.Count; j++)
        // {
        //     SlotIconView tempIcon;
        //     int[] pos = iconPos[j].Split(',').Select(int.Parse).ToArray();
        //     if (opposite)
        //         tempIcon = slotMatrix[pos[1]].slotImages[pos[0]];
        //     else
        //         tempIcon = slotMatrix[pos[0]].slotImages[pos[1]];

        //     tempIcon.blastAnim.SetActive(true);
        //     tempIcon.blastAnim.transform.DOScale(new Vector2(1.1f, 1.1f), 0.35f).SetEase(Ease.OutBack).OnComplete(() =>
        //     {
        //         tempIcon.blastAnim.SetActive(false);
        //         tempIcon.frontBorder.SetActive(true);
        //     });
        //     tempIcon.transform.SetParent(disableIconsPanel.transform.parent);
        //     if (!animatedIcons.Any(icon => icon.pos == tempIcon.pos))
        //         animatedIcons.Add(tempIcon);
        // }

    }


    internal void FreeSpinVHAnim(List<string> pos, ref List<ImageAnimation> VHcombo)
    {
        for (int i = 0; i < pos.Count; i++)
        {
            if (i % 2 != 0) continue;

            int[] iconPos = pos[i].Split(',').Select(int.Parse).ToArray();
            if (iconPos[1] == 1)
            {
                VHObjectsRed[iconPos[0]].gameObject.SetActive(true);
                VHObjectsRed[iconPos[0]].StartAnimation();
                VHObjectsRed[iconPos[0]].transform.DOPunchScale(new Vector3(0.4f, 0.4f, 0), 0.3f, 0, 1.2f);
                VHcombo.Add(VHObjectsRed[iconPos[0]]);
            }
            else if (iconPos[1] == 3)
            {
                VHObjectsBlue[iconPos[0]].gameObject.SetActive(true);
                VHObjectsBlue[iconPos[0]].StartAnimation();
                VHObjectsBlue[iconPos[0]].transform.DOPunchScale(new Vector3(0.4f, 0.4f, 0), 0.3f, 0, 1.2f);
                VHcombo.Add(VHObjectsBlue[iconPos[0]]);

            }

        }

    }

    internal void IconShakeAnim(List<string> vhPos)
    {
        int[] pos;
        for (int i = 0; i < vhPos.Count; i++)
        {
            pos = vhPos[i].Split(',').Select(int.Parse).ToArray();
            slotMatrix[pos[1]].slotImages[pos[0]].transform.DOShakePosition(1f, strength: new Vector3(25, 25, 0), vibrato: 20, randomness: 90, fadeOut: true);

        }
    }



    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        // slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.15f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        alltweens.Add(tweener);
        // tweener.Play();
    }

    private void StopTweening(Transform slotTransform, int index)
    {
        alltweens[index].Pause();
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, initialPos + 265);
        alltweens[index] = slotTransform.DOLocalMoveY(initialPos, 0.5f).SetEase(Ease.OutElastic); // slot initial pos - iconsizefactor - spacing

    }


    private void KillAllTweens()
    {
        for (int i = 0; i < alltweens.Count; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<SlotIconView> slotImages = new List<SlotIconView>(10);
}


