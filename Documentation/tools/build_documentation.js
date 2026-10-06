// Builds Documentation/Myst3ry_Project_Documentation.docx in the order required by Appendix A:
// cover page, table of contents, 1 Tutorial, 2 User Interface, 3 Specific Course Topics,
// 4 Self-Taught Aspects, 5 Code Listings (read straight from the source files).
//
// Usage (from this folder):  npm install   then   npm run build
// Re-run it whenever the code changes so the listings stay up to date.

const fs = require("fs");
const path = require("path");
const {
  AlignmentType, BorderStyle, Document, Footer, HeadingLevel, HeightRule, LevelFormat, Packer,
  PageNumber, Paragraph, ShadingType, Tab, TabStopType, Table, TableCell, TableOfContents,
  TableRow, TextRun, VerticalAlign, WidthType,
} = require("docx");

const ROOT = path.resolve(__dirname, "..", "..");
const OUT = path.resolve(__dirname, "..", "Myst3ry_Project_Documentation.docx");

// A4 with 2 cm margins
const PAGE = { width: 11906, height: 16838, margin: 1134 };
const CONTENT = PAGE.width - 2 * PAGE.margin; // 9638 DXA

const NAVY = "16213E";
// Yellow background for things the group still has to fill in (character shading, not "highlight",
// because docx-js writes a non-standard highlight element that Word may reject).
const MARK = { type: ShadingType.CLEAR, fill: "FFF200", color: "auto" };
const ACCENT = "E94560";
const LIGHT = "F4F5FA";
const GRID = "D0D4E0";

// ---------- small helpers ----------

// Turns "**bold**" and "`code`" inside a string into formatted runs.
function runs(text, base = {}) {
  return text.split(/(\*\*[^*]+\*\*|`[^`]+`)/).filter(Boolean).map((part) => {
    if (part.startsWith("**")) return new TextRun({ ...base, text: part.slice(2, -2), bold: true });
    if (part.startsWith("`")) return new TextRun({ ...base, text: part.slice(1, -1), font: "Consolas" });
    return new TextRun({ ...base, text: part });
  });
}

const p = (text, opts = {}) => new Paragraph({ ...opts, children: runs(text) });
const h1 = (text, opts = {}) => new Paragraph({ heading: HeadingLevel.HEADING_1, pageBreakBefore: true, ...opts, children: [new TextRun(text)] });
const h2 = (text) => new Paragraph({ heading: HeadingLevel.HEADING_2, children: [new TextRun(text)] });
const bullet = (text) => new Paragraph({ numbering: { reference: "bullets", level: 0 }, children: runs(text) });

let listInstance = 0;
function steps(items) {
  listInstance++;
  return items.map((t) => new Paragraph({ numbering: { reference: "steps", level: 0, instance: listInstance }, children: runs(t) }));
}

// Yellow-highlighted note for the group to act on (delete it afterwards).
const todo = (text) => new Paragraph({
  spacing: { before: 120, after: 120 },
  children: [new TextRun({ text: "TO DO: " + text, shading: MARK, bold: true })],
});

const cellBorders = {
  top: { style: BorderStyle.SINGLE, size: 4, color: GRID },
  bottom: { style: BorderStyle.SINGLE, size: 4, color: GRID },
  left: { style: BorderStyle.SINGLE, size: 4, color: GRID },
  right: { style: BorderStyle.SINGLE, size: 4, color: GRID },
};

function cell(content, width, { header = false, shade = null } = {}) {
  const paras = (Array.isArray(content) ? content : [content]).map((t) =>
    new Paragraph({
      spacing: { before: 0, after: 40 },
      children: runs(t, header ? { bold: true, color: "FFFFFF", size: 20 } : { size: 19 }),
    }));
  return new TableCell({
    width: { size: width, type: WidthType.DXA },
    borders: cellBorders,
    margins: { top: 70, bottom: 70, left: 100, right: 100 },
    shading: header ? { type: ShadingType.CLEAR, fill: NAVY, color: "auto" }
      : shade ? { type: ShadingType.CLEAR, fill: shade, color: "auto" } : undefined,
    children: paras,
  });
}

