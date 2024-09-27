using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedDeck
{
    private string deckName;
    private Deck deck;
    private bool selected = false;

    public void SaveDeck()
    {

    }

    public void UnSelect()
    {
        selected = false;
    }
    public void Select()
    {
        selected = true;
    }
}
