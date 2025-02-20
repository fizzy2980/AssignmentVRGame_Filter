using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

[System.Serializable]
public struct ProductPrefabComponents
{
    public TMP_Text nameText;
    public Image image;
}

[System.Serializable]
public struct CategoryPrefabComponents
{
    public TMP_Text categoryText;
    public Image categoryImage;
    public Button categoryButton;
}

[System.Serializable]
public struct SubCategoryPrefabComponents
{
    public TMP_Text subCategoryText;
    public Image subCategoryImage;
    public Button subCategoryButton;
}

[System.Serializable]
public class InitClassProduct
{
    public string name;
    public GameObject prefab;

    public Transform parent;
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


    [Header("IInitial Panel")]
    public List<InitClassProduct> initClassProducts;

    public List<GameObject> PanelList;
    public GameObject SecondPage;
    public GameObject FirstPage;


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
        for (int i = 0; i < initClassProducts.Count; i++)
        {
            InitializeProduct(initClassProducts[i].name, initClassProducts[i].prefab, initClassProducts[i].parent);
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

            CategoryPrefabComponents categoryComponents = new CategoryPrefabComponents
            {
                categoryText = newCategory.transform.GetChild(0).GetComponent<TMP_Text>(),
                categoryImage = newCategory.transform.GetChild(1).GetComponent<Image>(),
                categoryButton = newCategory.GetComponent<Button>()
            };

            categoryComponents.categoryText.text = category.mainCategory;

            int index = i;
            categoryComponents.categoryButton.onClick.AddListener(() => OnMainCategorySelectedEvent?.Invoke(category.mainCategory, index));

            StartCoroutine(LoadImage(category.imageUrl, categoryComponents.categoryImage));

            MainCategoryButtons.Add(categoryComponents.categoryButton);
            Debug.Log($"[CACHE] Added Main Category Button: {category.mainCategory} (Total: {MainCategoryButtons.Count})");
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
        Debug.Log("[CACHE] Clearing old subcategories...");

        foreach (Transform child in Product_SubCategoryParent)
        {
            child.gameObject.SetActive(false);
            subCategoryPool.Enqueue(child.gameObject);
        }

        Debug.Log($"[CACHE] SubCategory Pool Size after Enqueue: {subCategoryPool.Count}");

        SubCategoryButtons.Clear();
        string[] currentSubCategories = categories[index].subCategories;

        for (int i = 0; i < currentSubCategories.Length; i++)
        {
            string subCategoryName = currentSubCategories[i];
            string subCategoryImageUrl = GetSubCategoryImageUrl(subCategoryName);

            if (subCategoryImageUrl == null)
            {
                Debug.LogWarning($"[CACHE] No image found for subcategory: {subCategoryName}");
                continue;
            }

            GameObject subCategoryGO = GetOrCreateSubCategoryObject();
            SetupSubCategoryObject(subCategoryGO, subCategoryName, subCategoryImageUrl);
            Debug.Log($"[CACHE] Added SubCategory: {subCategoryName} (Total: {SubCategoryButtons.Count})");
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
            Debug.Log("[CACHE] Reusing an existing subcategory object from pool.");
            return subCategoryPool.Dequeue();
        }
        else
        {
            Debug.Log("[CACHE] Instantiating a new subcategory object.");
            return Instantiate(Product_SubCategory, Product_SubCategoryParent);
        }
    }

    private void SetupSubCategoryObject(GameObject subCategoryGO, string name, string imageUrl)
    {
        subCategoryGO.SetActive(true);
        subCategoryGO.transform.SetParent(Product_SubCategoryParent);
        subCategoryGO.transform.localScale = Vector3.one;

        // Cache components
        SubCategoryPrefabComponents subCategoryComponents = new SubCategoryPrefabComponents
        {
            subCategoryText = subCategoryGO.transform.GetChild(0).GetComponent<TMP_Text>(),
            subCategoryImage = subCategoryGO.transform.GetChild(1).GetComponent<Image>(),
            subCategoryButton = subCategoryGO.GetComponent<Button>()
        };

        // Set text
        subCategoryComponents.subCategoryText.text = name;

        // Configure button
        subCategoryComponents.subCategoryButton.onClick.RemoveAllListeners();
        subCategoryComponents.subCategoryButton.onClick.AddListener(() => OnSubCategorySelected(name));

        // Load image
        StartCoroutine(LoadImage(imageUrl, subCategoryComponents.subCategoryImage));

        // Store the button for later use
        SubCategoryButtons.Add(subCategoryComponents.subCategoryButton);
    }


