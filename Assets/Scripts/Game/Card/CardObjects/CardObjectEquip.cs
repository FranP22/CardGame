using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CardObjectEquip : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<EquipCard> card = new NetworkVariable<EquipCard>();
    [HideInInspector]
    public NetworkVariable<NetworkObjectReference> player = new NetworkVariable<NetworkObjectReference>();

    [SerializeField]
    private GameObject image;
    private RawImage imageComponent;

    void Start()
    {
        imageComponent = image.GetComponent<RawImage>();
    }

    public void CreateCard(int id)
    {
        EquipCard c = CardDatabaseManager.instance.GetEquipCard(id);
        card.Value = c.EquipClone();

        //SetImage(card.Value.cardInfo.name);
    }

    private void SetImage(string name, string folder = "")
    {
        string path;
        if (folder == "")
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


        Texture2D tex = new Texture2D(2, 2);
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

    public void SetPlayerRef(NetworkObjectReference player)
    {
        this.player.Value = player;
    }

    public void StartEffectTrigger(EffectTrigger trigger, NetworkObject source = null)
    {
        AdditionalEffectTriggers(trigger);

        List<CardEffect> effects = card.Value.effects;
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].trigger == trigger)
            {
                CardUtilities.TriggerEffect(gameObject, effects[i]);
            }
        }
    }

    private void AdditionalEffectTriggers(EffectTrigger trigger)
    {
        NetworkObject champion = ((NetworkObject)player.Value).GetComponent<PlayerController>().champion.Value;
        if (trigger == EffectTrigger.OnEquip)
        {
            champion.GetComponent<CardObjectField>().Equip(gameObject);
        }

        if (trigger == EffectTrigger.OnUnequip)
        {
            champion.GetComponent<CardObjectField>().Unequip(gameObject);
        }
    }
}
