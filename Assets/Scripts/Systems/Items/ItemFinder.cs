using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemFinder : MonoBehaviour
{
    public static ItemFinder instance;
    [SerializeField] private Dictionary<int, ItemSO> _items;

    private void Awake()
    {
        instance = this;
        _items = new Dictionary<int, ItemSO>();
    }

    void Start()
    {
        CreateRegistryDictionary();
    }

    private void CreateRegistryDictionary()
    {
        var items = Resources.LoadAll<ItemSO>("ItemCatagories");
        
        foreach (var item in items)
        {
            if (_items.ContainsKey(item.ID))
            {
                Debug.LogError(
                    "Multiple item IDs[" + item.ID + "] found: Error registering [" 
                    + item.name + "] when [" + IDToItem(item.ID).name + "] ID is already in use");
            }
            
            _items.Add(item.ID, item);
        }
    }

    public ItemSO IDToItem(int id)
    {
        if (_items.TryGetValue(id, out ItemSO newItemSO))
        {
            return newItemSO;
        }
    
        Debug.LogError($"ID {id} was not found");
        return null;
    }

}

