using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Collections;

public class GameGridHandler : MonoBehaviour
{

    bool IsGameFinish(bool any_obstacle, string any_move)
    {
        GameFinishHandler gameFinishHandler = FindObjectOfType<GameFinishHandler>();

        if (gameFinishHandler == null)
        {
            Debug.LogError("GameFinishHandler is not assigned!");
            return false;
        }

        if (!any_obstacle)
        {
            // Win screen
            gameFinishHandler.ShowWinPopup();
            return true;
        }
        else if (any_move == "0")
        {
            // Lose screen
            gameFinishHandler.ShowLosePopup();
            return true;
        }
        return false;
    }

    bool ArrangeGoals(List<List<string>> grid2d, Canvas goal_box, Canvas goal_stone, Canvas goal_vase){
        List<int> goals = new List<int> {0, 0, 0};

        for(int i = 0; i < grid2d.Count; i++){
            for(int j = 0; j < grid2d[0].Count; j++){
                if(grid2d[i][j] == "bo") goals[0]++;
                if(grid2d[i][j] == "s") goals[1]++;
                if(grid2d[i][j] == "v" || grid2d[i][j] == "v2") goals[2]++;
            }
        }
        
        TextMeshProUGUI goal_box_text = goal_box.GetComponentInChildren<TextMeshProUGUI>();
        goal_box_text.text = goals[0].ToString();
        
        TextMeshProUGUI goal_stone_text = goal_stone.GetComponentInChildren<TextMeshProUGUI>();
        goal_stone_text.text = goals[1].ToString();
        
        TextMeshProUGUI goal_vase_text = goal_vase.GetComponentInChildren<TextMeshProUGUI>();
        goal_vase_text.text = goals[2].ToString();

        int remaining_block = goals[0] + goals[1] + goals[2];
        if(remaining_block == 0) return false;
        else return true;
    }

    void DearrangeRocketBlocks(List<List<string>> grid2d){
        int rows = grid2d.Count;
        int cols = grid2d[0].Count;

        for(int i = 0; i < rows; i++)
        {
            for(int j = 0; j < cols; j++)
            {
                if(grid2d[i][j].EndsWith("_rocket")){
                    grid2d[i][j] = grid2d[i][j][0].ToString();
                }
            }
        }
    }

