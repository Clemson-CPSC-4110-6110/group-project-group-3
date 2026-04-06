Group Members:
- Anthony Martino
- Ainsley Davis
- Luke Smith
- Daniel Choi
- Diana Sanchez

Milestone 2 ------------------------

Description:
Player elements for core interaction (voting and progress board) have been implemented into near complete prototypes. There are prefabs for the policy cards, interactable voting stations each with a yes and no button, a socket voting box where the policy card can be inserted, and a dynamic progress board that shows progress. The core gameplay of Secret Hitler is socially driven but the main mechanics are voting to enact government, the government chooses a policy card to be dropped in the dropbox, and the progressboard increments with the corresponding policy. It is difficult to test this independently but the prefab voting buttons are used to cast a vote. If the majority votes yes, the government is enacted resulting in the board advancing. To showcase this, the majority voting is what advances the board. In future development, the board will only advance on the chancellor dropping a policy card into the socket. 

Known Issues:
Our entire team has apple laptops and we havent been able to test on the headset yet. The voting button prefabs are troublesome with the xr interactor and were tested in the scene view by transforming the button down into the trigger area. 

Milestone 3 ------------------------

Built a centralized GameplayManager that handles the heavy lifting: shuffling roles, counting votes, and tracking the score on the board. Each "iPad" is in sync with the manager which handles identity assignment, role transitions, and votes for hte active govt. 

When a vote passes, the PolicyManager generates a random deck of three cards and letting the President/Chancellor filter them down until one is left. Once that card is picked, the manager updates the physical progress ticks on the board, resets the hasVoted indicators on everyone's station, and loops the whole thing back to the next election. The user is responsible for playing their assigned role, voting on the government, and "discarding" policies until the remaining one increments the game board. Repeat until one of either sides wins. 

Basic multiplayer networking foundation using Photon Fusion in shared mode was created. Each player is assigned proper ownership, ensuring they only control their own character. This system establishes the core infrastructure needed for real time interaction and will support future features such as voice communication, gameplay mechanics, and synchronized interactions.

Known Issues:
Inability to test on the headset. 