using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMaker : MonoBehaviour
{
    //the prefab that'll be in each cell of the grid
    //MAKE: game object w/ sprite renderer
    public GameObject tilePrefab;

    //object that holds the grid cells
    //MAKE: empty game object
    public GameObject gridContainer;

    //size of the grid
    public int width;
    public int height;

    //space between tiles
    public float yOffset;
    public float xOffset;

    //colors of each cell type
    [SerializeField]
    Color grassColor, mountainColor, sandColor, waterColor, forestColor;

    //array that'll hold all our cell
    GameObject[,] tilesInGrid;
    
    int seed;

    // Start is called before the first frame update
    void Start()
    {
        //get a random number to base our map on
        seed = Random.Range(0, 100000);

        //set up the grid's array
        tilesInGrid = new GameObject[width, height];
        //loop through the width/height and make a cell in each spot
        for(int i = 0; i < width; i++){
            for(int k = 0; k < height; k++){
                MakeTile(i, k);
            }
        }
    }

    //creates one cell
    void MakeTile(int i, int k){
        //make a new tile game object
        GameObject newTile = Instantiate(tilePrefab, gridContainer.transform.position, gridContainer.transform.rotation);

        //set the new tile's parent to the container game object
        newTile.transform.SetParent(gridContainer.transform);

        //use perlin noise to smoothly generate a number between 0 and 1
        //and base the color of our tile on that number
        float noise = Mathf.PerlinNoise((float)(seed + i)/10, (float)(k+seed)/10);
        if(noise < 0.3f){
            newTile.GetComponent<SpriteRenderer>().color = grassColor;
        } else if(noise >= 0.3f && noise < 0.5f){
            newTile.GetComponent<SpriteRenderer>().color = forestColor;
        } else if(noise >= 0.5f && noise < 0.7f){
            newTile.GetComponent<SpriteRenderer>().color = waterColor;
        } else if(noise >= 0.7f && noise < 0.8f){
            newTile.GetComponent<SpriteRenderer>().color = mountainColor;
        } else if(noise >= 0.8f){
            newTile.GetComponent<SpriteRenderer>().color = sandColor;
        }

        //set the tile's potiion
        newTile.transform.position = new Vector3((i + k * 0.5f - k/2) * xOffset, (k * yOffset)/2);

        //add the tile to our array
        tilesInGrid[i,k] = newTile;
    }




}
