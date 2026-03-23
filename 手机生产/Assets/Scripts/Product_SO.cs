using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Product_SO", menuName = "Product/product_SO")]
public class Product_SO : ScriptableObject
{
    public List<Product> product = new List<Product>();
    public int currentProductIndex = 0;

    //获取当前生产信息
    public Product GetCurrentProduct()
    {
        if (product.Count > currentProductIndex)
        {
            return product[currentProductIndex];
        }
        return null;
    }

    //获取指定年份的生产
    public Product GetProductForYear(int year)
    {
        return product.Find(p => p.year == year);
    }

    // 添加生产
    public void AddProduct(Product products)
    {
        product.Add(products);
    }

    // 更新生产
    public void UpdateProduct(int year, Product newProduct)
    {
        int index = product.FindIndex(p => p.year == year);
        if (index >= 0)
        {
            product[index] = newProduct;
        }
        else
        {
            product.Add(newProduct);
        }
    }
}