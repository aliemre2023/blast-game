using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System.Linq;

public class GameInitalizer : MonoBehaviour
{
    public RectTransform grid; // grid
    public List<RectTransform> cubes; 
    public List<RectTransform> cube_particles; 
    public List<RectTransform> cube_rocket_state; 

    public List<RectTransform> obstacles; 
    public List<RectTransform> obstacle_particles; 

    public List<RectTransform> rocket; 
    public List<RectTransform> rocket_particles; 

    private LevelData levelData;
    private GameObject tempParent;

    public TextMeshProUGUI moveCountText;

    private List<int> goals = new List<int> {0, 0, 0};
    public Canvas goal_box;
    public Canvas goal_stone;
    public Canvas goal_vase;

    private List<List<string>> grid2d = new List<List<string>>();

    public RectTransform broken_block;



    void Start()
    {
        PlayerPrefs.SetInt("LevelNum", 1);
        InitializeGame();
        
    }

    public void InitializeGame()
    {
        //PlayerPrefs.SetInt("LevelNum", 1);
        int levelNum = PlayerPrefs.GetInt("LevelNum", 1); // default 1
        string level = levelNum.ToString("D2");
        string path = Application.dataPath + "/CaseStudyAssets2025/Levels/level_" + level + ".json";

        goal_box.gameObject.SetActive(false);
        goal_stone.gameObject.SetActive(false);
        goal_vase.gameObject.SetActive(false);

        JsonService jsonService = new JsonService(path);
        levelData = jsonService.LoadLevelData();  // Assign to the class field

        Set2DGrid();

        //Debug.Log(levelData);

        if (levelData != null)
        {
            //Debug.Log($"Level Loaded: {levelData.level_number}, Width: {levelData.grid_width}, Height: {levelData.grid_height}, Moves: {levelData.move_count}");
        }
        else
        {
            Debug.LogError("Failed to load level data.");
            return;
        }

        moveCountText.text = levelData.move_count.ToString();

        GridResizer gridResizer = new GridResizer(grid, levelData.grid_width, levelData.grid_height);
        gridResizer.ResizeGrid();

        SetGoalsInitial(goals, grid2d);

        HashSet<(int, int)> sliding_blocks = new HashSet<(int, int)>();
        List<int> sliding_cols = Enumerable.Repeat(0, grid2d.Count).ToList();
        /*
        for(int i = 0; i < grid2d.Count; i++){
            for(int j = 0; j < grid2d[0].Count; j++){
                sliding_blocks.Add((i, j));       
                sliding_cols[j] += 1;
                
            }
        }
        */
        BlockHandler(levelData, grid2d, sliding_blocks, sliding_cols);
        //BlcokHandler(levelData, grid2d);
    }

    public void BlockHandler(LevelData levelData, List<List<string>> grid2d, HashSet<(int, int)> sliding_blocks, List<int> sliding_cols)
    {
        //GameGridHandler gameGridHandler = new GameGridHandler();
        GameGridHandler gameGridHandler = FindObjectOfType<GameGridHandler>();

        if (gameGridHandler == null)
        {
            Debug.LogError("GameGridHandler is missing in the scene!");
            return;
        }


        // Create a new tempParent
        if (tempParent == null)
        {
            tempParent = new GameObject("TempParent");
        }
        if (tempParent != null)
        {
            Destroy(tempParent); // Destroy the entire tempParent GameObject
            tempParent = null;   
        }
        gameGridHandler.PositionBlocks(
            levelData, grid2d, grid, cubes, cube_rocket_state, obstacles, tempParent, goals, moveCountText, 
            rocket, goal_box, goal_stone, goal_vase, broken_block, cube_particles, obstacle_particles, rocket_particles,
            sliding_blocks, sliding_cols
        );
    }

    void SetGoalsInitial(List<int> goals, List<List<string>> grid2d){
        goals[0] = 0;
        goals[1] = 0;
        goals[2] = 0;


        for(int i = 0; i < grid2d.Count; i++){
            for(int j = 0; j < grid2d[0].Count; j++){
                if(grid2d[i][j] == "bo") goals[0]++;
                if(grid2d[i][j] == "s") goals[1]++;
                if(grid2d[i][j] == "v" || grid2d[i][j] == "v2") goals[2]++;
            }
        }

        if(goals[0] != 0){
            goal_box.gameObject.SetActive(true);
            TextMeshProUGUI goal_box_text = goal_box.GetComponentInChildren<TextMeshProUGUI>();
            goal_box_text.text = goals[0].ToString();
        }
        if(goals[1] != 0){
            goal_stone.gameObject.SetActive(true);
            TextMeshProUGUI goal_stone_text = goal_stone.GetComponentInChildren<TextMeshProUGUI>();
            goal_stone_text.text = goals[1].ToString();
        }
        if(goals[2] != 0){
            goal_vase.gameObject.SetActive(true);
            TextMeshProUGUI goal_vase_text = goal_vase.GetComponentInChildren<TextMeshProUGUI>();
            goal_vase_text.text = goals[2].ToString();
        }
    }

    void Set2DGrid(){
        grid2d = new List<List<string>>();

        for (int i = 0; i < levelData.grid_height; i++){
            List<string> row = new List<string>(); 
            for (int j = 0; j < levelData.grid_width; j++){
                if(levelData.grid[j+(levelData.grid_width*i)].ToString() == "rand"){
                    int random_cube_idx = Random.Range(0, 4);
                    if(random_cube_idx == 0){
                        levelData.grid[j+(levelData.grid_width*i)] = "b";
                    }
                    else if(random_cube_idx == 1){
                        levelData.grid[j+(levelData.grid_width*i)] = "g";
                    }
                    else if(random_cube_idx == 2){
                        levelData.grid[j+(levelData.grid_width*i)] = "r";
                    }
                    else if(random_cube_idx == 3){
                        levelData.grid[j+(levelData.grid_width*i)] = "y";
                    }
                }
                if (levelData.grid[j+(levelData.grid_width*i)].ToString() == "vro" ||
                    levelData.grid[j+(levelData.grid_width*i)].ToString() == "rocket_v")
                {
                    levelData.grid[j+(levelData.grid_width*i)] = "rocket_v";
                }
                else if (levelData.grid[j+(levelData.grid_width*i)].ToString() == "hro" ||
                    levelData.grid[j+(levelData.grid_width*i)].ToString() == "rocket_h")
                {
                    levelData.grid[j+(levelData.grid_width*i)] = "rocket_h";
                }
                row.Add(levelData.grid[j+(levelData.grid_width*i)]);


                if (levelData.grid[j+(levelData.grid_width*i)].ToString() == "bo")
                {
                    goals[0]++;
                }
                else if (levelData.grid[j+(levelData.grid_width*i)].ToString() == "s")
                {
                    goals[1]++;
                }
                else if (levelData.grid[j+(levelData.grid_width*i)].ToString() == "v")
                {
                    goals[2]++;
                }
            }
            grid2d.Add(row);
        }
    }
}