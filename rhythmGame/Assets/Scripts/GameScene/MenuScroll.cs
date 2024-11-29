using UnityEngine;
using System.Collections;

public class MenuScroll : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private RhythmGameManager gameManager;

    public GameObject[] menuItems;
    public Transform uiPivot;
    public float verticalSpacing = 2.0f;

    private void Start()
    {
        InitializeMenuItems();       
    }

    private void InitializeMenuItems()
    {
        int itemCount = gameManager.availableTracks.Count;
        menuItems = new GameObject[itemCount];

        for (int i = 0; i < itemCount; i++)
        {
            Vector3 position = new Vector3(0.25f, -i * verticalSpacing, 0.0f);
            menuItems[i] = Instantiate(itemPrefab, uiPivot);
            menuItems[i].transform.localPosition = position;
        }
      
    }

}