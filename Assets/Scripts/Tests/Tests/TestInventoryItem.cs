//Sasha Koroleva
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;


public class TestInventoryItem : MonoBehaviour, IInventoryItem
{
    public bool OnPickupCalled { get; private set; } 
    public bool OnDropCalled { get; private set; }
    public string Name => gameObject.name;
    public Sprite Image => null;

    public void OnPickup()
    {
        OnPickupCalled = true;
    }

    public void OnDrop()
    {
        OnDropCalled = true;
    }

    public void OnUse()
    {
         
    }
}

public class InventoryTests
{
    private Inventory inventory;
    private GameObject testItemObject;
    private TestInventoryItem testItem;
    private BoxCollider2D testCollider;

    [SetUp]
    public void Setup()
    {
        GameObject inventoryGo = new GameObject("Inventory");
        inventory = inventoryGo.AddComponent<Inventory>();

        testItemObject = new GameObject("TestItem");
        testItem = testItemObject.AddComponent<TestInventoryItem>();

        testCollider = testItemObject.AddComponent<BoxCollider2D>();
        testCollider.enabled = true;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(testItemObject);
        Object.DestroyImmediate(inventory.gameObject);
    }

    [Test]
    public void TestAddItem_DisablesColliderAndAddsItem()
    {
        Assert.IsTrue(testCollider.enabled, "Collider should be enabled before adding the item.");
        inventory.AddItem(testItem);
        Assert.IsFalse(testCollider.enabled, "Collider should be disabled after adding the item.");
        Assert.IsTrue(testItem.OnPickupCalled, "OnPickup should have been called on the item.");
        
    }
    
    [Test]
    public void TestRemoveItem_ReenablesColliderAndCallsOnDrop()
    {
        inventory.AddItem(testItem);
        inventory.RemoveItem(testItem);

        Assert.IsTrue(testCollider.enabled, "Collider should be re-enabled after removing the item.");
        Assert.IsTrue(testItem.OnDropCalled, "OnDrop should have been called on the item.");
    }
    
    [Test]
    public void TestAddItem_WhenInventoryFull_DoesNotAddNewItem()
    {
        List<TestInventoryItem> items = new List<TestInventoryItem>();

        for (int i = 0; i < 6; i++)
        {
            GameObject itemObj = new GameObject("TestItem" + i);
            TestInventoryItem item = itemObj.AddComponent<TestInventoryItem>();
            BoxCollider2D collider = itemObj.AddComponent<BoxCollider2D>();
            collider.enabled = true;
            inventory.AddItem(item);
            items.Add(item);
        }

        GameObject extraItemObj = new GameObject("ExtraTestItem");
        TestInventoryItem extraItem = extraItemObj.AddComponent<TestInventoryItem>();
        BoxCollider2D extraCollider = extraItemObj.AddComponent<BoxCollider2D>();
        extraCollider.enabled = true;
        inventory.AddItem(extraItem);

        Assert.IsTrue(extraCollider.enabled, "Extra item's collider should remain enabled since inventory is full.");
        Assert.IsFalse(extraItem.OnPickupCalled, "OnPickup should not be called for an item that is not added when inventory is full.");
        
        foreach (var item in items)
        {
            Object.DestroyImmediate(item.gameObject);
        }
        Object.DestroyImmediate(extraItemObj);
    }
    
    [Test]
    public void TestAddItem_NullItem_ThrowsException()
    {
        Assert.Throws<System.ArgumentNullException>(() => inventory.AddItem(null));
    }
    
    [Test]
    public void TestAddItem_ItemMissingCollider_LogsWarningAndDoesNotAddItem()
    {
        GameObject noColliderItemObject = new GameObject("NoColliderItem");
        TestInventoryItem noColliderItem = noColliderItemObject.AddComponent<TestInventoryItem>();

        LogAssert.Expect(LogType.Warning, "Collider2D is missing or already disabled for " + noColliderItem.gameObject.name);
        inventory.AddItem(noColliderItem);
        
        Assert.IsFalse(noColliderItem.OnPickupCalled, "OnPickup should not be called when item is missing a collider.");
        Object.DestroyImmediate(noColliderItemObject);
    }
}
