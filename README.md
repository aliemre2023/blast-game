# Blast Game 🟩 🟦 🟥 🟨 🚀
This is a simple puzzle game where players blast adjacent same-colored blocks to clear obstacles within a limited number of moves.
## Game Flow
1) Start with Level 1.
2) When the "Level 1" button is clicked, the game screen opens. Player can:
   - Click a block (blue, green, red, yellow) to blast adjacent blocks of the same color. If those blocks are adjacent to an obstacle (box, stone, vase), the obstacle's health decreases by one.
   - Click a rocket state block (when more than 3 adjacent blocks of the same color are grouped together) to generate a rocket at the clicked point, causing other blocks to be destroyed.
   - Click a rocket block to destroy a row and a column.
     -  If two rockets are adjacent, a 3row X 3col destruction is activated, and those blocks are destroyed. 
   - Click the "X" button and refresh the game and go back to the main screen.
3) If the player destroys all obstacles before the move count reaches zero, they win, and a victory screen appears. If the move count reaches zero while obstacles are still present, a defeat screen appears.
4) If a level is passed successfully, the next level becomes playable.
5) If all levels are finished, game restarts from "Level 1" (mod10).

Whole gameplay: <a href="https://www.youtube.com/shorts/N7tZ5WY1sQk">video</a><br>
Example screenshots given below respectively: <br>
<img height="250" alt="main1" src="https://github.com/user-attachments/assets/73124a95-332a-404d-a309-ee3e15d260a7" />
<img height="250" alt="game1" src="https://github.com/user-attachments/assets/516b8980-828c-4ca6-b9d1-01d2b5df7817" />
<img height="250" alt="blast1" src="https://github.com/user-attachments/assets/5dea6ca5-c712-495b-8f21-b817ef8f347e" />
<img height="250" alt="blast2" src="https://github.com/user-attachments/assets/ccc891fb-5af3-4c61-a8fa-90b7cf9db6b4" />
<img height="250" alt="blast3" src="https://github.com/user-attachments/assets/691f0c10-80a6-4f0e-96ec-f8a87d964c89" />
<img height="250" alt="gameyes" src="https://github.com/user-attachments/assets/1cb76890-397d-448d-9e10-48959c519a3e" />
<img height="250" alt="gameno" src="https://github.com/user-attachments/assets/f50bd252-da81-417e-93ba-39f5526a33cb" />
<img height="250" alt="main2" src="https://github.com/user-attachments/assets/d0b9f8c1-2485-45d4-ae33-953957708e52" />

## Pros of Project 
1) Learned Unity basics.
2) Learned C# tructure in Unity.
3) Learned how to create and arrange scenes in Unity.

## Cons of Project
1) A few animations were implemented due to limited time, making the project very simple currently.
2) Limited OOP principles were used due to my lack of knowledge about Unity and C#.

