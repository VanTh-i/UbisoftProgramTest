<h1 align="center">🥚 Multiplayer Egg Grabber (Server-Authoritative Simulation)</h1>

<p align="center">
  <img src="https://img.shields.io/badge/Engine-Unity-black?logo=unity"/>
  <img src="https://img.shields.io/badge/Language-C%23-blue"/>
  <img src="https://img.shields.io/badge/Architecture-Client--Server-green"/>
  <img src="https://img.shields.io/badge/Focus-Networking-orange"/>
</p>


## 🎮 Overview

This is a Unity prototype I built to practice and demonstrate my understanding of **multiplayer game architecture**, specifically how to handle **high network latency**.

The game is simple: 1 Local Player and "x" AI Bots compete to collect as many eggs as possible on a grid within "x" seconds. However, the real challenge I tackled here was the Networking Simulation. The server is intentionally designed to be slow and laggy (updating every 1 to 5 seconds, plus an adjustable network delay). My goal was to make the gameplay feel smooth and real-time on the client side despite the terrible network conditions.

---

## 🚀 How to Run the Project

- Open the project in Unity.

- Open the MainGame scene.

- In Hierarchy, open NetworkSimulate/SERVER to config player and UI/UIManager to config duration.

- Hit the Play button.

- Use W, A, S, D or Arrow Keys to move the green character.

The Latency Test: While playing, drag the Simulated Latency slider at the bottom right up to 2-5 seconds. Notice how the movement remains perfectly smooth, but the scoreboard updates are delayed.

---

## 🛠️ Tech Stack
- Engine: Unity 3D (C#), Unity Version: 2022.3.62f3

- Design Pattern: Client-Server Architecture, Singleton (for Network Simulator & Managers).

- Algorithms: A* (A-Star) Pathfinding on a Grid system.