// Table with a navy header row and light zebra stripes. Widths are fractions of the page width.
// Returns an array (table + spacer paragraph), so spread it: ...table(...)
function table(headers, rows, fractions) {
  const widths = fractions.map((f) => Math.round(f * CONTENT));
  widths[widths.length - 1] = CONTENT - widths.slice(0, -1).reduce((a, b) => a + b, 0);
  return [
    new Table({
      width: { size: CONTENT, type: WidthType.DXA },
      columnWidths: widths,
      rows: [
        new TableRow({ tableHeader: true, children: headers.map((h, i) => cell(h, widths[i], { header: true })) }),
        ...rows.map((r, ri) => new TableRow({
          cantSplit: true,
          children: r.map((c, i) => cell(c, widths[i], { shade: ri % 2 ? LIGHT : null })),
        })),
      ],
    }),
    // Keeps the next table from merging into this one.
    new Paragraph({ spacing: { before: 0, after: 120 } }),
  ];
}

// Phone-shaped boxes where the group pastes real screenshots, followed by a figure caption.
let figure = 0;
function screenshots(shots) {
  const box = 2700; // ~1.9 inches wide
  const gap = 300;
  const cells = [];
  const widths = [];
  shots.forEach((s, i) => {
    if (i > 0) { widths.push(gap); cells.push(new TableCell({ width: { size: gap, type: WidthType.DXA }, borders: { top: none, bottom: none, left: none, right: none }, children: [new Paragraph({ keepNext: true })] })); }
    widths.push(box);
    cells.push(new TableCell({
      width: { size: box, type: WidthType.DXA },
      verticalAlign: VerticalAlign.CENTER,
      shading: { type: ShadingType.CLEAR, fill: "EEF0F6", color: "auto" },
      borders: {
        top: { style: BorderStyle.DASHED, size: 8, color: "9AA0B4" },
        bottom: { style: BorderStyle.DASHED, size: 8, color: "9AA0B4" },
        left: { style: BorderStyle.DASHED, size: 8, color: "9AA0B4" },
        right: { style: BorderStyle.DASHED, size: 8, color: "9AA0B4" },
      },
      margins: { left: 120, right: 120 },
      children: [
        // keepNext keeps the box on the same page as its caption
        new Paragraph({ alignment: AlignmentType.CENTER, keepNext: true, children: [new TextRun({ text: "SCREENSHOT", bold: true, color: "6B7089", size: 18 })] }),
        new Paragraph({ alignment: AlignmentType.CENTER, keepNext: true, children: [new TextRun({ text: s.hint, color: "6B7089", size: 17, shading: MARK })] }),
      ],
    }));
  });
  const total = widths.reduce((a, b) => a + b, 0);
  const captions = shots.map((s) => {
    figure++;
    return `Figure ${figure}: ${s.caption}`;
  });
  return [
    new Table({
      alignment: AlignmentType.CENTER,
      width: { size: total, type: WidthType.DXA },
      columnWidths: widths,
      borders: noBorders,
      rows: [new TableRow({ cantSplit: true, height: { value: 5400, rule: HeightRule.ATLEAST }, children: cells })],
    }),
    new Paragraph({
      alignment: AlignmentType.CENTER,
      spacing: { before: 80, after: 200 },
      children: [new TextRun({ text: captions.join("     "), italics: true, size: 18, color: "5D6480" })],
    }),
  ];
}

const none = { style: BorderStyle.NONE, size: 0, color: "FFFFFF" };
const noBorders = { top: none, bottom: none, left: none, right: none, insideHorizontal: none, insideVertical: none };

// ---------- content ----------

const coverPage = [
  new Paragraph({ spacing: { before: 3000 } }),
  new Table({
    width: { size: CONTENT, type: WidthType.DXA },
    columnWidths: [CONTENT],
    rows: [new TableRow({
      height: { value: 5200, rule: HeightRule.ATLEAST },
      children: [new TableCell({
        width: { size: CONTENT, type: WidthType.DXA },
        verticalAlign: VerticalAlign.CENTER,
        shading: { type: ShadingType.CLEAR, fill: LIGHT, color: "auto" },
        borders: {
          top: { style: BorderStyle.DASHED, size: 12, color: ACCENT },
          bottom: { style: BorderStyle.DASHED, size: 12, color: ACCENT },
          left: { style: BorderStyle.DASHED, size: 12, color: ACCENT },
          right: { style: BorderStyle.DASHED, size: 12, color: ACCENT },
        },
        children: [
          new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 200 }, children: [new TextRun({ text: "COVER PAGE", bold: true, size: 40, color: NAVY })] }),
          new Paragraph({ alignment: AlignmentType.CENTER, children: [new TextRun({ text: "Replace this page with the completed and signed cover page", size: 24, shading: MARK })] }),
          new Paragraph({ alignment: AlignmentType.CENTER, children: [new TextRun({ text: "provided by the lecturer. It must be the first page of the PDF.", size: 24, shading: MARK })] }),
        ],
      })],
    })],
  }),
];

