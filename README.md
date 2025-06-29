# 🎮 Multiplayer Quiz Game

This is the final delivery of the Multiplayer Quiz Game project.

## ✅ Project Highlights:
- Supports both Single Player and Multiplayer modes.
- Real-time synchronization using Photon PUN 2.
- Animated, interactive UI with DOTween.
- Fully functional Unity Editor Dashboard for question management.
- Backend integration with Supabase for dynamic question loading.
- Multiplayer sync barrier and proper result comparison with win/lose display.

## ✅ Repository Structure:
The project is fully organized inside the `Assets/_Project` folder.
All scripts, scenes, and dashboard tools are clearly separated.

## ✅ Dashboard Instructions:
1. Open the project in Unity 6000.0.27f1 LTS .
2. Go to **Quiz Game > Question Dashboard** from the top menu.
3. You can Add, Edit, Delete, and Fetch questions directly from the Supabase database.
4. The dashboard works outside Play Mode.
5. API configuration is available in `DashboardEditor.cs`.

## ✅ API Setup:
Make sure to insert your Supabase API Key and API URL in the following section: ( but i already added them there)
```csharp
private const string apiKey = "YOUR_API_KEY_HERE";
private const string apiUrl = "YOUR_API_URL_HERE";
