# Myst3ry

A code-breaking game for Android (and Windows, iOS and macOS), built with .NET MAUI for the Mobile Application Development group project ("Memory Master" brief).

The app secretly picks a three-digit number in which every digit is different. The player keeps guessing; after each guess the app reports **hits** (right digit, right place) and **matches** (right digit, wrong place). Three hits wins, and the app shows how many guesses it took.

## Features

- Three difficulty levels. **Easy** colours each digit of a guess (green = hit, amber = match, grey = not in the code). **Normal** shows only the hit and match counts. **Hard** works like Normal with a 60-second countdown.
- Custom keypad that greys out digits already used (the code never repeats a digit) and a leading 0.
- Guess history with digit tiles and hit/match labels, plus plain-language hints.
- Timer, guess count and personal best on the Play screen. The clock starts on the first tap and pauses while another screen is open.
- Result card showing the code, the number of guesses and the time, with a Play Again button. Give Up and New Game buttons ask for confirmation first.
- Stats screen with played/won/lost totals, win rate, best/average/fastest and recent games. Games are saved on the device.
- How to Play screen that opens on first launch and from the **?** button.
- Light and dark themes, animations, haptic feedback and screen-reader support.

## Running the app

1. Install **Visual Studio 2022** with the **.NET Multi-platform App UI development** workload (.NET 9).
2. Open `Myst3ry.sln`.
3. Pick an Android emulator (the project was set up with *Pixel 7 – API 35*) or **Windows Machine**, then press **F5**.

If you pulled this version over an older copy, close Visual Studio first. Then use **Build > Clean Solution** before running, and uninstall the old app from the emulator if the icon or splash screen still looks old.

## Project structure

| Folder / file | Contents |
|---|---|
| `App.xaml(.cs)`, `AppShell.xaml(.cs)`, `MauiProgram.cs` | App start-up, Shell tab bar and routes, dependency injection |
| `Views/` | `GamePage`, `StatsPage`, `HelpPage` (XAML UI + small code-behind for events and animations) |
| `ViewModels/` | `GameViewModel` (game state, commands, timer), `StatsViewModel`, `ViewModelBase` |
| `Models/` | `GameModel` (code generation and scoring), `GuessResult`, `ScoreEntry`, enums |
| `Services/` | `ScoreService` (saves games with Preferences + JSON), `FeedbackService` (haptics) |
| `Converters/` | `BoolToColorConverter` |
| `Resources/Styles/` | `Colors.xaml` (palette), `AppStyles.xaml` (Myst3ry styles), `Styles.xaml` (template defaults) |
| `Resources/AppIcon`, `Resources/Splash`, `Resources/Images` | App icon, splash screen and tab/toolbar icons (SVG) |
| `Documentation/` | Project documentation, its generator, and the video script |

## Documentation and submission

`Documentation/Myst3ry_Project_Documentation.docx` follows the structure in Appendix A of the brief: cover page, table of contents, 1 Tutorial, 2 User Interface, 3 Specific Course Topics, 4 Self-Taught Aspects and 5 Code Listings.

Before submitting:

- [ ] Fill in the group number and each member's surname, first name and student number in the comment at the top of `App.xaml.cs`.
- [ ] Run the app and take the screenshots described in the dashed boxes in section 1, then paste each one into its box.
- [ ] Replace the first page with the completed and signed cover page.
- [ ] Fill in the group number in the footer.
- [ ] In section 4, delete any self-taught aspect that was actually covered in class.
- [ ] Remove all the yellow TO DO notes.
- [ ] Update the table of contents: right-click it, then **Update Field > Update entire table**.
- [ ] Export the document as a PDF (**File > Save As > PDF**).
- [ ] Record the video by following `Documentation/Video_Demo_Script.md`, then zip the video and the PDF together.

### Updating the code listings

The code listings in section 5 are read directly from the source files. If you change any code, regenerate the document so the listings stay correct. You need Node.js:

```
cd Documentation/tools
npm install
npm run build
```

This overwrites the `.docx`, so add the screenshots and cover page **after** the code is final.