const contentsPage = [
  new Paragraph({ pageBreakBefore: true, alignment: AlignmentType.LEFT, spacing: { after: 120 }, children: [new TextRun({ text: "Myst3ry", bold: true, size: 48, color: NAVY })] }),
  new Paragraph({ spacing: { after: 360 }, children: [new TextRun({ text: "A .NET MAUI code-breaking game: project documentation", size: 26, color: "5D6480" })] }),
  new Paragraph({ spacing: { after: 160 }, children: [new TextRun({ text: "Table of Contents", bold: true, size: 30, color: NAVY })] }),
  new TableOfContents("Table of Contents", { hyperlink: true, headingStyleRange: "1-2" }),
  todo("In Word, right-click the table of contents and choose Update Field > Update entire table after adding the screenshots, so the page numbers are correct."),
];

const tutorial = [
  h1("1. Tutorial"),
  p("**Myst3ry** is a code-breaking game built with .NET MAUI. The app secretly picks a three-digit number in which every digit is different. You keep guessing until you have all three digits in the correct order, and after every guess the app tells you how many **hits** and **matches** you got. This tutorial walks through every screen of the app."),
  h2("1.1 The requirements at a glance"),
  ...table(["Requirement from the brief", "Where you can see it in the app"], [
    ["The computer secretly selects a random three-digit number where each digit is different.", "A new code is generated for every game (section 1.3). Codes run from 102 to 987 and never repeat a digit."],
    ["After each guess, show the number of hits and matches.", "Each guess appears in the guess history with a green hits label and an amber matches label (section 1.5)."],
    ["The player wins with three hits.", "The result card \"Code cracked!\" appears (section 1.7)."],
    ["At the end of the game, display the number of guesses.", "The result card shows the number of guesses in large type, with the time taken (section 1.7)."],
    ["Keep guessing until the code is found, or quit.", "The Give Up button ends the game and reveals the code (section 1.8)."],
    ["The player can play repeatedly.", "Play Again on the result card, or New Game at any time (sections 1.7 and 1.8)."],
  ], [0.45, 0.55]),

  h2("1.2 Starting Myst3ry"),
  ...steps([
    "Tap the **Myst3ry** icon (a red padlock on navy) on the phone's home screen.",
    "A navy splash screen with the padlock appears while the app loads.",
    "The first time the app runs, the **How to Play** screen opens automatically (section 1.10). Read it and tap **Got it, let's play!** to go to the Play screen.",
  ]),
  ...screenshots([
    { hint: "Home screen showing the Myst3ry app icon", caption: "The app icon" },
    { hint: "The splash screen while the app starts", caption: "Splash screen" },
  ]),

  h2("1.3 The Play screen"),
  p("The Play screen is where the game happens. Its parts are, from top to bottom:"),
  ...table(["", "Part", "What it does"], [
    ["A", "Title bar and **?** button", "Shows the app name. The **?** button opens How to Play."],
    ["B", "Difficulty selector", "**Easy**, **Normal** or **Hard ⏱**. The selected level is red, with a one-line description underneath."],
    ["C", "Information chips", "**TIME** (or **TIME LEFT** on Hard), the number of **GUESSES** so far, and your **BEST** (fewest guesses) on this difficulty."],
    ["D", "Guess history", "Every guess with its clues, newest at the top."],
    ["E", "Guess slots", "The three digits you are typing. The next empty slot has a red outline."],
    ["F", "Status message", "Tells you what to do next, or what your last guess revealed."],
    ["G", "Keypad", "Digits 0 to 9, **⌫** to delete and **✓** to submit."],
    ["H", "New Game and Give Up", "Start again, or end the game and reveal the code."],
    ["I", "Tab bar", "Switch between the **Play** and **Stats** screens."],
  ], [0.06, 0.28, 0.66]),
  ...screenshots([{ hint: "Play screen at the start of a Normal game. Label the parts A to I", caption: "The Play screen" }]),

  h2("1.4 Making a guess"),
  ...steps([
    "Tap three digits on the keypad. They appear in the guess slots. The clock starts with your first tap.",
    "Each digit you use is greyed out until you submit, because the secret code never repeats a digit. **0** is greyed out for the first digit because the code is a three-digit number.",
    "Made a mistake? Tap **⌫** to remove the last digit.",
    "When all three slots are full, the green **✓** button lights up. Tap it to submit your guess.",
    "If you enter a guess you have already tried, the slots shake, a message explains why, and the guess is not counted.",
  ]),
  ...screenshots([
    { hint: "Two digits typed: used digits greyed out, third slot outlined in red", caption: "Typing a guess" },
    { hint: "The slots shaking after a repeated guess, with the message", caption: "A repeated guess is rejected" },
  ]),

  h2("1.5 Reading the clues"),
  p("After each guess a new row appears at the top of the guess history. It shows the guess number, the three digits you guessed and two labels:"),
  bullet("The green **hits** label: how many digits are correct **and** in the correct place."),
  bullet("The amber **matches** label: how many digits are correct but in the **wrong** place."),
  p("**Example from the brief:** if the secret code is **586** and you guess **856**, the row shows **1 hit** and **2 matches**: the 6 is in the right place, and the 8 and 5 are in the code but swapped."),
  p("The status message under the slots adds a hint. If a guess has no hits and no matches it reminds you that none of those digits are in the code, and if all three digits are right but in the wrong order it says only the order is left to find."),
  ...screenshots([{ hint: "Guess history after three or four guesses on Normal", caption: "Guess history with hits and matches" }]),

  h2("1.6 Choosing a difficulty"),
  p("Tap **Easy**, **Normal** or **Hard ⏱** at the top of the Play screen. Changing the level starts a new game, so if you have already made a guess the app asks you to confirm first."),
  ...table(["Level", "Clues you get", "Clock"], [
    ["Easy", "The hit and match counts, **plus** each digit of the guess is coloured: green = hit, amber = match, grey = not in the code.", "Counts up, no limit"],
    ["Normal", "Only the number of hits and matches.", "Counts up, no limit"],
    ["Hard", "Only the number of hits and matches.", "Counts **down** from 60 seconds. It turns red and pulses in the last 10 seconds. At 0 the game is lost."],
  ], [0.14, 0.56, 0.30]),
  ...screenshots([
    { hint: "Easy game: history with green, amber and grey digit tiles", caption: "Easy mode colours each digit" },
    { hint: "Hard game with under 10 seconds left (red clock)", caption: "Hard mode countdown" },
  ]),

  h2("1.7 Winning the game"),
  p("When a guess gets **3 hits**, the keypad is replaced by a result card that shows:"),
  bullet("**🎉 Code cracked!**"),
  bullet("The secret code in green tiles."),
  bullet("The **number of guesses** the game took, and the time taken."),
  bullet("**🏆 New personal best!** if you beat your best score on that difficulty."),
  p("Tap **Play Again** to start a new game with a new code. You can play as many games as you like."),
  ...screenshots([{ hint: "Result card after a win", caption: "The result card after a win" }]),

  h2("1.8 Giving up, running out of time and starting over"),
  bullet("**Give Up:** tap **Give Up** and confirm. The game ends, the code is revealed and the game counts as a loss."),
  bullet("**Out of time (Hard):** when the countdown reaches 0, a **⏰ Time's up!** card shows the code."),
  bullet("**New Game:** tap **New Game** at any time. If you have already made guesses, the app asks before throwing the game away."),
  ...screenshots([
    { hint: "The Give up? confirmation dialog", caption: "Confirming Give Up" },
    { hint: "Result card after giving up or running out of time", caption: "The result card after a loss" },
  ]),

  h2("1.9 The Stats screen"),
  p("Tap **Stats** in the tab bar to see your progress. Games are saved on the phone, so your statistics are still there when you reopen the app."),
  bullet("**Played / Won / Lost** totals and a **win rate** bar."),
  bullet("**Best** (fewest guesses), **average** guesses and **fastest** win."),
  bullet("Your last 10 games: ✅ won, 🏳️ gave up or ⏰ ran out of time, with the difficulty, time and date."),
  bullet("**Clear History** deletes all saved games after you confirm."),
  ...screenshots([
    { hint: "Stats screen after a few games", caption: "The Stats screen" },
    { hint: "The Clear history? confirmation dialog", caption: "Clearing the history" },
  ]),

  h2("1.10 The How to Play screen"),
  p("How to Play opens automatically the first time the app runs. To open it again, tap the **?** button at the top right of the Play screen. It explains the rules with a worked example and the Easy colour legend. Tap **Got it, let's play!** (or the back arrow) to return. The game clock pauses while you are on this screen or on the Stats screen."),
  ...screenshots([{ hint: "The How to Play screen", caption: "How to Play" }]),

  h2("1.11 Light and dark mode"),
  p("Myst3ry follows the phone's light or dark theme setting automatically. Every screen has colours designed for both."),
  ...screenshots([
    { hint: "Play screen in light mode, mid-game", caption: "Light mode" },
    { hint: "Same screen in dark mode", caption: "Dark mode" },
  ]),
];

