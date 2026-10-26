> This README was translated into English by AI and may contain inaccuracies.

[한국어(Korean) README](README.ko.md)

# Mario 2D Platform Game

A midterm exam project made with Unity.

## Controls

| Action | Input |
| --- | --- |
| Move | A / D or Left / Right Arrow |
| Jump | Space — hold for a higher jump |
| Shoot a fireball | Left Mouse Button while powered up |
| Enter the next stage | Up Arrow while touching the exit door |
| Retry after Game Over or final clear | R |

## Boss Battle

The boss patrols in the air, spins, and then charges toward the player. Colliding with a wall or the ground makes it fall and become stunned.

- A fire flower appears.
- Fireball attacks damage the boss only while it is stunned on the ground.
- Land 10 valid fireball hits to defeat the boss.

## How to Run

### Windows Build

1. Download the Build folder from GitHub or download a release, then run `Build/MIDTERM EXAM.exe`.

### Unity Editor

1. Open the project in Unity **6000.6.2f1**.
2. Open `Assets/Scenes/Stage1.unity`.
3. Enter Play Mode and click the Game view to enable keyboard and mouse input.

You can also open `Assets/Scenes/Stage2.unity` to test the second stage directly.

## Midterm Exam Checklist

- [✔] **1. Power-up: player grows by 20% and can shoot fireballs.**
- [✔] **2. Fireballs travel correctly and hit enemies / the boss.**
- [✔] **3. Flying enemy flies, detects the player, and attacks.**
- [✔] **4. Collisions remove the power-up or reduce health / lives.**
- [✔] **5. Checkpoint activates and saves the respawn position.**
- [✔] **6. Player respawns correctly after death.**
- [✔] **7. HUD displays lives, coins, score, and timer.**
- [✔] **8. Timer counts down from 180 seconds.**
- [✔] **9. Boss patrols within its designated area and attacks the player.**
- [✔] **10. Boss is defeated after 10 hits.**
- [✔] **11. Game Over and Stage Clear screens are implemented.**

## Gameplay Videos

- **Full Gameplay:** Link coming soon.
- **1-Minute Gameplay:** Link coming soon.

## Asset Credits

- **Sound:** [Interface Sounds](https://kenney.nl/assets/interface-sounds) by Kenney.
- **Platform assets:** [Pixel Platformer](https://kenney.nl/assets/pixel-platformer) and [Pixel Platformer Industrial Expansion](https://kenney.nl/assets/pixel-platformer-industrial-expansion) by Kenney.
