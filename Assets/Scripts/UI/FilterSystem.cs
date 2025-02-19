using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

[System.Serializable]
public class CategoryData
{
    public string mainCategory;
    public string[] subCategories;
    public string imageUrl;
}

[System.Serializable]
public class SubCategoryData
{
    public string name;
    public string imageUrl;
}

[System.Serializable]
public class Product
{
    public int id;
    public string name;
    public string mainCategory;
    public string subCategory;
    public float price;
    public string imageUrl;
}

[System.Serializable]
public class CategoryWrapper
{
    public CategoryData[] categories;
}

[System.Serializable]
public class SubCategoryWrapper
{
    public SubCategoryData[] subCategories;
}

[System.Serializable]
public class ProductWrapper
{
    public Product[] products;
}

public class FilterSystem : MonoBehaviour
{
    private List<Product> allProducts = new List<Product>();
    private CategoryData[] categories;
    private SubCategoryData[] subCategories;



    private string selectedMainCategory;
    private string selectedSubCategory;

    [Space(10)]
    [Header("Panels/UI")]
    [Space(5)]
    public GameObject productPrefab;
    public Transform productListContainer;

    public GameObject Product_MainCategory;
    public Transform Product_MainCategoryParent;
    public GameObject Product_SubCategory;
    public Transform Product_SubCategoryParent;



    [Header("Enable/Disable Panels")]
    public GameObject SubCategoryPanel;
    public GameObject Sub_SubCategoryPanel;

    private List<Button> MainCategoryButtons = new List<Button>();
    private List<Button> SubCategoryButtons = new List<Button>();

    public delegate void MainCategorySelected(string category, int index);

    public static event MainCategorySelected OnMainCategorySelectedEvent;

    [Header("Main Panels")]

    public GameObject ProductPopulatePanel;
    public Transform ProductPopulatePanelParent;

    // Second Panel Male/Female
    private Queue<GameObject> subCategoryPool = new Queue<GameObject>();

    void Start()
    {
        try
        {
            LoadData();
            OnMainCategorySelectedEvent += OnMainCategoryButtonClicked;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading JSON: " + e.Message);
        }

        LoadMainCategory();
    }

    private void LoadData()
    {
        categories = LoadJson<CategoryWrapper>("Categories").categories;
        subCategories = LoadJson<SubCategoryWrapper>("SubCategories").subCategories;
        allProducts = new List<Product>(LoadJson<ProductWrapper>("Products").products);
    }