const userInterface = [
  h1("2. User Interface"),
  p("**Usability** is how well users can reach their goals with a product **effectively** (they succeed), **efficiently** (with little effort and time) and with **satisfaction** (ISO 9241-11). Jakob Nielsen adds learnability, memorability and few, recoverable errors. For Myst3ry this means the rules are quick to learn, guesses are fast to enter, mistakes are prevented, the player always knows what is happening, and the app is pleasant to use. The table lists the main features that promote usability."),
  ...table(["UI aspect", "Where", "Why it promotes usability"], [
    ["Large custom keypad (52-point buttons) instead of the phone keyboard", "Play screen", "Big, well-spaced targets are fast to tap and hard to miss. Only valid keys exist, so no letters can be typed (efficiency, fewer errors)."],
    ["Used digits and a leading 0 are greyed out; ✓ only works with 3 digits", "Keypad", "Prevents invalid guesses before they happen instead of showing an error afterwards (error prevention)."],
    ["Next empty slot outlined in red", "Guess slots", "The player always sees where the next digit will go (visibility of system status)."],
    ["Plain-language status message after every guess", "Under the slots", "Immediate feedback in words, with hints such as \"none of those digits are in the code\" (feedback, learnability)."],
    ["Hits and matches shown as coloured labels with text", "Guess history", "Colour makes clues quick to scan; the words mean they don't rely on colour alone (accessibility)."],
    ["Guess history, newest first and numbered", "Play screen", "Earlier clues stay visible, so players don't have to remember them (recognition rather than recall)."],
    ["Timer, guesses and best-score chips", "Play screen", "Key game status is visible at a glance and the best score motivates replay (status, satisfaction)."],
    ["Hard-mode clock turns red and pulses in the last 10 seconds", "Timer chip", "Warns the player before time runs out, so the loss is not a surprise (feedback)."],
    ["Difficulty as three visible buttons with a description", "Top of Play screen", "All options are visible in one tap, and the description explains what each level changes (visibility, learnability)."],
    ["Confirmation before New Game, difficulty change, Give Up and Clear History", "Dialogs", "Destructive actions can't happen by accident (error prevention, user control)."],
    ["Repeated guesses rejected with a shake and message", "Guess slots", "Stops wasted guesses and explains why (error recovery)."],
    ["Result card with code, guesses, time and Play Again", "End of game", "Clear closure of the game and a one-tap way to play again (efficiency, satisfaction)."],
    ["How to Play on first launch and the ? button", "Help screen", "New players learn the rules straight away and can check them at any time (help and documentation)."],
    ["Consistent colours, card style and buttons on every screen", "Whole app", "Things that look the same behave the same, so the app is quick to learn (consistency)."],
    ["Bottom tab bar with icons", "Whole app", "A familiar mobile navigation pattern; both main screens are one tap away (match with platform conventions)."],
    ["Light and dark themes", "Whole app", "Comfortable to read in any lighting and follows the user's system setting (satisfaction)."],
    ["Haptic feedback on key presses and results", "Keypad, results", "The player feels each tap register, even without looking (feedback)."],
    ["Screen-reader descriptions and announcements", "Keypad, history, results", "Blind and low-vision players can play with TalkBack or VoiceOver (accessibility)."],
    ["Clock starts on the first tap and pauses off-screen", "Timer", "Reading the rules or checking stats is never counted against the player (fairness, user control)."],
  ], [0.36, 0.17, 0.47]),
];

