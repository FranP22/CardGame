using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Netcode;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardObjectUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Card card = null;
    public bool debugMode = false;
    public int debugId = 0;

    [HideInInspector]
    public int handId = -1;
    [HideInInspector]
    public NetworkObjectReference playerOwner;

    [SerializeField]
    private GameObject mana;
    [SerializeField]
    private TextMeshProUGUI manaText;
    [SerializeField]
    private GameObject attack;
    [SerializeField]
    private TextMeshProUGUI attackText;
    [SerializeField]
    private GameObject health;
    [SerializeField]
    private TextMeshProUGUI healthText;

    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private GameObject image;
    private RawImage imageComponent;

    void Start()
    {
        imageComponent = image.GetComponent<RawImage>();
        if (debugMode)
        {
            card = CardDatabaseManager.instance.GetAllyCard(debugId);
            UpdateCard();
        }
    }

    void Update()
    {
        card.cardInfo.currentMana = card.cardInfo.mana;
        if(card != null) UpdateCard(false);
    }

    public void UpdateCard(bool setInfo = true)
    {
        switch (card.cardInfo.cardType)
        {
            case CardType.Champion:
                UpdateCardChampion();
                break;
            case CardType.Equipment:
                UpdateCardEquipment();
                break;
            case CardType.Minion:
                UpdateCardMinion();
                break;
            case CardType.Summon:
                UpdateCardSummon();
                break;
            case CardType.Magic:
                UpdateCardMagic();
                break;
            default:
                return;
        }

        if (setInfo)
        {
            SetInfo(card);
        }
    }

    private void UpdateCardChampion()
    {
        attack.SetActive(true);
        health.SetActive(true);
        mana.SetActive(false);

        ChampionCard newCard = card as ChampionCard;

        SetAttack(newCard.allyStats.attack);
        SetHealth(newCard.allyStats.health);

        SetInfo(newCard);
    }
    private void UpdateCardEquipment()
    {
        attack.SetActive(false);
        health.SetActive(false);
        mana.SetActive(false);

        EquipCard newCard = card as EquipCard;

        SetInfo(newCard);
    }
    private void UpdateCardMinion()
    {
        attack.SetActive(true);
        health.SetActive(true);
        mana.SetActive(true);

        AllyCard newCard = card as AllyCard;
        SetAttack(newCard.allyStats.attack);
        SetHealth(newCard.allyStats.health);
        SetMana(newCard.cardInfo.currentMana);
    }
    private void UpdateCardSummon()
    {
        attack.SetActive(true);
        health.SetActive(true);
        mana.SetActive(false);

        AllyCard newCard = card as AllyCard;
        SetAttack(newCard.allyStats.attack);
        SetHealth(newCard.allyStats.health);
    }
    private void UpdateCardMagic()
    {
        attack.SetActive(false);
        health.SetActive(false);
        mana.SetActive(true);

        MagicCard newCard = card as MagicCard;
        SetMana(newCard.cardInfo.currentMana);
    }

    private void SetMana(int mana)
    {
        manaText.text = mana.ToString();
    }
    private void SetAttack(int attack)
    {
        attackText.text = attack.ToString();
    }
    private void SetHealth(int health)
    {
        healthText.text = health.ToString();
    }

    private void SetInfo(Card card)
    {
        SetImage(card.cardInfo.name);
        SetName(card.cardInfo.name);
    }
    private void SetImage(string name, string folder = "")
    {
        string path;
        if(folder == "")
        {
            path = "Assets/Materials/Images/" + name + ".png";
        }
        else
        {
            path = "Assets/Materials/Images/" + folder + "/" + name + ".png";
        }

        if (!File.Exists(path))
        {
            Debug.Log("Image doesn't exist");
            return;
        }

        byte[] imageData = File.ReadAllBytes(path);
        

        Texture2D tex = new Texture2D(2,2);
        if (tex.LoadImage(imageData))
        {
            imageComponent = image.GetComponent<RawImage>();
            imageComponent.texture = tex;
        }
        else
        {
            Debug.Log("Load image failed");
        }
    }
    private void SetName(string name)
    {
        nameText.text = name;
        if (mana.activeSelf)
        {
            nameText.rectTransform.offsetMin = new Vector2(25, 0); // from bottom left corner
            nameText.rectTransform.offsetMax = new Vector2(0, 0); // from top right corner
        }
        else
        {
            nameText.rectTransform.offsetMin = new Vector2(0, 0);
            nameText.rectTransform.offsetMax = new Vector2(0, 0);
        }
    }

    private Vector3 startingPos = Vector3.zero;

    public void OnBeginDrag(PointerEventData eventData)
    {
        startingPos = gameObject.transform.position;

        //Debug.Log("Drag Started");
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(transform.position.y > 200)
        {
            //Debug.Log("Play Started");
            ((NetworkObject)playerOwner).GetComponent<PlayerController>().PlayCardInit_ServerRpc(handId);
        }

        transform.position = startingPos;
        startingPos = Vector3.zero;

        //Debug.Log("Dropped");
    }


}