    void ArrangeRocketBlocks(List<List<string>> grid2d)
    {
        int rows = grid2d.Count;
        int cols = grid2d[0].Count;
        HashSet<(int, int)> visited = new HashSet<(int, int)>();
        
        // Directions: Left, Right, Down, Up
        int[] dx = { 0, 0, 1, -1 }; 
        int[] dy = { 1, -1, 0, 0 };

        string[] colors = { "b", "g", "r", "y", "b_rocket", "g_rocket", "r_rocket", "y_rocket" };

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                string color = grid2d[i][j];
                if (!colors.Contains(color) || visited.Contains((i, j)))
                    continue;

                List<(int, int)> cluster = new List<(int, int)>();
                Queue<(int, int)> queue = new Queue<(int, int)>();
                queue.Enqueue((i, j));
                visited.Add((i, j));

                while (queue.Count > 0)
                {
                    var (x, y) = queue.Dequeue();
                    cluster.Add((x, y));

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];

                        if (nx >= 0 && nx < rows && ny >= 0 && ny < cols && 
                            (grid2d[nx][ny] == color || 
                            grid2d[nx][ny] == color + "_rocket") && 
                            !visited.Contains((nx, ny)))
                        {
                            queue.Enqueue((nx, ny));
                            visited.Add((nx, ny));
                        }
                    }
                }

                // If we have more than 3 same-colored blocks, convert them to rocket state
                if (cluster.Count > 3)
                {
                    string rocketColor = color;
                    if (!color.Contains("rocket")){
                        rocketColor = color + "_rocket";
                    }
                    
                    foreach (var (x, y) in cluster)
                    {
                        grid2d[x][y] = rocketColor;
                    }
                }
            }
        }

        /*
        for (int i = 0; i < rows; i++)
        {
            string rowText = ""; // Store row elements as a single string
            for (int j = 0; j < cols; j++)
            {
                rowText += grid2d[i][j] + " ";
            }
        }
        */

    }

    public void PositionBlocks(
        LevelData levelData, List<List<string>> grid2d, RectTransform grid, 
        List<RectTransform> cubes, List<RectTransform> cube_rocket_state, 
        List<RectTransform> obstacles, GameObject tempParent, List<int> goals, 
        TextMeshProUGUI moveCountText, List<RectTransform> rocket,
        Canvas goal_box, Canvas goal_stone, Canvas goal_vase, RectTransform broken_block,
        List<RectTransform> cube_particles, List<RectTransform> obstacle_particles, List<RectTransform> rocket_particles,
        HashSet<(int, int)> sliding_blocks, List<int> sliding_cols
    )
    {
        
        
        if (cubes.Count != 4 || obstacles.Count != 4 || levelData == null || grid2d == null || grid2d.Count == 0 || grid2d[0].Count == 0)
        {
            Debug.LogError("Null reference: grid, cubes, or levelData is not assigned.");
            return;
        }

        
        // Handle table
        if (tempParent == null)
        {
            tempParent = GameObject.Find("current_table");

            if (tempParent == null)
            {
                tempParent = new GameObject("current_table");
                tempParent.transform.SetParent(grid, false);
            }
            else if(tempParent != null)
            {
                foreach (Transform child in tempParent.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        // Boundaries of the grid background
        float minX = grid.rect.xMin;
        float maxX = grid.rect.xMax;
        float minY = grid.rect.yMin;
        float maxY = grid.rect.yMax;

        bool broken_grid = false;

        for(int i = 0; i < levelData.grid_height; i++){
            for(int j = 0; j < levelData.grid_width; j++){
                if (grid2d[i][j].ToString() == "rand"){
                    int random_cube_idx = Random.Range(0, cubes.Count);
                    if(random_cube_idx == 0) grid2d[i][j] = "b";
                    else if(random_cube_idx == 1) grid2d[i][j] = "g";
                    else if(random_cube_idx == 2) grid2d[i][j] = "r";
                    else if(random_cube_idx == 3) grid2d[i][j] = "y";
                }
                if(grid2d[i][j].ToString().Contains("_broken")) broken_grid = true;
            }
        }
        
        if(broken_grid){
            sliding_blocks = new HashSet<(int, int)>();;
            sliding_cols = Enumerable.Repeat(0, grid2d.Count).ToList();
        }
        
    
        
        DearrangeRocketBlocks(grid2d);
        ArrangeRocketBlocks(grid2d);
        
        for(int i = 0; i < levelData.grid_height; i++)
        {
            for(int j = 0; j < levelData.grid_width; j++)
            {
                float x = 12.5f + 20f*j; // width
                float y = 12.5f + 20f*i; // height

                RectTransform block = null;


                // Assign block based on the grid value
                if (grid2d[i][j].ToString() == "rand")
                {
                    int random_cube_idx = Random.Range(0, cubes.Count);
                    block = cubes[random_cube_idx];
                    if(random_cube_idx == 0) grid2d[i][j] = "b";
                    else if(random_cube_idx == 1) grid2d[i][j] = "g";
                    else if(random_cube_idx == 2) grid2d[i][j] = "r";
                    else if(random_cube_idx == 3) grid2d[i][j] = "y";
                }
                else if (grid2d[i][j].ToString().EndsWith("_broken"))
                {
                    Debug.Log("====****---Im in _broken---****====");

                    RectTransform particle = null;

                    if (grid2d[i][j].ToString() == "bo_broken") particle = obstacle_particles[0];
                    else if (grid2d[i][j].ToString() == "s_broken") particle = obstacle_particles[1];
                    else if (grid2d[i][j].ToString() == "v2_broken") particle = obstacle_particles[2];
                    else if (grid2d[i][j].ToString() == "rocket_v_broken" || grid2d[i][j].ToString() == "rocket_h_broken") particle = rocket_particles[0];
                    else if (grid2d[i][j].ToString() == "b_broken" || grid2d[i][j].ToString() == "b_rocket_broken") particle = cube_particles[0];
                    else if (grid2d[i][j].ToString() == "g_broken" || grid2d[i][j].ToString() == "g_rocket_broken") particle = cube_particles[1];
                    else if (grid2d[i][j].ToString() == "r_broken" || grid2d[i][j].ToString() == "r_rocket_broken") particle = cube_particles[2];
                    else if (grid2d[i][j].ToString() == "y_broken" || grid2d[i][j].ToString() == "y_rocket_broken") particle = cube_particles[3];
                    else particle = cube_particles[0];

                    /*
                    find the Rectransform broken_schema, make all the child images source to the particle 
                    */
                    Image particleImage = particle.GetComponent<Image>();
                    if (particleImage == null)
                    {
                        Debug.LogError("Particle does not have an Image component!");
                    }
                    else
                    {
                        Debug.Log($"Particle Image found. Sprite: {particleImage.sprite.name}, Color: {particleImage.color}");
                    }

                    foreach (Transform child in broken_block)
                    {
                        Image childImage = child.GetComponent<Image>();
                        if (childImage != null)
                        {
                            Debug.Log($"Updating child: {child.name} with particle sprite: {particleImage.sprite.name}");
                            childImage.sprite = particleImage.sprite;
                            childImage.color = particleImage.color;
                        }
                        else
                        {
                            Debug.LogWarning($"Child: {child.name} does not have an Image component!");
                        }
                    }


                    RectTransform copy_block = Instantiate(broken_block, tempParent.transform);
                    copy_block.anchoredPosition = new Vector2(minX + x, minY + y);
                    copy_block.name = $"{i},{j}";
                }
                else if (grid2d[i][j].ToString() == "b") block = cubes[0];
                else if (grid2d[i][j].ToString() == "b_rocket") block = cube_rocket_state[0];
                else if (grid2d[i][j].ToString() == "g") block = cubes[1];
                else if (grid2d[i][j].ToString() == "g_rocket") block = cube_rocket_state[1];
                else if (grid2d[i][j].ToString() == "r") block = cubes[2];
                else if (grid2d[i][j].ToString() == "r_rocket") block = cube_rocket_state[2];
                else if (grid2d[i][j].ToString() == "y") block = cubes[3];
                else if (grid2d[i][j].ToString() == "y_rocket") block = cube_rocket_state[3];
                else if (grid2d[i][j].ToString() == "bo") block = obstacles[0];
                else if (grid2d[i][j].ToString() == "s") block = obstacles[1];
                else if (grid2d[i][j].ToString() == "v") block = obstacles[2];
                else if (grid2d[i][j].ToString() == "v2") block = obstacles[3];
                else if (grid2d[i][j].ToString() == "rocket_h") block = rocket[0];
                else if (grid2d[i][j].ToString() == "rocket_v") block = rocket[3];


                // Only instantiate if a valid block is assigned
                if (block != null && !grid2d[i][j].ToString().EndsWith("_broken"))
                {
                    RectTransform copy_block = Instantiate(block, tempParent.transform);
                    copy_block.anchoredPosition = new Vector2(minX + x, minY + y);
                    
                    if(sliding_blocks != null && sliding_cols != null && sliding_blocks.Contains((i, j)))
                    {
                        Vector2 startPosition = new Vector2(minX + x, minY + y + sliding_cols[j]*20);
                        Vector2 endPosition = new Vector2(minX + x, minY + y);
                        StartCoroutine(SmoothSlide(copy_block, startPosition, endPosition, sliding_cols[j]*0.2f));
                    }
                    else
                    {
                        copy_block.anchoredPosition = new Vector2(minX + x, minY + y);
                    }
                    
                    copy_block.name = $"{i},{j}";

                    // listener
                    Button instantiatedButton = copy_block.GetComponent<Button>();
                    if (instantiatedButton != null && !broken_grid)
                    {
                        instantiatedButton.onClick.AddListener(() => OnButtonClicked(
                            copy_block.name, levelData, grid2d, grid, cubes, cube_rocket_state, obstacles, tempParent, goals, 
                            moveCountText, rocket, goal_box, goal_stone, goal_vase, broken_block,
                            cube_particles, obstacle_particles, rocket_particles
                        ));
                    }
                    else
                    {
                        Debug.LogWarning($"Instantiated block at {copy_block.name} does not have a Button component!");
                    }
                }

            }
        }
    }
    
    private IEnumerator SmoothSlide(RectTransform block, Vector2 startPosition, Vector2 endPosition, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            block.anchoredPosition = Vector2.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        block.anchoredPosition = endPosition;
    }
    

    void OnButtonClicked(string buttonName, LevelData levelData, List<List<string>> grid2d, RectTransform grid, 
        List<RectTransform> cubes, List<RectTransform> cube_rocket_state, 
        List<RectTransform> obstacles, GameObject tempParent, List<int> goals, 
        TextMeshProUGUI moveCountText, List<RectTransform> rocket,
        Canvas goal_box, Canvas goal_stone, Canvas goal_vase, RectTransform broken_block,
        List<RectTransform> cube_particles, List<RectTransform> obstacle_particles, List<RectTransform> rocket_particles)
    {
        //Debug.Log($"Button clicked! İndex: ({buttonName})");


        // Extract index
        string x = "";
        string y = "";
        int i = 0;
        while (i < buttonName.Length && buttonName[i] != ',')
        {
            x += buttonName[i];
            i++;
        }

        i++;

        while (i < buttonName.Length)
        {
            y += buttonName[i];
            i++;
        }

        if(x.Length == 0 || y.Length == 0){
            Debug.LogError("Indexes not taken properly.");
            return;
        }

        int x_int = int.Parse(x);
        int y_int = int.Parse(y);

        BreakingBlocks(x_int, y_int, levelData, grid2d, grid, cubes, cube_rocket_state, obstacles, tempParent, goals, moveCountText, rocket, goal_box, goal_stone, goal_vase, broken_block, cube_particles, obstacle_particles, rocket_particles);
    }

    void BreakingBlocks(int x, int y, 
        LevelData levelData, List<List<string>> grid2d, RectTransform grid, 
        List<RectTransform> cubes, List<RectTransform> cube_rocket_state, 
        List<RectTransform> obstacles, GameObject tempParent, List<int> goals, 
        TextMeshProUGUI moveCountText, List<RectTransform> rocket,
        Canvas goal_box, Canvas goal_stone, Canvas goal_vase, RectTransform broken_block,
        List<RectTransform> cube_particles, List<RectTransform> obstacle_particles, List<RectTransform> rocket_particles)
    {
        //Debug.Log("----BREAKING BLOCK DEBUG----");

        // copy
        List<List<string>> broken_grid2d = grid2d.Select(row => row.ToList()).ToList();

        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        int rows = grid2d.Count;
        int cols = grid2d[0].Count;

        
        if(grid2d[x][y].StartsWith("rocket")){
            int currentMoves = int.Parse(moveCountText.text); 
            currentMoves -= 1;
            moveCountText.text = currentMoves.ToString();

            // Double rocket
            for(int i = 0; i < 4; i++){
                int nx = x + dx[i];
                int ny = y + dy[i];
                if(nx < rows && 0 <= nx && ny < cols && 0 <= ny){
                    if(grid2d[nx][ny].StartsWith("rocket")){
                        // 'if' order is matter
                        if(rows > x+1){
                            for(int t = 0; t < cols; t++){
                                if(grid2d[x+1][t] == "v"){
                                    grid2d[x+1][t] = "v2";
                                }
                                else{
                                    int temp_x = x+1;
                                    broken_grid2d[temp_x][t] = grid2d[temp_x][t] + "_broken";
                                    for(int _x = temp_x+1; _x < rows; _x++){
                                        grid2d[temp_x][t] = grid2d[_x][t];
                                        temp_x = _x; 
                                    }
                                    grid2d[rows-1][t] = "rand";
                                }
                            }
                        }
                        for(int t = 0; t < cols; t++){
                            if(grid2d[x][t] == "v"){
                                grid2d[x][t] = "v2";
                            }
                            else{
                                int temp_x = x;
                                broken_grid2d[temp_x][t] = grid2d[temp_x][t] + "_broken";
                                for(int _x = temp_x+1; _x < rows; _x++){
                                    grid2d[temp_x][t] = grid2d[_x][t];
                                    temp_x = _x; 
                                }
                                grid2d[rows-1][t] = "rand";
                            }
                        }
                        if(0 <= x-1){
                            for(int t = 0; t < cols; t++){
                                if(grid2d[x-1][t] == "v"){
                                    grid2d[x-1][t] = "v2";
                                }
                                else{
                                    int temp_x = x-1;
                                    broken_grid2d[temp_x][t] = grid2d[temp_x][t] + "_broken";
                                    for(int _x = temp_x+1; _x < rows; _x++){
                                        grid2d[temp_x][t] = grid2d[_x][t];
                                        temp_x = _x; 
                                    }
                                    grid2d[rows-1][t] = "rand";
                                }
                            }
                        }

                        // For vertical blast, no need sliding
                        if(cols > y+1){
                            for(int t = 0; t < rows; t++){
                                if(grid2d[t][y+1] == "v"){
                                    grid2d[t][y+1] = "v2";
                                }
                                else{
                                    if(!broken_grid2d[t][y+1].EndsWith("_broken")) broken_grid2d[t][y+1] = grid2d[t][y+1] + "_broken";
                                    grid2d[t][y+1] = "rand";
                                }
                            }
                        }
                        for(int t = 0; t < rows; t++){
                            if(grid2d[t][y] == "v"){
                                grid2d[t][y] = "v2";
                            }
                            else{
                                if(!broken_grid2d[t][y].EndsWith("_broken")) broken_grid2d[t][y] = grid2d[t][y] + "_broken";
                                grid2d[t][y] = "rand";
                            }
                        }
                        if(0 <= y-1){
                            for(int t = 0; t < rows; t++){
                                if(grid2d[t][y-1] == "v"){
                                    grid2d[t][y-1] = "v2";
                                }
                                else{
                                    if(!broken_grid2d[t][y-1].EndsWith("_broken")) broken_grid2d[t][y-1] = grid2d[t][y-1] + "_broken";
                                    grid2d[t][y-1] = "rand";
                                }
                            }
                        }    
                    }
                }
            }

            if(grid2d[x][y] == "rocket_h"){
                for(int t = 0; t < cols; t++){
                    if(grid2d[x][t] == "v"){
                        grid2d[x][t] = "v2";
                    }
                    else{
                        int temp_x = x;
                        broken_grid2d[temp_x][t] = grid2d[temp_x][t] + "_broken";
                        for(int _x = temp_x+1; _x < rows; _x++){
                            grid2d[temp_x][t] = grid2d[_x][t];
                            temp_x = _x; 
                        }
                        grid2d[rows-1][t] = "rand";
                        
                        // dikey, yatay
                        //grid2d[x][t] = "rand";
                    }
                }
            }
            if(grid2d[x][y] == "rocket_v"){
                for(int t = 0; t < rows; t++){
                    if(grid2d[t][y] == "v"){
                        grid2d[t][y] = "v2";
                    }
                    else{
                        broken_grid2d[t][y] = grid2d[t][y] + "_broken";
                        grid2d[t][y] = "rand";
                    }
                }
            }
        }
        
        
        else if(grid2d[x][y].EndsWith("_rocket")){
            int currentMoves = int.Parse(moveCountText.text); 
            currentMoves -= 1;
            moveCountText.text = currentMoves.ToString();
            
            Debug.Log($"Clicked!: rocket block: ({x}, {y})");
            string rocket_color = grid2d[x][y];

            HashSet<(int, int)> visited = new HashSet<(int, int)>();

            
            List<(int, int)> cluster = new List<(int, int)>();
            Queue<(int, int)> queue = new Queue<(int, int)>();
            queue.Enqueue((x, y));
            visited.Add((x, y));

            while(queue.Count > 0)
            {
                var (ox, oy) = queue.Dequeue();
                Debug.Log($"Queued ox, oy: ({ox}, {oy})");
                cluster.Add((ox, oy));

                for(int d = 0; d < 4; d++){
                    int nx = ox + dx[d];
                    int ny = oy + dy[d];

                    if(nx >= 0 && nx < rows && ny >= 0 && ny < cols && 
                    grid2d[nx][ny] == rocket_color && !visited.Contains((nx, ny)))
                    {
                        queue.Enqueue((nx, ny));
                        visited.Add((nx, ny));
                    }
                }
            }

            System.Random random = new System.Random();
            int randomNumber = random.Next(0, 2);
            if(randomNumber == 0) {
                grid2d[x][y] = "rocket_h";
                broken_grid2d[x][y] = "rocket_h";
            }
            else {
                grid2d[x][y] = "rocket_v";
                broken_grid2d[x][y] = "rocket_v";
            }

            cluster.Sort((a, b) => b.Item1.CompareTo(a.Item1));
            for(int i = 0; i < cluster.Count; i++){
                var (_x, _y) = cluster[i];
                if(_x == x && _y == y) continue;
                broken_grid2d[_x][_y] = grid2d[_x][_y] + "_broken";

                Debug.Log($"({_x}, {_y})");
                
                for(int __x = _x; __x < rows; __x++){
                    grid2d[_x][_y] = grid2d[__x][_y];
                    _x = __x; 
                }
                grid2d[rows-1][_y] = "rand";
            }
            

            for(int i = 0; i < cluster.Count; i++){

                var (_x, _y) = cluster[i];
                for(int d = 0; d < 4; d++){
                    int nx = _x + dx[d];
                    int ny = _y + dy[d];

                    if(nx < rows && 0 <= nx && ny < cols && 0 <= ny){
                        if(grid2d[nx][ny] == "bo" || grid2d[nx][ny] == "s" || grid2d[nx][ny] == "v" || grid2d[nx][ny] == "v2"){
                            if(grid2d[nx][ny] == "bo" || grid2d[nx][ny] == "s" || grid2d[nx][ny] == "v2"){
                                broken_grid2d[nx][ny] = grid2d[nx][ny] + "_broken";

                                for(int __x = nx; __x < rows; __x++){
                                    grid2d[nx][ny] = grid2d[__x][ny];
                                    nx = __x; 
                                }
                                grid2d[rows-1][ny] = "rand";
                            }
                            else{
                                grid2d[nx][ny] = "v2";
                            }
                        }
                    }

                }
            }           
        }

        else if(grid2d[x][y] == "b" || grid2d[x][y] == "g" || grid2d[x][y] == "r" || grid2d[x][y] == "y")
        {
            int currentMoves = int.Parse(moveCountText.text); 
            currentMoves -= 1;
            moveCountText.text = currentMoves.ToString();

            Debug.Log($"Clicked!: normal block: ({x}, {y})");
            string block_color = grid2d[x][y];

            HashSet<(int, int)> visited = new HashSet<(int, int)>();

            List<(int, int)> cluster = new List<(int, int)>();
            Queue<(int, int)> queue = new Queue<(int, int)>();
            queue.Enqueue((x, y));
            visited.Add((x, y));

            while(queue.Count > 0){
                var (ox, oy) = queue.Dequeue();

                cluster.Add((ox,oy));

                for(int d = 0; d < 4; d++){
                    int nx = ox + dx[d];
                    int ny = oy + dy[d];

                    if(nx >= 0 && nx < rows && ny >= 0 && ny < cols && 
                    grid2d[nx][ny] == block_color && !visited.Contains((nx, ny)))
                    {
                        queue.Enqueue((nx, ny));
                        visited.Add((nx, ny));
                    }
                }
            }

            cluster.Sort((a, b) => b.Item1.CompareTo(a.Item1));
            for(int i = 0; i < cluster.Count; i++){
                var (_x, _y) = cluster[i];
                for(int d = 0; d < 4; d++){
                    int nx = _x + dx[d];
                    int ny = _y + dy[d];

                    if(nx < rows && 0 <= nx && ny < cols && 0 <= ny){
                        if(grid2d[nx][ny] == "bo" || grid2d[nx][ny] == "s" || grid2d[nx][ny] == "v" || grid2d[nx][ny] == "v2"){
                            if(grid2d[nx][ny] == "bo" || grid2d[nx][ny] == "s" || grid2d[nx][ny] == "v2"){
                                broken_grid2d[nx][ny] = grid2d[nx][ny] + "_broken";

                                for(int __x = nx; __x < rows; __x++){
                                    grid2d[nx][ny] = grid2d[__x][ny];
                                    nx = __x; 
                                }
                                grid2d[rows-1][ny] = "rand";
                            }
                            else{
                                grid2d[nx][ny] = "v2";
                            }
                        }
                    }

                }
            }

            for(int i = 0; i < cluster.Count; i++){
                var (_x, _y) = cluster[i];

                broken_grid2d[_x][_y] = grid2d[_x][_y] + "_broken";

                Debug.Log($"({_x}, {_y})");
                
                for(int __x = _x; __x < rows; __x++){
                    grid2d[_x][_y] = grid2d[__x][_y];
                    _x = __x; 
                }
                grid2d[rows-1][_y] = "rand";
            }

        }

        HashSet<(int, int)> sliding_blocks = new HashSet<(int, int)>();
        List<int> sliding_cols = Enumerable.Repeat(0, broken_grid2d[0].Count).ToList();
        
        for(int i = 0; i < broken_grid2d.Count; i++){
            for(int j = 0; j < broken_grid2d[0].Count; j++){
                // sliding_cols.Count > j && 
                if(broken_grid2d[i][j].EndsWith("_broken")){
                    for(int k = i; k < broken_grid2d.Count; k++){
                        sliding_blocks.Add((k, j));
                    }
                    sliding_cols[j] += 1;
                }
            }
        }
        
        

        /*
        Debug.Log("Printing broken_grid2d:");
        for (int i = 0; i < broken_grid2d.Count; i++)
        {
            string row = string.Join(", ", broken_grid2d[i]);
            Debug.Log($"Row {i}: {row}");
        */

        GameInitalizer gameInit = FindObjectOfType<GameInitalizer>();
        


        if(gameInit == null) Debug.LogError("gameInit is null! IMPORRRTANTTT");
        else Debug.LogError("gameInit is not null!");
        if (broken_grid2d == null) Debug.LogError("broken_grid2d is null!");
        else Debug.LogError("broken_grid2d is not null!");
        if (grid2d == null) Debug.LogError("grid2d is null!");
        else Debug.LogError("grid2d is not null!");
        if (levelData == null) Debug.LogError("levelData is null!");
        else Debug.LogError("levelData is not null!");



        if (gameInit != null && broken_grid2d != null && grid2d != null && levelData != null)
        {
            Debug.Log("Starting HandleGridUpdates coroutine...");
            StartCoroutine(HandleGridUpdates(levelData, grid2d, broken_grid2d, gameInit, goal_box, goal_stone, goal_vase, moveCountText, sliding_blocks, sliding_cols));

            //gameInit.BlockHandler(levelData, broken_grid2d);
            //gameInit.BlockHandler(levelData, grid2d);
            

        }
        else
        {
            Debug.LogError("GameInitalizer component not found in the scene!");
        }
    }

    IEnumerator HandleGridUpdates(LevelData levelData, List<List<string>> grid2d, List<List<string>> broken_grid2d, GameInitalizer gameInit,
        Canvas goal_box, Canvas goal_stone, Canvas goal_vase, TextMeshProUGUI moveCountText,
        HashSet<(int, int)> sliding_blocks, List<int> sliding_cols
    )
    {
        if (gameInit != null && broken_grid2d != null && grid2d != null && levelData != null)
        {
            Debug.Log("Starting HandleGridUpdates coroutine...");

            // Apply first changes
            gameInit.BlockHandler(levelData, broken_grid2d, sliding_blocks, sliding_cols);
            
            // Renderr
            yield return null;
            yield return new WaitForSeconds(0.4f);

            bool any_obstacle = ArrangeGoals(grid2d, goal_box, goal_stone, goal_vase);
            bool isFinish = IsGameFinish(any_obstacle, moveCountText.text);

            if(!isFinish) gameInit.BlockHandler(levelData, grid2d, sliding_blocks, sliding_cols); 
            /*
            if(isFinish){
                //grid2d = new List<List<string>>();
                StopAllCoroutines();
                yield break;
            }
            */
        }
    }


}