    private T LoadJson<T>(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile == null)
        {
            Debug.LogError(fileName + ".json not found in Resources!");
            return default;
        }
        return JsonUtility.FromJson<T>(jsonFile.text);
    }

    private void LoadMainCategory()
    {
        for (int i = 0; i < categories.Length; i++)
        {
            var category = categories[i];
            GameObject newCategory = Instantiate(Product_MainCategory, Product_MainCategoryParent);
            newCategory.transform.GetChild(0).GetComponent<TMP_Text>().text = category.mainCategory;

            Button categoryButton = newCategory.GetComponent<Button>();
            MainCategoryButtons.Add(categoryButton);

            // Capture the index and pass it to the listener
            int index = i; // Capture the current index
            categoryButton.onClick.AddListener(() => OnMainCategorySelectedEvent?.Invoke(category.mainCategory, index));

            StartCoroutine(LoadImage(category.imageUrl, newCategory.transform.GetChild(1).GetComponent<Image>()));
        }
    }


    private void OnMainCategoryButtonClicked(string mainCategory, int index)
    {
        selectedMainCategory = mainCategory;
        Debug.Log($"Main Category {mainCategory} with index {index} selected.");
        SubCategoryPanel.SetActive(true);
        PopulateSubCategories(index);
    }

    

    private void PopulateSubCategories(int index)
    {
        // Deactivate and enqueue existing subcategories
        foreach (Transform child in Product_SubCategoryParent)
        {
            child.gameObject.SetActive(false);
            subCategoryPool.Enqueue(child.gameObject);
        }

        SubCategoryButtons.Clear();

        string[] currentSubCategories = categories[index].subCategories;

        for (int i = 0; i < currentSubCategories.Length; i++)
        {
            string subCategoryName = currentSubCategories[i];
            string subCategoryImageUrl = GetSubCategoryImageUrl(subCategoryName);

            if (subCategoryImageUrl == null)
            {
                Debug.LogWarning($"No image found for subcategory: {subCategoryName}");
                continue;
            }

            GameObject subCategoryGO = GetOrCreateSubCategoryObject();
            SetupSubCategoryObject(subCategoryGO, subCategoryName, subCategoryImageUrl);
        }
    }

    private string GetSubCategoryImageUrl(string subCategoryName)
    {
        foreach (var subCategoryData in subCategories)
        {
            if (subCategoryData.name == subCategoryName)
                return subCategoryData.imageUrl;
        }
        return null;
    }

    private GameObject GetOrCreateSubCategoryObject()
    {
        if (subCategoryPool.Count > 0)
        {
            return subCategoryPool.Dequeue();
        }
        else
        {
            return Instantiate(Product_SubCategory, Product_SubCategoryParent);
        }
    }

    private void SetupSubCategoryObject(GameObject subCategoryGO, string name, string imageUrl)
    {
        subCategoryGO.SetActive(true);
        subCategoryGO.transform.SetParent(Product_SubCategoryParent);
        subCategoryGO.transform.localScale = Vector3.one;

        // Set text
        TMP_Text textComponent = subCategoryGO.transform.GetChild(0).GetComponent<TMP_Text>();
        textComponent.text = name;

        // Configure button
        Button subCategoryButton = subCategoryGO.GetComponent<Button>();
        SubCategoryButtons.Add(subCategoryButton);
        subCategoryButton.onClick.RemoveAllListeners();
        subCategoryButton.onClick.AddListener(() => OnSubCategorySelected(name));

        // Load image
        Image imageComponent = subCategoryGO.transform.GetChild(1).GetComponent<Image>();
        StartCoroutine(LoadImage(imageUrl, imageComponent));
    }


    private void OnSubCategorySelected(string subCategory)
    {
        selectedSubCategory = subCategory;
        Sub_SubCategoryPanel.SetActive(true);
        FilterProducts();
    }
    private Queue<GameObject> productPool = new Queue<GameObject>();
    private Queue<GameObject> populatePanelPool = new Queue<GameObject>();

    private void FilterProducts()
    {
        List<Product> filtered = allProducts.FindAll(p =>
            p.mainCategory == selectedMainCategory &&
            p.subCategory == selectedSubCategory
        );

        foreach (Transform child in productListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Product p in filtered)
        {
            Debug.Log("Filter Items");
            GameObject newProduct = Instantiate(productPrefab, productListContainer);
            newProduct.transform.GetChild(0).GetComponent<TMP_Text>().text = p.name;
            StartCoroutine(LoadImage(p.imageUrl, newProduct.transform.GetChild(1).GetComponent<Image>()));
        }
    }

    public void FilterProducts_Populate()
    {
        List<Product> filtered = allProducts.FindAll(p =>
            p.mainCategory == selectedMainCategory &&
            p.subCategory == selectedSubCategory
        );

        foreach (Transform child in productListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Product p in filtered)
        {
            Debug.Log("Filter Items");
            GameObject newProduct = Instantiate(ProductPopulatePanel, ProductPopulatePanelParent);
            newProduct.transform.GetChild(1).GetComponent<TMP_Text>().text = p.name;
            newProduct.transform.GetChild(2).GetComponent<TMP_Text>().text = "$" + p.price;
            StartCoroutine(LoadImage(p.imageUrl, newProduct.transform.GetChild(0).GetComponent<Image>()));
        }
    }

    private IEnumerator LoadImage(string url, Image imageComponent)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            // Send the web request and wait for a response
            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Get the downloaded texture and create a sprite from it
                Texture2D texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
                imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogError("Failed to load image: " + webRequest.error);
            }
        }
    }
}
