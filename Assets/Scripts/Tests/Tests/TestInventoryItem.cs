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
        // Create a new Inventory instance on a GameObject
        GameObject inventoryGo = new GameObject("Inventory");
        inventory = inventoryGo.AddComponent<Inventory>();

        // Create a test item GameObject and add the TestInventoryItem component
        testItemObject = new GameObject("TestItem");
        testItem = testItemObject.AddComponent<TestInventoryItem>();

        // Add a BoxCollider2D to simulate the required collider.
        testCollider = testItemObject.AddComponent<BoxCollider2D>();
        testCollider.enabled = true;
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up objects after each test.
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
}