const courseTopics = [
  h1("3. Specific Course Topics"),
  h2("3.1 Timers"),
  ...table(["Where", "What we used it for"], [
    ["`GameViewModel.cs`: field `_timer`, created in the constructor with `Application.Current.Dispatcher.CreateTimer()`, `Interval` = 1 second, `IsRepeating` = true",
      "The game clock. Each tick adds one second to the time taken."],
    ["`GameViewModel.OnTimerTick` (the `Tick` event handler) and `UpdateTimerDisplay()`",
      "Updates the TIME chip: counting up on Easy and Normal, and counting down from 60 seconds on Hard. Sets `IsTimeLow` in the last 10 seconds, which turns the clock red."],
    ["`GameViewModel.OnTimerTick`",
      "On Hard, ends the game with a loss when the 60 seconds are used up."],
    ["`OnDigitPressed` (start), `PauseClock()` / `ResumeClock()` called from `GamePage.OnDisappearing` / `OnAppearing`, and `EndGame()` (stop)",
      "The clock only starts on the player's first tap, pauses while another screen is shown, and stops when the game ends. The final time is saved with the score."],
  ], [0.55, 0.45]),
  h2("3.2 Event handlers"),
  ...table(["Event handler", "Where", "What it does"], [
    ["`OnDifficultyClicked` (Button.Clicked)", "GamePage.xaml.cs", "Changes difficulty; asks for confirmation if a game is in progress."],
    ["`OnNewGameClicked` (Button.Clicked)", "GamePage.xaml.cs", "Starts a new game, confirming first if guesses were made."],
    ["`OnGiveUpClicked` (Button.Clicked)", "GamePage.xaml.cs", "Confirms, then ends the game and reveals the code."],
    ["`OnHelpClicked` (ToolbarItem.Clicked)", "GamePage.xaml.cs", "Navigates to the How to Play screen."],
    ["`OnGotItClicked` (Button.Clicked)", "HelpPage.xaml.cs", "Goes back to the previous screen."],
    ["`OnClearClicked` (Button.Clicked)", "StatsPage.xaml.cs", "Confirms, then deletes all saved games."],
    ["`OnTimerTick` (IDispatcherTimer.Tick)", "GameViewModel.cs", "Runs every second to update the clock (section 3.1)."],
    ["`OnGuessRejected`, `OnGuessScored`, `OnGameEnded` (our own view-model events)", "GamePage.xaml.cs", "Shake the slots, scroll the history to the newest guess, animate the result card and announce the result to screen readers."],
    ["`OnViewModelPropertyChanged` (INotifyPropertyChanged.PropertyChanged)", "GamePage.xaml.cs", "Pulses the clock each second when time is low on Hard."],
    ["`OnAppearing` / `OnDisappearing` (page life-cycle)", "GamePage.xaml.cs, StatsPage.xaml.cs", "Resume or pause the clock, refresh the best score, show help on first launch, and reload the statistics."],
  ], [0.36, 0.22, 0.42]),
];

