//Farzana Tanni

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestCabinDoor : MonoBehaviour
{
    private bool enterAllowed = false;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            enterAllowed = true;
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            enterAllowed = false;
    }

    public bool IsEnterAllowed()
    {
        return enterAllowed;
    }
}

public class DoorTests
{
    private GameObject player;
    private GameObject doorObject;
    private TestCabinDoor door;

    [SetUp]
    public void SetUp()
    {
        player = new GameObject("Player");
        player.tag = "Player";
        player.AddComponent<BoxCollider2D>();

        doorObject = new GameObject("CabinDoor");
        door = doorObject.AddComponent<TestCabinDoor>();
        doorObject.AddComponent<BoxCollider2D>().isTrigger = true;
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(player);
        Object.Destroy(doorObject);
    }

    [Test]
    public void Test_Player_Near_Door_Can_Enter()
    {
        door.OnTriggerEnter2D(player.GetComponent<Collider2D>());
        Assert.IsTrue(door.IsEnterAllowed(), "Player should be allowed to enter");
    }

    [Test]
    public void Test_Player_Leaves_Door_Area_Cannot_Enter()
    {
        door.OnTriggerEnter2D(player.GetComponent<Collider2D>());
        door.OnTriggerExit2D(player.GetComponent<Collider2D>());
        Assert.IsFalse(door.IsEnterAllowed(), "Player should NOT be allowed to enter after leaving");
    }
}
