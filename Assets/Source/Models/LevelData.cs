using System;
using System.Collections.Generic;

[Serializable]  // Required for Unity JSON serialization
public class LevelData
{
    public int level_number { get; set; }
    public int grid_width { get; set; }
    public int grid_height { get; set; }
    public int move_count { get; set; }
    public List<string> grid { get; set; }  // Keep as List<string> if necessary
}