    private void OnSubCategorySelected(string subCategory)
    {
        selectedSubCategory = subCategory;
        Sub_SubCategoryPanel.SetActive(true);
        FilterProducts();
    }

    private void FilterProducts()
    {
        List<Product> filtered = allProducts.FindAll(p =>
            p.mainCategory == selectedMainCategory &&
            p.subCategory == selectedSubCategory
        );

        Debug.Log($"[FILTER] Found {filtered.Count} products for category: {selectedMainCategory}, sub-category: {selectedSubCategory}");

        foreach (Transform child in productListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Product p in filtered)
        {
            GameObject newProduct = Instantiate(productPrefab, productListContainer);

            ProductPrefabComponents productComponents = new ProductPrefabComponents
            {
                nameText = newProduct.transform.GetChild(0).GetComponent<TMP_Text>(),
                image = newProduct.transform.GetChild(1).GetComponent<Image>()
            };

            productComponents.nameText.text = p.name;

            StartCoroutine(LoadImage(p.imageUrl, productComponents.image));
        }
    }

    private void InitializeProduct(string categoryName, GameObject prefab, Transform parent)
    {
        // Filter products to ONLY include "Shoes" category
        List<Product> filtered = allProducts.FindAll(p =>
            p.mainCategory == categoryName // Only include Shoes category
        );

        Debug.Log($"[FILTER] Found {filtered.Count} products in category: Shoes");
        // Instantiate new filtered product prefabs
        foreach (Product p in filtered)
        {
            GameObject newProduct = Instantiate(prefab, parent);

            // Get UI components from prefab
            TMP_Text nameText = newProduct.transform.GetChild(1).GetComponent<TMP_Text>();
            TMP_Text priceText = newProduct.transform.GetChild(2).GetComponent<TMP_Text>();
            Image productImage = newProduct.transform.GetChild(0).GetComponent<Image>();

            // Assign values to UI elements
            nameText.text = p.name;
            priceText.text = $"${p.price:F2}"; // Formats price to 2 decimal places

            // Load product image
            StartCoroutine(LoadImage(p.imageUrl, productImage));
        }
    }
    public void FilterProducts_Populate()
    {
        Debug.Log("Active : " + AreAllPanelsActive());
        if (!AreAllPanelsActive())
        {
            return;
        }
        FirstPage.SetActive(false);
        SecondPage.SetActive(true);
        List<Product> filtered = allProducts.FindAll(p =>
            p.mainCategory == selectedMainCategory &&
            p.subCategory == selectedSubCategory
        );

        // Clear existing products
        foreach (Transform child in ProductPopulatePanelParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Product p in filtered)
        {
            GameObject newProduct = Instantiate(ProductPopulatePanel, ProductPopulatePanelParent);

            // Cache components
            ProductPrefabComponents productComponents = new ProductPrefabComponents
            {
                nameText = newProduct.transform.GetChild(1).GetComponent<TMP_Text>(),
                image = newProduct.transform.GetChild(0).GetComponent<Image>()
            };

            // Set text and price
            productComponents.nameText.text = p.name;
            newProduct.transform.GetChild(2).GetComponent<TMP_Text>().text = "$" + p.price;

            // Load image
            StartCoroutine(LoadImage(p.imageUrl, productComponents.image));
        }
    }

    public bool AreAllPanelsActive()
    {
        foreach (GameObject panel in PanelList)
        {
            if (panel == null || !panel.activeInHierarchy)
            {
                return false;
            }
        }
        return true;
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