const selfTaught = [
  h1("4. Self-Taught Aspects"),
  todo("Delete any row below that was covered in class, so that only genuinely self-taught aspects are listed."),
  ...table(["Aspect", "Where it was incorporated", "Why"], [
    ["MVVM pattern (Model-View-ViewModel)", "`Models/`, `ViewModels/`, `Views/` folders; `ViewModelBase` implements `INotifyPropertyChanged`", "Keeps game rules, screen state and XAML separate, so the code is modular and easier to change."],
    ["Data binding with compiled bindings (`x:DataType`)", "All pages bind to their view model", "The UI updates itself when data changes, and binding mistakes are caught when building."],
    ["Commands with CanExecute (`ICommand`, `Command<T>`)", "Keypad in `GamePage.xaml`, `GameViewModel`", "Keys enable and disable themselves automatically (used digits, ✓ only with 3 digits)."],
    ["Dependency injection", "`MauiProgram.cs`; page and view-model constructors", "Shell creates each page with its view model and services, so classes don't create their own dependencies."],
    ["Shell navigation: tab bar and registered routes", "`AppShell.xaml`, `AppShell.xaml.cs`, `GoToAsync`", "Two main tabs plus a How to Play page pushed on top, with a Back button."],
    ["CollectionView and BindableLayout with item templates and empty views", "Guess history, digit tiles, recent games", "Lists that build themselves from data, with a friendly message when they are empty."],
    ["Data triggers and style triggers", "`AppStyles.xaml` (tile colours by clue), `GamePage.xaml` (selected difficulty, active slot, red clock)", "Changes the look of controls from data, in XAML only."],
    ["Visual State Manager", "Button styles in `AppStyles.xaml`", "Buttons shrink slightly when pressed and fade when disabled."],
    ["Value converter (`IValueConverter`)", "`BoolToColorConverter`: green or red result title", "Turns a true/false value into a colour in XAML."],
    ["Resource dictionaries, styles, `AppThemeBinding`, `OnPlatform`", "`Colors.xaml`, `AppStyles.xaml`, `Styles.xaml`", "One palette and style set for the whole app; automatic light and dark themes; a monospaced timer font on each platform."],
    ["Saving data with Preferences and JSON", "`ScoreService` (`IPreferences`, `System.Text.Json`)", "Games and statistics survive closing the app."],
    ["Animations", "`GamePage.xaml.cs`: `TranslateTo`, `ScaleTo`, `FadeTo`", "Shake on a rejected guess, pop-in result card, pulsing clock; gives lively feedback."],
    ["Haptic feedback", "`FeedbackService` (`HapticFeedback`), VIBRATE permission in `AndroidManifest.xml`", "The player feels key presses and results."],
    ["Accessibility", "`SemanticProperties` in XAML, `SemanticScreenReader.Announce`", "Works with TalkBack and VoiceOver."],
    ["Custom events (`event EventHandler`)", "`GameViewModel`: `GuessRejected`, `GuessScored`, `GameEnded`", "Lets the view model tell the page to animate without knowing about the UI."],
    ["Custom app icon, splash screen and tab icons (SVG)", "`Resources/AppIcon`, `Resources/Splash`, `Resources/Images`", "A recognisable, professional look on the home screen and in the app."],
  ], [0.27, 0.36, 0.37]),
];

