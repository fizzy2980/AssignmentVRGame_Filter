using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
