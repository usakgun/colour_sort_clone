# Colour Sort - 2D Puzzle Game (Unity 6)

A production-ready water sort puzzle game developed in **Unity 6 (2D URP)** using clean architecture principles, design patterns, and procedural asset/level generation.

## Key Features & Architecture

* **MVC / MVP Architecture:** Strict separation of concerns between Pure C# data models (BottleModel), game logic controllers (GameController, LevelManager), and visual components (BottleView, UIManager).
* **Command Pattern (Undo System):** Full move history and state rollback implementation using ICommand, PourCommand, and CommandInvoker without state duplication.
* **100% Solvable Procedural Level Generator:** Reverse-shuffle algorithm (LevelGenerator) that guarantees every procedurally generated level is solvable without dead-ends, combined with ScriptableObject support for hand-crafted levels.
* **Deadlock & Meaningful Move Detection:** Smart fail-state algorithm that detects both direct deadlocks and infinite ping-pong loops (HasAnyValidMove), triggering an Out-of-Moves recovery flow.
* **Dynamic Booster System:** In-game "+1 Extra Bottle" booster with real-time dynamic grid re-layout (UpdateBottlePositions).
* **Procedural Visuals & Game Feel (DOTween):**
  * Runtime procedural SDF texture generation (ProceduralSpriteGenerator) for anti-aliased test tubes, UI cards, and radial gradient backgrounds.
  * Liquid masking (SpriteMask), animated meniscus surface, pouring stream (LineRenderer), bubble particles, error shake feedback, and cork-stopper completion animations.

## Project Structure

* **Assets/Scripts/Commands:** Command Pattern implementation (ICommand, PourCommand, CommandInvoker)
* **Assets/Scripts/Controllers:** Core game loop, deadlock detection, and level management
* **Assets/Scripts/Core:** Procedural solvable level generator
* **Assets/Scripts/Models:** Pure C# data structures and ScriptableObject definitions
* **Assets/Scripts/Views:** DOTween animations, procedural sprite generation, and UI management

## Tech Stack

* **Engine:** Unity 6 (6000.x) - 2D Universal Render Pipeline (URP)
* **Language:** C#
* **Animation:** DOTween (Demigiant)