// ---------- code listings ----------

const listingGroups = [
  { title: "a) App.xaml", files: ["App.xaml"] },
  { title: "b) App.xaml.cs", files: ["App.xaml.cs"] },
  { title: "c) AppShell.xaml", files: ["AppShell.xaml"] },
  { title: "d) AppShell.xaml.cs", files: ["AppShell.xaml.cs"] },
  {
    title: "e) Pages (XAML, then C#)", files: [
      "Views/GamePage.xaml", "Views/GamePage.xaml.cs",
      "Views/StatsPage.xaml", "Views/StatsPage.xaml.cs",
      "Views/HelpPage.xaml", "Views/HelpPage.xaml.cs",
    ],
  },
  {
    title: "f) Supporting files", files: [
      "MauiProgram.cs",
      "ViewModels/ViewModelBase.cs", "ViewModels/GameViewModel.cs", "ViewModels/StatsViewModel.cs",
      "Models/GameModel.cs", "Models/Difficulty.cs", "Models/GameOutcome.cs",
      "Models/DigitFeedback.cs", "Models/GuessResult.cs", "Models/ScoreEntry.cs",
      "Services/ScoreService.cs", "Services/FeedbackService.cs",
      "Converters/BoolToColorConverter.cs",
      "Resources/Styles/AppStyles.xaml", "Resources/Styles/Colors.xaml", "Resources/Styles/Styles.xaml",
    ],
  },
];

// One code line. Indentation is done with paragraph indents so that a long line which wraps
// continues a little to the right of where it started, instead of at the left margin.
const CHAR = 88; // width of one Consolas 8 pt character in twips
function codeLine(line) {
  const indent = line.length - line.trimStart().length;
  return new Paragraph({
    style: "Code",
    indent: { left: (indent + 4) * CHAR, hanging: 4 * CHAR },
    children: [new TextRun(line.trimStart())],
  });
}

function listing(relPath) {
  const text = fs.readFileSync(path.join(ROOT, relPath), "utf8").replace(/^﻿/, "").replace(/\t/g, "    ").replace(/\r/g, "");
  const lines = text.replace(/\n+$/, "").split("\n");
  return [
    new Paragraph({
      heading: HeadingLevel.HEADING_2,
      pageBreakBefore: true,
      spacing: { after: 40 },
      children: [new TextRun({ text: path.basename(relPath), size: 44 })],
    }),
    new Paragraph({
      spacing: { after: 160 },
      border: { bottom: { style: BorderStyle.SINGLE, size: 8, color: ACCENT, space: 4 } },
      children: [new TextRun({ text: relPath, size: 18, color: "5D6480" })],
    }),
    ...lines.map(codeLine),
  ];
}

const codeListings = [
  h1("5. Code Listings"),
  p("Each file starts on a new page with its name at the top. The required files come first (App.xaml, App.xaml.cs, AppShell.xaml, AppShell.xaml.cs, then the XAML and C# file of each page), followed by the supporting C# and XAML files. Files in the `Platforms` folder are the unchanged .NET MAUI template start-up code and are not listed."),
  ...listingGroups.map((g) => p(`**${g.title}:** ${g.files.map((f) => path.basename(f)).join(", ")}`)),
  ...listingGroups.flatMap((g) => g.files.flatMap(listing)),
];

// ---------- document ----------

const doc = new Document({
  creator: "Myst3ry group",
  title: "Myst3ry Project Documentation",
  description: "Tutorial, usability, course topics, self-taught aspects and code listings for the Myst3ry .NET MAUI app",
  features: { updateFields: true },
  styles: {
    default: { document: { run: { font: "Calibri", size: 22 }, paragraph: { spacing: { after: 120, line: 264 } } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 34, bold: true, color: NAVY }, paragraph: { spacing: { before: 0, after: 200 }, outlineLevel: 0, keepNext: true } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 26, bold: true, color: NAVY }, paragraph: { spacing: { before: 280, after: 120 }, outlineLevel: 1, keepNext: true } },
      { id: "Code", name: "Code", basedOn: "Normal", quickFormat: true,
        run: { font: "Consolas", size: 16 }, paragraph: { spacing: { before: 0, after: 0, line: 228 } } },
    ],
  },
  numbering: {
    config: [
      { reference: "bullets", levels: [{ level: 0, format: LevelFormat.BULLET, text: "•", alignment: AlignmentType.LEFT,
        style: { paragraph: { indent: { left: 540, hanging: 270 } } } }] },
      { reference: "steps", levels: [{ level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT,
        style: { paragraph: { indent: { left: 540, hanging: 340 } } } }] },
    ],
  },
  sections: [{
    properties: { page: { size: { width: PAGE.width, height: PAGE.height }, margin: { top: PAGE.margin, bottom: PAGE.margin, left: PAGE.margin, right: PAGE.margin } } },
    footers: {
      default: new Footer({
        children: [new Paragraph({
          tabStops: [{ type: TabStopType.RIGHT, position: CONTENT }],
          border: { top: { style: BorderStyle.SINGLE, size: 4, color: GRID, space: 4 } },
          children: [
            new TextRun({ text: "Myst3ry  |  Group ", size: 16, color: "5D6480" }),
            new TextRun({ text: "[number]", size: 16, color: "5D6480", shading: MARK }),
            new TextRun({ children: [new Tab(), "Page ", PageNumber.CURRENT], size: 16, color: "5D6480" }),
          ],
        })],
      }),
    },
    children: [...coverPage, ...contentsPage, ...tutorial, ...userInterface, ...courseTopics, ...selfTaught, ...codeListings],
  }],
});

Packer.toBuffer(doc).then((buffer) => {
  fs.writeFileSync(OUT, buffer);
  console.log(`Wrote ${path.relative(ROOT, OUT)} (${figure} screenshot placeholders, ${listingGroups.reduce((n, g) => n + g.files.length, 0)} code listings)`);
});
